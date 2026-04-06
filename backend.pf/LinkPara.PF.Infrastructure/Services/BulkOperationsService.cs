using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Transactions;
using ClosedXML.Excel;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.HttpProviders.Location;
using LinkPara.HttpProviders.Location.Models;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.IdentityModels;
using LinkPara.PF.Application.Commons.Models.IKS;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.Boa.Merchants;
using LinkPara.PF.Application.Features.Boa.Merchants.Command.CreateBoaMerchant;
using LinkPara.PF.Application.Features.BulkOperations.Merchants;
using LinkPara.PF.Application.Features.BulkOperations.Merchants.Command.BulkImportMerchant;
using LinkPara.PF.Application.Features.BulkOperations.Merchants.Command.BulkMerchantExcelValidation;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class BulkOperationsService : IBulkOperationsService
{
    private readonly ILogger<BulkOperationsService> _logger;
    private readonly PfDbContext _context;
    private readonly IRoleService _roleService;
    private readonly ILocationService _locationService;
    private readonly IParameterService _parameterService;
    private readonly IMerchantPoolService _merchantPoolService;
    private readonly IApiKeyGenerator _apiKeyGenerator;
    private readonly IAccountingService _accountingService;
    private readonly ICustomerService _customerService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IUserService _userService;
    private readonly IBus _bus;

    public BulkOperationsService(
        ILogger<BulkOperationsService> logger, 
        PfDbContext context, 
        IRoleService roleService,
        ILocationService locationService, 
        IParameterService parameterService, 
        IMerchantPoolService merchantPoolService, 
        IApiKeyGenerator apiKeyGenerator, 
        IAccountingService accountingService, 
        ICustomerService customerService, 
        IApplicationUserService applicationUserService, 
        IUserService userService, 
        IBus bus)
    {
        _logger = logger;
        _context = context;
        _roleService = roleService;
        _locationService = locationService;
        _parameterService = parameterService;
        _merchantPoolService = merchantPoolService;
        _apiKeyGenerator = apiKeyGenerator;
        _accountingService = accountingService;
        _customerService = customerService;
        _applicationUserService = applicationUserService;
        _userService = userService;
        _bus = bus;
    }

    public async Task<BulkImportMerchantResponse> BulkImportMerchantAsync(BulkImportMerchantCommand command)
    {
        var excelDictionary = ReadExcelToDictionary(command.Bytes);
        var excelMerchants = MapToModels<ExcelMerchantModel>(excelDictionary);
        var response = new BulkImportMerchantResponse { ImportRecords = [] };
        
        var locations = await GetDistrictLocationsAsync(excelDictionary);

        var parentMerchantId = Guid.Empty;
        string parentMerchantName=null, parentMerchantNumber=null;
        if (command.MerchantType == MerchantType.SubMerchant && command.ParentMerchantId.HasValue &&
            command.ParentMerchantId != Guid.Empty)
        {
            var parentMerchant =
                await _context.Merchant.FirstOrDefaultAsync(x => x.Id == command.ParentMerchantId);
            parentMerchantId = parentMerchant.Id;
            parentMerchantName = parentMerchant.Name;
            parentMerchantNumber = parentMerchant.Number;
        }

        foreach (var excelMerchant in excelMerchants)
        {
            var rowResponse = new ImportRecord
            {
                IsSuccess = true,
                RowIndex = excelMerchant.Index
            };
            
            var merchantUser = new MerchantUser();
            try
            {
                if (excelMerchant.Customer.CompanyType == CompanyType.Individual)
                {
                    excelMerchant.Customer.TaxNumber = excelMerchant.Customer.AuthorizedPerson.IdentityNumber;
                }

                var location = locations.First(s => s.DistrictCode == excelMerchant.Customer.District);

                var newMerchant = new Merchant();

                var strategy = _context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    var authorizedPerson = PopulateNewContactPerson(excelMerchant.Customer.AuthorizedPerson,
                        ContactPersonType.AuthorizedPerson);
                    await _context.ContactPerson.AddAsync(authorizedPerson);
                    
                    var createCustomerResponse = await CreateCustomerAsync(excelMerchant, newMerchant.Id, location);
                    var customer = PopulateNewCustomer(excelMerchant.Customer, authorizedPerson.Id,
                        createCustomerResponse.CustomerNumber, createCustomerResponse.CustomerId, location);
                    await _context.Customer.AddAsync(customer);

                    var technicalPerson =
                        PopulateNewContactPerson(excelMerchant.TechnicalContact, ContactPersonType.TechnicalPerson);
                    await _context.ContactPerson.AddAsync(technicalPerson);

                    var merchantNumber = await _merchantPoolService.GenerateMerchantNumberAsync();

                    var merchantPool = PopulateMerchantPool(excelMerchant, command, location);
                    await _context.MerchantPool.AddAsync(merchantPool);

                    newMerchant.Name = excelMerchant.MerchantName;
                    newMerchant.Number = merchantNumber;
                    newMerchant.MerchantType = command.MerchantType;
                    newMerchant.IsInvoiceCommissionReflected = command.IsInvoiceCommissionReflected;
                    newMerchant.MerchantStatus = MerchantStatus.Pending;
                    newMerchant.ApplicationChannel = command.ApplicationChannel;
                    newMerchant.IntegrationMode = command.IntegrationMode;
                    newMerchant.MccCode = excelMerchant.MccCode;
                    newMerchant.CustomerId = customer.Id;
                    newMerchant.Language = command.Language;
                    newMerchant.WebSiteUrl = excelMerchant.WebSiteUrl;
                    newMerchant.MonthlyTurnover = excelMerchant.MonthlyTurnover;
                    newMerchant.PhoneCode = excelMerchant.PhoneCode;
                    newMerchant.AgreementDate = command.AgreementDate;
                    newMerchant.SalesPersonId = command.SalesPersonId;
                    newMerchant.PaymentDueDay = 0;

                    newMerchant.Is3dRequired = command.Is3dRequired;
                    newMerchant.IsDocumentRequired = command.IsDocumentRequired;
                    newMerchant.IsManuelPayment3dRequired = command.IsManuelPayment3dRequired;
                    newMerchant.IsLinkPayment3dRequired = command.IsLinkPayment3dRequired;
                    newMerchant.IsHostedPayment3dRequired = command.IsHostedPayment3dRequired;
                    newMerchant.IsCvvPaymentAllowed = command.IsCvvPaymentAllowed;
                    newMerchant.IsPostAuthAmountHigherAllowed = command.IsPostAuthAmountHigherAllowed;
                    newMerchant.HalfSecureAllowed = command.HalfSecureAllowed;
                    newMerchant.InstallmentAllowed = command.InstallmentAllowed;
                    newMerchant.IsExcessReturnAllowed = command.IsExcessReturnAllowed;
                    newMerchant.InternationalCardAllowed = command.InternationalCardAllowed;
                    newMerchant.PreAuthorizationAllowed = command.PreAuthorizationAllowed;
                    newMerchant.FinancialTransactionAllowed = command.FinancialTransactionAllowed;
                    newMerchant.PaymentAllowed = command.PaymentAllowed;
                    newMerchant.PaymentReverseAllowed = command.PaymentReverseAllowed;
                    newMerchant.PaymentReturnAllowed = command.PaymentReturnAllowed;
                    newMerchant.IsReturnApproved = command.IsReturnApproved ;
                    newMerchant.IsPaymentToMainMerchant = command.IsPaymentToMainMerchant;

                    newMerchant.PricingProfileNumber = command.PricingProfileNumber;
                    newMerchant.MerchantPoolId = merchantPool.Id;
                    newMerchant.MerchantIntegratorId = command.MerchantIntegratorId;
                    newMerchant.ContactPersonId = technicalPerson.Id;
                    newMerchant.HostingTaxNo = command.HostingTaxNo;
                    newMerchant.HostingTradeName = command.HostingTradeName;
                    newMerchant.HostingUrl = command.HostingUrl;
                    newMerchant.PostingPaymentChannel = PostingPaymentChannel.Unknown;
                    newMerchant.CreatedBy = "BULKIMPORT";
                    newMerchant.ParentMerchantId = parentMerchantId;
                    newMerchant.ParentMerchantName = parentMerchantName;
                    newMerchant.ParentMerchantNumber = parentMerchantNumber;

                    await _context.Merchant.AddAsync(newMerchant);

                    var merchantApiKeyDto = await _apiKeyGenerator.Generate(newMerchant.Id);
                    var merchantApiKey = new MerchantApiKey
                    {
                        CreatedBy = "BULKIMPORT",
                        PrivateKeyEncrypted = merchantApiKeyDto.PrivateKeyEncrypted,
                        PublicKey = merchantApiKeyDto.PublicKey,
                        MerchantId = merchantApiKeyDto.MerchantId,
                        CreateDate = DateTime.Now
                    };
                    await _context.MerchantApiKey.AddAsync(merchantApiKey);
                    merchantApiKeyDto.Id = merchantApiKey.Id;
                    merchantApiKeyDto.MerchantNumber = newMerchant.Number;

                    try
                    {
                        var role = await _roleService.GetRoleAsync(command.AdminUserRoleId);
                        merchantUser.UserId = await CreateIdentityUserAsync(excelMerchant.AdminUser, command.AdminUserRoleId, excelMerchant.PhoneCode);
                        merchantUser.Name = excelMerchant.AdminUser.Name;
                        merchantUser.Surname = excelMerchant.AdminUser.Surname;
                        merchantUser.BirthDate = excelMerchant.AdminUser.BirthDate;
                        merchantUser.Email = excelMerchant.AdminUser.Email;
                        merchantUser.MobilePhoneNumber = excelMerchant.AdminUser.MobilePhoneNumber;
                        merchantUser.RoleId = command.AdminUserRoleId.ToString();
                        merchantUser.RoleName = role.Name;
                        merchantUser.MerchantId = newMerchant.Id;
                        merchantUser.RecordStatus = RecordStatus.Active;
                        merchantUser.CreateDate = DateTime.Now;
                        merchantUser.CreatedBy = "BULKIMPORT";
                        merchantUser.ExternalPersonId = Guid.Empty;
                        await _context.MerchantUser.AddAsync(merchantUser);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, $"Create identity user failed. MerchantId: {newMerchant.Id}");
                        throw;
                    }

                    try
                    {
                        await _accountingService.CreateCustomerAsync(newMerchant, customer, authorizedPerson);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, $"Create accounting customer failed. MerchantId: {newMerchant.Id}");
                        throw;
                    }

                    await _context.SaveChangesAsync();
                    scope.Complete();
                });

                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.CreateIksMerchant"));
                await endpoint.Send(new CreateIksMerchant
                {
                    MerchantId = newMerchant.Id
                }, tokenSource.Token);

                response.ImportRecords.Add(rowResponse);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Create Bulk Merchant Failed : {exception}");
                if (merchantUser.UserId != Guid.Empty)
                {
                    using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Identity.DeleteUser"));
                    await endpoint.Send(new DeleteUser
                    {
                        UserId = merchantUser.UserId
                    }, tokenSource.Token);
                }

                rowResponse.IsSuccess = false;
                rowResponse.ErrorMessage = exception.Message;
                response.ImportRecords.Add(rowResponse);
            }
        }

        return response;
    }

    private async Task<Guid> CreateIdentityUserAsync(ExcelMerchantAdminUserModel merchantUser, Guid roleId, string phoneCode)
    {
        var roles = new List<Guid> { roleId };

        CreateUserRequest createUserRequest = new()
        {
            Email = merchantUser.Email,
            FirstName = merchantUser.Name,
            LastName = merchantUser.Surname,
            BirthDate = merchantUser.BirthDate,
            PhoneCode = phoneCode,
            PhoneNumber = merchantUser.MobilePhoneNumber,
            UserType = UserType.Corporate,
            IsBlacklistControl = false,
            Roles = roles,
            UserName =
                string.Concat(UserTypePrefix.Corporate,
                    phoneCode.Replace("+", ""),
                    merchantUser.MobilePhoneNumber)
        };

        var result = await _userService.CreateUserAsync(createUserRequest);

        return result.UserId;
    }

    private static Customer PopulateNewCustomer(ExcelMerchantCustomerModel customer, Guid contactPersonId,
        int customerNumber, Guid customerId, DistrictDetailDto location)
    {
        return new Customer
        {
            CommercialTitle = customer.CommercialTitle,
            CompanyType = customer.CompanyType,
            TradeRegistrationNumber = customer.TradeRegistrationNumber,
            TaxAdministration = customer.TaxAdministration,
            TaxNumber = customer.TaxNumber,
            Country = location.CountryCode,
            CountryName = location.CountryName,
            City = location.CityCode,
            CityName = location.CityName,
            District = location.DistrictCode,
            DistrictName = location.Name,
            PostalCode = customer.PostalCode,
            Address = customer.Address,
            ContactPersonId = contactPersonId,
            CustomerStatus = CustomerStatus.Active,
            CustomerId = customerId,
            CustomerNumber = customerNumber,
            CreateDate = DateTime.Now,
            CreatedBy = "BULKIMPORT",
            ExternalCustomerId = Guid.Empty
        };
    }

    private static MerchantPool PopulateMerchantPool(ExcelMerchantModel excelMerchant, BulkImportMerchantCommand command, DistrictDetailDto location)
    {
        return new MerchantPool
        {
            MerchantPoolStatus = MerchantPoolStatus.Completed,
            MerchantType = command.MerchantType,
            ParentMerchantId = command.ParentMerchantId,
            ParentMerchantName = string.Empty,
            ParentMerchantNumber = string.Empty,
            IsInvoiceCommissionReflected = command.IsInvoiceCommissionReflected,
            MerchantName = excelMerchant.MerchantName,
            CompanyType = excelMerchant.Customer.CompanyType,
            CommercialTitle = excelMerchant.Customer.CommercialTitle,
            WebSiteUrl = excelMerchant.WebSiteUrl,
            MonthlyTurnover = excelMerchant.MonthlyTurnover,
            PostalCode = excelMerchant.Customer.PostalCode,
            Address = excelMerchant.Customer.Address,
            PhoneCode = excelMerchant.PhoneCode,
            Country = location.CountryCode,
            CountryName = location.CountryName,
            City = location.CityCode,
            CityName = location.CityName,
            District = location.DistrictCode,
            DistrictName = location.Name,
            TaxAdministration = excelMerchant.Customer.TaxAdministration,
            TaxNumber = excelMerchant.Customer.TaxNumber,
            TradeRegistrationNumber = excelMerchant.Customer.TradeRegistrationNumber ?? string.Empty,
            Iban = null,
            BankCode = null,
            PostingPaymentChannel = PostingPaymentChannel.Unknown,
            WalletNumber = null,
            CurrencyCode = "TRY",
            Channel = "BULKIMPORT",
            Email = excelMerchant.Customer.AuthorizedPerson.Email,
            CompanyEmail = string.Empty,
            AuthorizedPersonIdentityNumber = excelMerchant.Customer.AuthorizedPerson.IdentityNumber,
            AuthorizedPersonName = excelMerchant.Customer.AuthorizedPerson.Name,
            AuthorizedPersonSurname = excelMerchant.Customer.AuthorizedPerson.Surname,
            AuthorizedPersonBirthDate = excelMerchant.Customer.AuthorizedPerson.BirthDate,
            AuthorizedPersonCompanyPhoneNumber = excelMerchant.Customer.AuthorizedPerson.CompanyPhoneNumber,
            AuthorizedPersonMobilePhoneNumber = excelMerchant.Customer.AuthorizedPerson.MobilePhoneNumber,
            AuthorizedPersonMobilePhoneNumberSecond = string.Empty,
            CreateDate = DateTime.Now,
            CreatedBy = "BULKIMPORT",
            IsPaymentToMainMerchant = command.IsPaymentToMainMerchant
        };
    }

    private static ContactPerson PopulateNewContactPerson(ExcelMerchantContactPersonModel contactPerson,
        ContactPersonType personType)
    {
        return new ContactPerson
        {
            ContactPersonType = personType,
            Email = contactPerson.Email,
            CompanyEmail = null,
            IdentityNumber = contactPerson.IdentityNumber,
            Name = contactPerson.Name,
            Surname = contactPerson.Surname,
            BirthDate = contactPerson.BirthDate,
            CompanyPhoneNumber = contactPerson.CompanyPhoneNumber,
            MobilePhoneNumber = contactPerson.MobilePhoneNumber,
            MobilePhoneNumberSecond = null,
            CreateDate = DateTime.Now,
            CreatedBy = "BULKIMPORT",
            ExternalPersonId = Guid.Empty
        };
    }

    private async Task<CreateCustomerResponse> CreateCustomerAsync(ExcelMerchantModel command, Guid merchantId, DistrictDetailDto location)
    {
        var product = new CustomerProductDto
        {
            OpeningDate = DateTime.Now,
            MerchantId = merchantId,
            ProductType = ProductType.PF,
            CustomerProductStatus = CustomerProductStatus.Active
        };

        var customerRequest = CreateCustomerRequest(command, location);

        customerRequest.CustomerType = command.Customer.CompanyType switch
        {
            CompanyType.Individual => CustomerType.Individual,
            CompanyType.Corporate => CustomerType.Corporate,
            CompanyType.Enterprise => CustomerType.Enterprise,
            _ => customerRequest.CustomerType
        };

        customerRequest.CreateCustomerProducts.Add(product);

        return await _customerService.CreateCustomerAsync(customerRequest);
    }

    private CreateCustomerRequest CreateCustomerRequest(ExcelMerchantModel command, DistrictDetailDto location)
    {
        var address = new CustomerAddressDto();
        var productList = new List<CustomerProductDto>();
        var addressList = new List<CustomerAddressDto>();
        var customerRequest = new CreateCustomerRequest();

        if (command.Customer.CompanyType == CompanyType.Individual)
        {
            customerRequest.FirstName = command.Customer.AuthorizedPerson.Name;
            customerRequest.LastName = command.Customer.AuthorizedPerson.Surname;
            customerRequest.BirthDate = command.Customer.AuthorizedPerson.BirthDate;
            customerRequest.NationCountry = "TR";
            customerRequest.NationCountryId = "TR";
            customerRequest.IdentityNumber = command.Customer.AuthorizedPerson.IdentityNumber;
            customerRequest.DocumentType = DocumentType.Identity;
        }

        customerRequest.CommercialTitle = command.Customer.CommercialTitle;
        customerRequest.TradeRegistrationNumber = command.Customer.TradeRegistrationNumber;
        customerRequest.TaxNumber = command.Customer.TaxNumber;
        customerRequest.TaxAdministration = command.Customer.TaxAdministration;
        customerRequest.UserId = _applicationUserService.ApplicationUserId;

        address.DistrictId = command.Customer.District;
        address.District = location.Name;
        address.CountryId = location.CountryCode;
        address.Country = location.CountryName;
        address.City = location.CityName;
        address.CityId = location.CityCode;
        address.Address = command.Customer.Address;
        address.PostalCode = command.Customer.PostalCode;
        address.AddressType = AddressType.Company;
        addressList.Add(address);

        customerRequest.CreateCustomerAddresses = addressList;
        customerRequest.CreateCustomerProducts = productList;
        customerRequest.CreateCustomerPhones = CreatePhonesDto(command);
        customerRequest.CreateCustomerEmails = CreateEmailsDto(command);

        return customerRequest;
    }

    private static List<CustomerPhoneDto> CreatePhonesDto(ExcelMerchantModel command)
    {
        var phoneList = new List<CustomerPhoneDto>();
        var customerIndividualPhone = new CustomerPhoneDto()
        {
            PhoneCode = command.PhoneCode,
            PhoneNumber = command.Customer.AuthorizedPerson.MobilePhoneNumber,
            Primary = true,
            PhoneType = PhoneType.Individual,
        };
        var customerCompanyPhone = new CustomerPhoneDto()
        {
            PhoneCode = command.PhoneCode,
            PhoneNumber = command.Customer.AuthorizedPerson.CompanyPhoneNumber,
            Primary = false,
            PhoneType = PhoneType.Company,
        };

        phoneList.Add(customerIndividualPhone);
        phoneList.Add(customerCompanyPhone);

        return phoneList;
    }

    private static List<CustomerEmailDto> CreateEmailsDto(ExcelMerchantModel command)
    {
        var emailList = new List<CustomerEmailDto>();

        var customerIndividualEmail = new CustomerEmailDto()
        {
            Email = command.Customer.AuthorizedPerson.Email,
            EmailType = HttpProviders.CustomerManagement.Models.Enums.EmailType.Individual,
            Primary = true,
        };
        emailList.Add(customerIndividualEmail);

        return emailList;
    }

    public async Task<BulkMerchantExcelValidationResponse> PreValidateBulkMerchantExcelFileAsync(
        BulkMerchantExcelValidationCommand command)
    {
        var excelDictionary = ReadExcelToDictionary(command.Bytes);

        var dictionaryErrors = ValidateDictionary(excelDictionary);

        var uniqueErrors = ValidateUniqueColumns(excelDictionary);
        dictionaryErrors.AddRange(uniqueErrors);

        var integrityErrors = await ValidateDataIntegrityAsync(excelDictionary);
        dictionaryErrors.AddRange(integrityErrors);

        var requestValidation = await ValidateRequestAsync(command);
        if (requestValidation.Count > 0)
        {
            dictionaryErrors.Add(new ExcelRowValidationError
            {
                RowIndex = 0,
                Errors = [new ExcelColumnValidationError { ColumnName = "Request", ErrorMessages = requestValidation }]
            });
        }

        var normalizedErrors = NormalizeErrors(dictionaryErrors);

        return new BulkMerchantExcelValidationResponse
        {
            IsValid = normalizedErrors.Count == 0,
            Errors = normalizedErrors
        };
    }

    private static readonly string[] UniqueFields =
    [
        "Customer.CommercialTitle",
        "Customer.TaxNumber",
        "AdminUser.Email",
        "AdminUser.MobilePhoneNumber"
    ];

    private List<ExcelRowValidationError> ValidateUniqueColumns(
        List<Dictionary<string, object>> excelDictionary)
    {
        var lookup = UniqueFields.ToDictionary(
            field => field,
            _ => new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase)
        );

        for (int i = 0; i < excelDictionary.Count; i++)
        {
            var rowIndex = ExcelRow(i);

            foreach (var field in UniqueFields)
            {
                var value = GetValue(excelDictionary[i], field)?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (!lookup[field].TryGetValue(value, out var list))
                {
                    list = new List<int>();
                    lookup[field][value] = list;
                }

                list.Add(rowIndex);
            }
        }

        var errors = new Dictionary<int, ExcelRowValidationError>();

        foreach (var fieldEntry in lookup)
        {
            var field = fieldEntry.Key;

            foreach (var duplicated in fieldEntry.Value.Where(v => v.Value.Count > 1))
            {
                foreach (var rowIndex in duplicated.Value)
                {
                    if (!errors.TryGetValue(rowIndex, out var rowError))
                    {
                        rowError = new ExcelRowValidationError
                        {
                            RowIndex = rowIndex,
                            Errors = new List<ExcelColumnValidationError>()
                        };
                        errors[rowIndex] = rowError;
                    }

                    AddOrUpdateColumnError(
                        rowError.Errors,
                        field,
                        $"Duplicate value '{duplicated.Key}' detected. This field must be unique."
                    );
                }
            }
        }

        return errors.Values.OrderBy(e => e.RowIndex).ToList();
    }

    private async Task<List<string>> ValidateRequestAsync(BulkMerchantExcelValidationCommand command)
    {
        var errors = new List<string>();

        if (command.MerchantType == MerchantType.SubMerchant &&
            !await _context.Merchant.AnyAsync(m =>
                m.Id == command.ParentMerchantId &&
                m.MerchantStatus == MerchantStatus.Active &&
                m.MerchantType == MerchantType.MainMerchant))
        {
            errors.Add("ParentMerchant not found");
        }

        if (command.IsPaymentToMainMerchant && command.MerchantType != MerchantType.SubMerchant)
        {
            errors.Add("IsPaymentToMainMerchant cannot be true when MerchantType is not SubMerchant");
        }

        if (command.MerchantIntegratorId.HasValue &&
            !await _context.MerchantIntegrator.AnyAsync(m => m.Id == command.MerchantIntegratorId))
        {
            errors.Add("MerchantIntegrator not found");
        }

        var pricingProfile = await _context.PricingProfile
            .FirstOrDefaultAsync(b =>
                b.PricingProfileNumber == command.PricingProfileNumber && b.ProfileStatus == ProfileStatus.InUse);

        if (pricingProfile is null)
        {
            errors.Add("PricingProfile not found");
        }

        if (pricingProfile is not null &&
            ((pricingProfile.IsPaymentToMainMerchant && !command.IsPaymentToMainMerchant) ||
             (!pricingProfile.IsPaymentToMainMerchant && command.IsPaymentToMainMerchant)))
        {
            errors.Add("Pricing profile is invalid for IsPaymentToMainMerchant");
        }

        if (
            pricingProfile is not null &&
            ((command.MerchantType is MerchantType.SubMerchant && pricingProfile.ProfileType == ProfileType.Standard)
             ||
             ((command.MerchantType is MerchantType.MainMerchant or MerchantType.StandartMerchant
                 or MerchantType.EasyMerchant) && pricingProfile.ProfileType == ProfileType.SubMerchant))
        )
        {
            errors.Add("Pricing profile type is invalid for MerchantType");
        }

        try
        {
            var role = await _roleService.GetRoleAsync(command.AdminUserRoleId);
            if (role.RoleScope is not RoleScope.CorporateSubMerchant && role.RoleScope is not RoleScope.Merchant)
            {
                errors.Add("Invalid admin user RoleScope");
            }
        }
        catch (Exception exception)
        {
            errors.Add("AdminUserRoleId is invalid.{" + exception.Message + "}");
        }

        if (command.Language.ToLower() is not "tr" or "en")
        {
            errors.Add("Invalid language");
        }

        return errors;
    }

    private async Task<List<ExcelRowValidationError>> ValidateDataIntegrityAsync(
        List<Dictionary<string, object>> excelDictionary)
    {
        var errors = new List<ExcelRowValidationError>();

        var locations = await GetDistrictLocationsAsync(excelDictionary);
        var taxAdministrations = (await _parameterService.GetParametersAsync("TaxAdministrations"))
            .Select(s => s.ParameterValue).ToList();
        var mccCodes = await GetMccCodesAsync(excelDictionary);
        var existingMerchantTaxNumbers = await GetExistingMerchantTaxNumbersAsync(excelDictionary);
        var existingUsers = await GetExistingUsersAsync(excelDictionary);

        for (var i = 0; i < excelDictionary.Count; i++)
        {
            var row = excelDictionary[i];
            if (row == null) continue;

            var rowErrors = ValidateRowDataIntegrity(row, locations, taxAdministrations, mccCodes,
                existingMerchantTaxNumbers, existingUsers);

            if (rowErrors.Count > 0)
            {
                errors.Add(new ExcelRowValidationError { RowIndex = ExcelRow(i), Errors = rowErrors });
            }
        }

        return errors;
    }

    private List<ExcelRowValidationError> ValidateDictionary(
        List<Dictionary<string, object>> excelDictionary)
    {
        var errors = new List<ExcelRowValidationError>();

        for (var i = 0; i < excelDictionary.Count; i++)
        {
            var row = excelDictionary[i];
            if (row == null) continue;

            var rowErrors = ValidateRow(row);

            if (rowErrors.Count > 0)
            {
                errors.Add(new ExcelRowValidationError { RowIndex = ExcelRow(i), Errors = rowErrors });
            }
        }

        return errors;
    }

    private static List<ExcelRowValidationError> NormalizeErrors(
        List<ExcelRowValidationError> errors)
    {
        return errors
            .GroupBy(e => e.RowIndex)
            .Select(rowGroup => new ExcelRowValidationError
            {
                RowIndex = rowGroup.Key,
                Errors = rowGroup
                    .SelectMany(r => r.Errors)
                    .GroupBy(c => c.ColumnName)
                    .Select(colGroup => new ExcelColumnValidationError
                    {
                        ColumnName = colGroup.Key,
                        ErrorMessages = colGroup
                            .SelectMany(c => c.ErrorMessages)
                            .Distinct()
                            .ToList()
                    })
                    .OrderBy(c => c.ColumnName)
                    .ToList()
            })
            .OrderBy(r => r.RowIndex)
            .ToList();
    }

    private async Task<List<DistrictDetailDto>> GetDistrictLocationsAsync(
        List<Dictionary<string, object>> excelDictionary)
    {
        var districts = new HashSet<int>();

        foreach (var value in excelDictionary.Select(row => GetValue(row, "Customer.District")))
        {
            switch (value)
            {
                case int district:
                    districts.Add(district);
                    break;
                case string str when int.TryParse(str, out var parsed):
                    districts.Add(parsed);
                    break;
            }
        }

        return await _locationService.GetDistrictsByDistrictCodes(districts.ToList());
    }

    private async Task<List<string>> GetMccCodesAsync(List<Dictionary<string, object>> excelDictionary)
    {
        var mccList = new HashSet<string>();

        foreach (var value in excelDictionary.Select(row => GetValue(row, "MccCode")))
        {
            if (value is string)
                mccList.Add(value.ToString());
        }

        return await _context.Mcc.Where(s => mccList.Contains(s.Code)).Select(s => s.Code).Distinct().ToListAsync();
    }

    private async Task<List<string>> GetExistingMerchantTaxNumbersAsync(
        List<Dictionary<string, object>> excelDictionary)
    {
        var taxNumbers = new HashSet<string>();

        foreach (var value in excelDictionary.Select(row => GetValue(row, "Customer.TaxNumber")))
        {
            if (value is string)
                taxNumbers.Add(value.ToString());
        }

        var existingCustomers = await _context.Customer
            .Where(s =>
                (s.CustomerStatus == CustomerStatus.Active || s.CustomerStatus == CustomerStatus.Pending) &&
                taxNumbers.Contains(s.TaxNumber) &&
                s.RecordStatus == RecordStatus.Active
            )
            .Select(s => new { Id = s.Id, TaxNumber = s.TaxNumber })
            .ToDictionaryAsync(s => s.Id, s => s.TaxNumber);

        var activeMerchantTaxNumbers = await _context.Merchant
            .Where(x =>
                x.RecordStatus == RecordStatus.Active &&
                !new[]
                {
                    MerchantStatus.Annulment,
                    MerchantStatus.Reject,
                    MerchantStatus.Closed
                }.Contains(x.MerchantStatus) &&
                existingCustomers.Keys.Contains(x.CustomerId)
            )
            .Select(s => existingCustomers[s.CustomerId])
            .ToListAsync();

        return activeMerchantTaxNumbers;
    }

    private async Task<Dictionary<string, string>> GetExistingUsersAsync(
        List<Dictionary<string, object>> excelDictionary)
    {
        var credentials = new List<string>();

        foreach (var row in excelDictionary)
        {
            var email = GetValue(row, "AdminUser.Email")?.ToString();
            var phone = GetValue(row, "AdminUser.MobilePhoneNumber")?.ToString();

            if (email is null || phone is null)
                continue;

            credentials.Add(email);
            credentials.Add(phone);
        }

        var activeUsers = await _context.MerchantUser
            .Where(b =>
                (credentials.Contains(b.MobilePhoneNumber) || credentials.Contains(b.Email))
                && b.RecordStatus == RecordStatus.Active)
            .Select(s => new { Phone = s.MobilePhoneNumber, Email = s.Email })
            .ToDictionaryAsync(s => s.Phone, s => s.Email);

        var activeSubMerchantUser = await _context.SubMerchantUser
            .Where(b =>
                (credentials.Contains(b.MobilePhoneNumber) || credentials.Contains(b.Email))
                && b.RecordStatus == RecordStatus.Active)
            .Select(s => new { Phone = s.MobilePhoneNumber, Email = s.Email })
            .ToDictionaryAsync(s => s.Phone, s => s.Email);

        return activeUsers.Concat(activeSubMerchantUser).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private List<ExcelColumnValidationError> ValidateRowDataIntegrity(Dictionary<string, object> row,
        List<DistrictDetailDto> locations,
        List<string> taxAdministrations,
        List<string> mccCodes,
        List<string> activeTaxNumbers,
        Dictionary<string, string> activeUsers)
    {
        var errors = new List<ExcelColumnValidationError>();

        if (activeTaxNumbers.Contains(GetValue(row, "Customer.TaxNumber")?.ToString()))
        {
            AddOrUpdateColumnError(errors, "Customer.TaxNumber", $"Merchant with this tax number already exists");
        }

        if (!taxAdministrations.Contains(GetValue(row, "Customer.TaxAdministration")?.ToString()))
        {
            AddOrUpdateColumnError(errors, "Customer.TaxAdministration", "Tax administration not found");
        }

        if (!mccCodes.Contains(GetValue(row, "MccCode")?.ToString()))
        {
            AddOrUpdateColumnError(errors, "MccCode", "MccCode not found");
        }

        var adminPhoneNumber = GetValue(row, "AdminUser.MobilePhoneNumber")?.ToString();
        var adminEmail = GetValue(row, "AdminUser.Email")?.ToString();
        if (adminPhoneNumber is not null &&
            adminEmail is not null &&
            (activeUsers.ContainsKey(adminPhoneNumber) || activeUsers.ContainsValue(adminEmail)))
        {
            AddOrUpdateColumnError(errors, "AdminUser.MobilePhoneNumber", $"Admin user credentials already exists");
            AddOrUpdateColumnError(errors, "AdminUser.Email", $"Admin user credentials already exists");
        }

        if (GetValue(row, "Customer.District") is not int districtCode ||
            GetValue(row, "Customer.City") is not int cityCode ||
            GetValue(row, "Customer.Country") is not int countryCode)
        {
            AddOrUpdateColumnError(errors, "Customer.District", $"District-City-Country must be integer");
            return errors;
        }

        var district = locations.FirstOrDefault(s => s.DistrictCode == districtCode);
        if (district is null || district.CityCode != cityCode || district.CountryCode != countryCode)
        {
            AddOrUpdateColumnError(errors, "Customer.District", $"District-City-Country mismatch");
        }

        return errors;
    }

    private List<ExcelColumnValidationError> ValidateRow(
        Dictionary<string, object> row)
    {
        var errors = new List<ExcelColumnValidationError>();

        #region validation rules

        //validate data
        Required("MerchantName");
        MaxLen("MerchantName", 150);

        Required("WebSiteUrl");
        MaxLen("WebSiteUrl", 150);
        RegexRule("WebSiteUrl",
            @"^(https?:\/\/)?(www\.)?[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}$",
            "WebSiteUrl must be a valid domain or URL.");

        Required("MccCode");
        NumericString("MccCode", min: 3, max: 4);

        Required("PhoneCode");
        RegexRule("PhoneCode", @"^\+\d{1,4}$", "PhoneCode must be in format like +90.");

        Required("MonthlyTurnover");
        DecimalMin("MonthlyTurnover", 0);

        Required("Customer.CompanyType");
        MustBeEnum<CompanyType>("Customer.CompanyType");

        Required("Customer.CommercialTitle");
        MaxLen("Customer.CommercialTitle", 100);

        Required("Customer.TradeRegistrationNumber");
        NumericString("Customer.TradeRegistrationNumber", max: 16);

        Required("Customer.TaxAdministration");
        MaxLen("Customer.TaxAdministration", 200);

        Required("Customer.TaxNumber");
        NumericString("Customer.TaxNumber", min: 10, max: 11);

        Required("Customer.Country");
        MustBeInt("Customer.Country");

        Required("Customer.City");
        MustBeInt("Customer.City");

        Required("Customer.District");
        MustBeInt("Customer.District");

        Required("Customer.PostalCode");
        NumericString("Customer.PostalCode", exact: 5);

        Required("Customer.Address");
        MaxLen("Customer.Address", 256);

        Required("Customer.AuthorizedPerson.IdentityNumber");
        NumericString("Customer.AuthorizedPerson.IdentityNumber", exact: 11);

        Required("Customer.AuthorizedPerson.Name");
        MaxLen("Customer.AuthorizedPerson.Name", 100);

        Required("Customer.AuthorizedPerson.Surname");
        MaxLen("Customer.AuthorizedPerson.Surname", 100);

        Required("Customer.AuthorizedPerson.Email");
        MaxLen("Customer.AuthorizedPerson.Email", 256);
        RegexRule("Customer.AuthorizedPerson.Email", @"^[^@\s]+@[^@\s]+\.[^@\s]+$", "Invalid email.");

        Required("Customer.AuthorizedPerson.BirthDate");
        MustBeDate("Customer.AuthorizedPerson.BirthDate");

        Required("Customer.AuthorizedPerson.CompanyPhoneNumber");
        NumericString("Customer.AuthorizedPerson.CompanyPhoneNumber", exact: 10);

        Required("Customer.AuthorizedPerson.MobilePhoneNumber");
        NumericString("Customer.AuthorizedPerson.MobilePhoneNumber", exact: 10);

        Required("TechnicalContact.IdentityNumber");
        NumericString("TechnicalContact.IdentityNumber", exact: 11);

        Required("TechnicalContact.Name");
        MaxLen("TechnicalContact.Name", 100);

        Required("TechnicalContact.Surname");
        MaxLen("TechnicalContact.Surname", 100);

        Required("TechnicalContact.Email");
        MaxLen("TechnicalContact.Email", 256);
        RegexRule("TechnicalContact.Email", @"^[^@\s]+@[^@\s]+\.[^@\s]+$", "Invalid email.");

        Required("TechnicalContact.BirthDate");
        MustBeDate("TechnicalContact.BirthDate");

        Required("TechnicalContact.CompanyPhoneNumber");
        NumericString("TechnicalContact.CompanyPhoneNumber", exact: 10);

        Required("TechnicalContact.MobilePhoneNumber");
        NumericString("TechnicalContact.MobilePhoneNumber", exact: 10);

        Required("AdminUser.Name");
        MaxLen("AdminUser.Name", 100);

        Required("AdminUser.Surname");
        MaxLen("AdminUser.Surname", 100);

        Required("AdminUser.Email");
        MaxLen("AdminUser.Email", 256);
        RegexRule("AdminUser.Email", @"^[^@\s]+@[^@\s]+\.[^@\s]+$", "Invalid email.");

        Required("AdminUser.MobilePhoneNumber");
        NumericString("AdminUser.MobilePhoneNumber", exact: 10);

        Required("AdminUser.PhoneCode");
        RegexRule("AdminUser.PhoneCode", @"^\+\d{1,4}$", "PhoneCode must be in format like +90.");

        Required("AdminUser.BirthDate");
        MustBeDate("AdminUser.BirthDate");

        #endregion

        #region validation functions

        void Required(string key)
        {
            if (IsMissing(GetValue(row, key)))
            {
                AddOrUpdateColumnError(errors, key, $"{key} is required.");
            }
        }

        void MaxLen(string key, int len)
        {
            var value = GetValue(row, key);
            if (value != null && !MaxLength(value, len))
            {
                AddOrUpdateColumnError(errors, key, $"{key} must be at most {len} characters.");
            }
        }

        void NumericString(string key, int? exact = null, int? min = null, int? max = null)
        {
            var value = GetValue(row, key);
            if (value == null) return;

            if (value is not string)
            {
                AddOrUpdateColumnError(errors, key, $"This field must be formatted as text to preserve leading zeros.");
                return;
            }

            if (!IsNumericString(value, exact, min, max))
            {
                AddOrUpdateColumnError(errors, key, $"{key} must contain only digits.");
            }
        }

        void RegexRule(string key, string pattern, string message)
        {
            var value = GetValue(row, key)?.ToString();

            if (string.IsNullOrWhiteSpace(value))
                return;

            value = value
                .Normalize(NormalizationForm.FormKC)
                .Trim();

            if (!Regex.IsMatch(value, pattern, RegexOptions.CultureInvariant))
            {
                AddOrUpdateColumnError(errors, key, message);
            }
        }

        void MustBeInt(string key)
        {
            var value = GetValue(row, key);
            if (value == null || value is not int)
            {
                AddOrUpdateColumnError(errors, key, $"{key} must be a valid integer.");
            }
        }

        void MustBeDate(string key)
        {
            var value = GetValue(row, key);
            if (value == null || !DateTime.TryParse(value.ToString(), out _))
            {
                AddOrUpdateColumnError(errors, key, $"{key} must be a valid date.");
            }
        }

        void MustBeEnum<TEnum>(string key) where TEnum : struct
        {
            var value = GetValue(row, key);
            if (value is not string str || !Enum.TryParse<TEnum>(str, true, out _))
            {
                AddOrUpdateColumnError(errors, key, $"{key} must be a valid {typeof(TEnum).Name}.");
            }
        }

        void DecimalMin(string key, decimal min)
        {
            var value = GetValue(row, key);
            if (value is not decimal d || d < min)
            {
                AddOrUpdateColumnError(errors, key, $"{key} must be greater than or equal to {min}.");
            }
        }

        #endregion

        return errors;
    }

    //dictionary validation helpers
    private void AddOrUpdateColumnError(List<ExcelColumnValidationError> errors, string columnName, string errorMessage)
    {
        if (errors.Any(s => s.ColumnName == columnName))
        {
            errors.First(s => s.ColumnName == columnName).ErrorMessages.Add(errorMessage);
        }
        else
        {
            errors.Add(new ExcelColumnValidationError { ColumnName = columnName, ErrorMessages = [errorMessage] });
        }
    }

    private static object? GetValue(Dictionary<string, object> row, string key)
    {
        var parts = key.Split('.');
        object? current = row;

        foreach (var part in parts)
        {
            if (current is not Dictionary<string, object> dict)
                return null;

            if (!dict.TryGetValue(part, out current))
                return null;
        }

        return current;
    }

    private static bool IsMissing(object? value)
    {
        return value == null ||
               (value is string s && string.IsNullOrWhiteSpace(s));
    }

    private static bool IsNumericString(
        object? value,
        int? exactLength = null,
        int? minLength = null,
        int? maxLength = null)
    {
        if (value is not string str)
            return false;

        if (!str.All(char.IsDigit))
            return false;

        if (exactLength.HasValue && str.Length != exactLength.Value)
            return false;

        if (minLength.HasValue && str.Length < minLength.Value)
            return false;

        if (maxLength.HasValue && str.Length > maxLength.Value)
            return false;

        return true;
    }

    private static bool MaxLength(object? value, int max)
    {
        return value is string str && str.Length <= max;
    }


    //excel readings
    private static List<Dictionary<string, object>> ReadExcelToDictionary(byte[] excelBytes)
    {
        var allowedHeaders = new List<string>
        {
            "MerchantName",
            "WebSiteUrl",
            "MccCode",
            "PhoneCode",
            "MonthlyTurnover",
            "Customer.CompanyType",
            "Customer.CommercialTitle",
            "Customer.TradeRegistrationNumber",
            "Customer.TaxAdministration",
            "Customer.TaxNumber",
            "Customer.Country",
            "Customer.City",
            "Customer.District",
            "Customer.PostalCode",
            "Customer.Address",
            "Customer.AuthorizedPerson.IdentityNumber",
            "Customer.AuthorizedPerson.Name",
            "Customer.AuthorizedPerson.Surname",
            "Customer.AuthorizedPerson.Email",
            "Customer.AuthorizedPerson.BirthDate",
            "Customer.AuthorizedPerson.CompanyPhoneNumber",
            "Customer.AuthorizedPerson.MobilePhoneNumber",
            "TechnicalContact.IdentityNumber",
            "TechnicalContact.Name",
            "TechnicalContact.Surname",
            "TechnicalContact.Email",
            "TechnicalContact.BirthDate",
            "TechnicalContact.CompanyPhoneNumber",
            "TechnicalContact.MobilePhoneNumber",
            "AdminUser.Name",
            "AdminUser.Surname",
            "AdminUser.Email",
            "AdminUser.MobilePhoneNumber",
            "AdminUser.PhoneCode",
            "AdminUser.BirthDate"
        };

        var result = new List<Dictionary<string, object>>();

        using var stream = new MemoryStream(excelBytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        var headers = worksheet.Rows("1").Cells().Where(cell => cell.Value.ToString() != "")
            .Select(cell => cell.Value.ToString()).Distinct().ToList();

        if (!headers.SequenceEqual(allowedHeaders))
        {
            throw new InvalidDataException("Excel headers are not valid.");
        }

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            if (row.CellsUsed().All(c => string.IsNullOrWhiteSpace(c.GetValue<string>())))
                continue;

            var rowDict = new Dictionary<string, object>();

            for (var i = 0; i < headers.Count; i++)
            {
                var key = headers[i];
                var cell = row.Cell(i + 1);
                var value = cell.Value;

                AddNestedExcelValue(rowDict, key, value);
            }

            result.Add(rowDict);
        }

        return result;
    }

    private static void AddNestedExcelValue(Dictionary<string, object> root, string key, object value)
    {
        var parts = key.Split('.');
        var current = root;

        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (!current.ContainsKey(parts[i]))
                current[parts[i]] = new Dictionary<string, object>();

            current = (Dictionary<string, object>)current[parts[i]];
        }

        current[parts[^1]] = ParseExcelValue(key, value.ToString());
    }

    private static object ParseExcelValue(string key, string value)
    {
        var parsedValue = key switch
        {
            "MonthlyTurnover" =>
                decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                    ? (object)d
                    : value,
            "Customer.Country" => int.TryParse(value, out var i) ? i : value,
            "Customer.City" => int.TryParse(value, out var i) ? i : value,
            "Customer.District" => int.TryParse(value, out var i) ? i : value,
            "Customer.AuthorizedPerson.BirthDate" => DateTime.TryParse(value, out var dt) ? dt : value,
            "TechnicalContact.BirthDate" => DateTime.TryParse(value, out var dt) ? dt : value,
            "AdminUser.BirthDate" => DateTime.TryParse(value, out var dt) ? dt : value,
            _ => value
        };

        return parsedValue;
    }

    static int ExcelRow(int dictionaryIndex) => dictionaryIndex + 2;

    //dictionary to model mapping
    private static List<T> MapToModels<T>(List<Dictionary<string, object>> source)
    {
        if (source == null || source.Count == 0)
            return new List<T>();

        for (int i = 0; i < source.Count; i++)
        {
            source[i]["Index"] = ExcelRow(i);
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        var json = JsonSerializer.Serialize(source, options);

        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, options)
                   ?? new List<T>();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Excel → Model mapping failed for {typeof(T).Name}", ex);
        }
    }
}