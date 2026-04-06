using AutoMapper;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Interfaces.Boa;
using LinkPara.PF.Application.Commons.Models.IdentityModels;
using LinkPara.PF.Application.Commons.Models.IKS;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Boa.Merchants;
using LinkPara.PF.Application.Features.Boa.Merchants.Command.CreateBoaMerchant;
using LinkPara.PF.Application.Features.PricingProfiles.Queries.GetFilterPricingProfile;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Transactions;
using LinkPara.PF.Application.Commons.Helpers;

namespace LinkPara.PF.Infrastructure.Boa;

public class BoaMerchantService : IBoaMerchantService
{
    private readonly ILogger<BoaMerchantService> _logger;
    private readonly PfDbContext _context;
    private readonly IResponseCodeService _errorCodeService;
    private readonly IMerchantPoolService _merchantPoolService;
    private readonly IApiKeyGenerator _apiKeyGenerator;
    private readonly IAccountingService _accountingService;
    private readonly ICustomerService _customerService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IBus _bus;
    private readonly IMapper _mapper;
    private readonly IVaultClient _vaultClient;
    private readonly ISearchService _searchService;
    private readonly IParameterService _parameterService;
    private readonly IStringLocalizer _localizer;

    private const string GenericErrorCode = "99";

    public BoaMerchantService(
        ILogger<BoaMerchantService> logger,
        PfDbContext context, IResponseCodeService errorCodeService,
        IMerchantPoolService merchantPoolService,
        IApiKeyGenerator apiKeyGenerator,
        IAccountingService accountingService,
        ICustomerService customerService,
        IApplicationUserService applicationUserService,
        IUserService userService,
        IRoleService roleService,
        IBus bus,
        IStringLocalizerFactory factory,
        IMapper mapper, IVaultClient vaultClient, ISearchService searchService, IParameterService parameterService)
    {
        _logger = logger;
        _context = context;
        _errorCodeService = errorCodeService;
        _merchantPoolService = merchantPoolService;
        _apiKeyGenerator = apiKeyGenerator;
        _accountingService = accountingService;
        _customerService = customerService;
        _applicationUserService = applicationUserService;
        _userService = userService;
        _roleService = roleService;
        _bus = bus;
        _mapper = mapper;
        _vaultClient = vaultClient;
        _searchService = searchService;
        _parameterService = parameterService;
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
    }

    public async Task<CreateBoaMerchantResponse> CreateBoaMerchantAsync(CreateBoaMerchantCommand command)
    {
        var merchantUser = new MerchantUser();
        try
        {
            var response = new CreateBoaMerchantResponse
            {
                IsSucceed = true,
                MerchantStatus = MerchantStatus.Pending
            };

            if (command.Customer.CompanyType == CompanyType.Individual)
            {
                command.Customer.TaxNumber = command.Customer.AuthorizedPerson.IdentityNumber;
            }

            var checkCustomer = await _context.Customer
                .Where(s =>
                    (s.CustomerStatus == CustomerStatus.Active || s.CustomerStatus == CustomerStatus.Pending) &&
                    s.TaxNumber == command.Customer.TaxNumber &&
                    s.CommercialTitle.ToLower() == command.Customer.CommercialTitle.ToLower() &&
                    s.RecordStatus == RecordStatus.Active
                )
                .FirstOrDefaultAsync();

            var validationResponse = await CheckValidationsAsync(command, checkCustomer);
            if (!validationResponse.IsValid)
            {
                _logger.LogError($"CreateBoaMerchant failed with code : {validationResponse.Code}, " +
                                 $"Message: {validationResponse.Message}");

                return new CreateBoaMerchantResponse
                {
                    IsSucceed = false,
                    ErrorCode = validationResponse.Code,
                    ErrorMessage = validationResponse.Message
                };
            }

            //blacklist control
            var IsBlacklistCheckEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "BlacklistEnabled");
            string amlReferenceNumber = null;
            if (IsBlacklistCheckEnabled)
            {
                var validateBlacklist = await CheckBlacklistAsync(command);
                
                SearchByNameRequest searchRequest = new()
                {
                    Name = $"{command.AdminUser.Name} {command.AdminUser.Surname}",
                    BirthYear = command.AdminUser.BirthDate.Year.ToString(),
                    SearchType = SearchType.Corporate,
                    FraudChannelType = command.FraudChannelType
                };

                var res = await UserBlacklistControlAsync(searchRequest);
                amlReferenceNumber = res.Message;

                if (!validateBlacklist.IsValid || !res.IsValid)
                {
                    _logger.LogError($"CreateBoaMerchant failed with code : {validateBlacklist.Code}, " +
                                     $"Message: {validateBlacklist.Message}");

                    return new CreateBoaMerchantResponse
                    {
                        IsSucceed = false,
                        ErrorCode = validateBlacklist.Code,
                        ErrorMessage = validateBlacklist.Message
                    };
                }
            }

            var newMerchant = new Merchant();

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var authorizedPerson = PopulateNewContactPerson(command.Customer.AuthorizedPerson,
                    ContactPersonType.AuthorizedPerson);
                await _context.ContactPerson.AddAsync(authorizedPerson);

                var createCustomerResponse = await CreateCustomerAsync(command, newMerchant.Id);
                var customer = PopulateNewCustomer(command.Customer, authorizedPerson.Id,
                    createCustomerResponse.CustomerNumber, createCustomerResponse.CustomerId);
                await _context.Customer.AddAsync(customer);

                var technicalPerson =
                    PopulateNewContactPerson(command.TechnicalContact, ContactPersonType.TechnicalPerson);
                await _context.ContactPerson.AddAsync(technicalPerson);

                var merchantBusinessPartner = PopulateNewMerchantBusinessPartners(command.MerchantBusinessPartner, newMerchant.Id);
                _context.MerchantBusinessPartner.AddRange(merchantBusinessPartner);

                var merchantNumber = await _merchantPoolService.GenerateMerchantNumberAsync();
                response.MerchantNumber = merchantNumber;

                var merchantPool = PopulateMerchantPool(command);
                await _context.MerchantPool.AddAsync(merchantPool);

                newMerchant.Name = command.MerchantName;
                newMerchant.Number = merchantNumber;
                newMerchant.MerchantType = command.MerchantType;
                newMerchant.IsInvoiceCommissionReflected =
                    (command.MerchantType == MerchantType.MainMerchant &&
                     command.IsInvoiceCommissionReflected.HasValue) && command.IsInvoiceCommissionReflected.Value;
                newMerchant.MerchantStatus = MerchantStatus.Pending;
                newMerchant.ApplicationChannel = command.ApplicationChannel;
                newMerchant.IntegrationMode = command.IntegrationMode;
                newMerchant.MccCode = command.MccCode;
                newMerchant.CustomerId = customer.Id;
                newMerchant.Language = command.Language;
                newMerchant.WebSiteUrl = command.WebSiteUrl;
                newMerchant.MonthlyTurnover = command.MonthlyTurnover;
                newMerchant.PhoneCode = command.PhoneCode;
                newMerchant.AgreementDate = command.AgreementDate;
                newMerchant.SalesPersonId = command.SalesPersonId;
                newMerchant.PaymentDueDay = 0;
                newMerchant.EstablishmentDate = command.EstablishmentDate;
                newMerchant.BusinessModel = command.BusinessModel;
                newMerchant.BusinessActivity = command.BusinessActivity;
                newMerchant.BranchCount = command.BranchCount;
                newMerchant.EmployeeCount = command.EmployeeCount;

                newMerchant.Is3dRequired = command.Is3dRequired ?? true;
                newMerchant.IsDocumentRequired = command.IsDocumentRequired ?? false;
                newMerchant.IsManuelPayment3dRequired = command.IsManuelPayment3dRequired ?? true;
                newMerchant.IsLinkPayment3dRequired = command.IsLinkPayment3dRequired ?? true;
                newMerchant.IsHostedPayment3dRequired = command.IsHostedPayment3dRequired ?? true;
                newMerchant.IsCvvPaymentAllowed = command.IsCvvPaymentAllowed ?? true;
                newMerchant.IsPostAuthAmountHigherAllowed = command.IsPostAuthAmountHigherAllowed ?? false;
                newMerchant.HalfSecureAllowed = command.HalfSecureAllowed ?? false;
                newMerchant.InstallmentAllowed = command.InstallmentAllowed ?? false;
                newMerchant.IsExcessReturnAllowed = command.IsExcessReturnAllowed ?? false;
                newMerchant.InternationalCardAllowed = command.InternationalCardAllowed ?? false;
                newMerchant.PreAuthorizationAllowed = command.PreAuthorizationAllowed ?? false;
                newMerchant.FinancialTransactionAllowed = command.FinancialTransactionAllowed ?? false;
                newMerchant.PaymentAllowed = command.PaymentAllowed ?? false;
                newMerchant.PaymentReverseAllowed = command.PaymentReverseAllowed ?? false;
                newMerchant.PaymentReturnAllowed = command.PaymentReturnAllowed ?? false;
                newMerchant.IsReturnApproved = command.IsReturnApproved ?? true;
                newMerchant.IsPaymentToMainMerchant = false;

                newMerchant.PricingProfileNumber = command.PricingProfileNumber;
                newMerchant.MerchantPoolId = merchantPool.Id;
                newMerchant.MerchantIntegratorId = command.MerchantIntegratorId;
                newMerchant.ContactPersonId = technicalPerson.Id;
                newMerchant.HostingTaxNo = command.HostingTaxNo;
                newMerchant.HostingTradeName = command.HostingTradeName;
                newMerchant.HostingUrl = command.HostingUrl;
                newMerchant.PostingPaymentChannel = command.PostingPaymentChannel;
                newMerchant.Information = command.FraudChannelType.ToString();
                newMerchant.CreatedBy = "BOA";

                var defaultMoneyTransferHour =
                    await MoneyTransferHourHelper.GetMoneyTransferHourAsync(_parameterService, _logger);
                newMerchant.MoneyTransferStartHour = command.MoneyTransferStartHour;
                newMerchant.MoneyTransferStartMinute = command.MoneyTransferStartMinute;

                if (command.MerchantType == MerchantType.SubMerchant && command.ParentMerchantId.HasValue &&
                    command.ParentMerchantId != Guid.Empty)
                {
                    var parentMerchant =
                        await _context.Merchant.FirstOrDefaultAsync(x => x.Id == command.ParentMerchantId);
                    newMerchant.ParentMerchantId = parentMerchant.Id;
                    newMerchant.ParentMerchantName = parentMerchant.Name;
                    newMerchant.ParentMerchantNumber = parentMerchant.Number;
                }

                await _context.Merchant.AddAsync(newMerchant);

                var merchantBankAccount = new MerchantBankAccount
                {
                    Iban = command.MerchantIban,
                    BankCode = command.MerchantIbanBankCode,
                    MerchantId = newMerchant.Id,
                    RecordStatus = RecordStatus.Active,
                    CreateDate = DateTime.Now,
                    CreatedBy = "BOA"
                };
                await _context.MerchantBankAccount.AddAsync(merchantBankAccount);

                if (!string.IsNullOrEmpty(command.MerchantWalletNumber))
                {
                    var merchantWallet = new MerchantWallet
                    {
                        WalletNumber = command.MerchantWalletNumber.Trim().Replace(" ", string.Empty),
                        MerchantId = newMerchant.Id,
                        RecordStatus = RecordStatus.Active,
                        CreateDate = DateTime.Now,
                        CreatedBy = "BOA"
                    };
                    await _context.MerchantWallet.AddAsync(merchantWallet);
                }

                var merchantApiKeyDto = await _apiKeyGenerator.Generate(newMerchant.Id);
                var merchantApiKey = new MerchantApiKey
                {
                    CreatedBy = "BOA",
                    PrivateKeyEncrypted = merchantApiKeyDto.PrivateKeyEncrypted,
                    PublicKey = merchantApiKeyDto.PublicKey,
                    MerchantId = merchantApiKeyDto.MerchantId,
                    CreateDate = DateTime.Now
                };
                await _context.MerchantApiKey.AddAsync(merchantApiKey);
                merchantApiKeyDto.Id = merchantApiKey.Id;
                merchantApiKeyDto.MerchantNumber = newMerchant.Number;
                response.MerchantApiKey = merchantApiKeyDto;

                List<Vpos> vposList;
                if (command.IsAllBanksEnabled.HasValue && command.IsAllBanksEnabled.Value)
                {
                    vposList = await _context.Vpos.Where(s =>
                            s.VposStatus == VposStatus.Active && s.IsOnUsVpos != true && s.IsTopUpVpos != true)
                        .ToListAsync();
                }
                else
                {
                    vposList = await _context.Vpos
                        .Where(s => s.VposStatus == VposStatus.Active && command.VposList.Contains(s.Id)).ToListAsync();
                }

                var priority = 1;
                var merchantVposList = new List<MerchantVpos>();
                foreach (var merchantVpos in vposList.Select(vpos => new MerchantVpos
                {
                    MerchantId = newMerchant.Id,
                    VposId = vpos.Id,
                    TerminalStatus = vpos.IsOnUsVpos || (vpos.IsTopUpVpos.HasValue && vpos.IsTopUpVpos.Value)
                                 ? TerminalStatus.Active
                                 : TerminalStatus.PendingRequest,
                    Priority = priority,
                    RecordStatus = RecordStatus.Active,
                    CreateDate = DateTime.Now,
                    CreatedBy = "BOA"
                }))
                {
                    merchantVposList.Add(merchantVpos);
                    priority++;
                }

                _context.MerchantVpos.AddRange(merchantVposList);

                var merchantLimitList = command.MerchantLimits.Select(limitRequest => new MerchantLimit
                {
                    MerchantId = newMerchant.Id,
                    TransactionLimitType = limitRequest.TransactionLimitType,
                    Period = limitRequest.Period,
                    LimitType = limitRequest.LimitType,
                    MaxPiece = limitRequest.MaxPiece,
                    MaxAmount = limitRequest.MaxAmount,
                    Currency = limitRequest.CurrencyCode,
                    RecordStatus = RecordStatus.Active,
                    CreateDate = DateTime.Now,
                    CreatedBy = "BOA"
                })
                    .ToList();
                _context.MerchantLimit.AddRange(merchantLimitList);

                try
                {
                    var role = await _roleService.GetRoleAsync(command.AdminUser.RoleId);
                    merchantUser.UserId = await CreateIdentityUserAsync(command.AdminUser, command.PhoneCode, amlReferenceNumber);
                    merchantUser.Name = command.AdminUser.Name;
                    merchantUser.Surname = command.AdminUser.Surname;
                    merchantUser.BirthDate = command.AdminUser.BirthDate;
                    merchantUser.Email = command.AdminUser.Email;
                    merchantUser.MobilePhoneNumber = command.AdminUser.MobilePhoneNumber;
                    merchantUser.RoleId = command.AdminUser.RoleId.ToString();
                    merchantUser.RoleName = role.Name;
                    merchantUser.MerchantId = newMerchant.Id;
                    merchantUser.RecordStatus = RecordStatus.Active;
                    merchantUser.CreateDate = DateTime.Now;
                    merchantUser.CreatedBy = "BOA";
                    merchantUser.ExternalPersonId = command.AdminUser.ExternalPersonId;
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

            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Create Boa Merchant Failed : {exception}");
            if (merchantUser.UserId != Guid.Empty)
            {
                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Identity.DeleteUser"));
                await endpoint.Send(new DeleteUser
                {
                    UserId = merchantUser.UserId
                }, tokenSource.Token);
            }
            return new CreateBoaMerchantResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
            };
        }
    }

    private async Task<Guid> CreateIdentityUserAsync(CreateMerchantUser merchantUser, string phoneCode, string amlReferenceNumber)
    {
        var roles = new List<Guid> { merchantUser.RoleId };

        CreateUserRequest createUserRequest = new()
        {
            Email = merchantUser.Email,
            FirstName = merchantUser.Name,
            LastName = merchantUser.Surname,
            BirthDate = merchantUser.BirthDate,
            PhoneCode = phoneCode,
            PhoneNumber = merchantUser.MobilePhoneNumber,
            UserType = UserType.Corporate,
            IsBlacklistControl = true,
            AmlReferenceNumber = amlReferenceNumber,
            Roles = roles,
            UserName =
                string.Concat(UserTypePrefix.Corporate,
                    phoneCode.Replace("+", ""),
                    merchantUser.MobilePhoneNumber)
        };

        var result = await _userService.CreateUserAsync(createUserRequest);

        return result.UserId;
    }

    private static Customer PopulateNewCustomer(CreateMerchantCustomer customer, Guid contactPersonId, int customerNumber, Guid customerId)
    {
        return new Customer
        {
            CommercialTitle = customer.CommercialTitle,
            CompanyType = customer.CompanyType,
            TradeRegistrationNumber = customer.TradeRegistrationNumber,
            TaxAdministration = customer.TaxAdministration,
            TaxNumber = customer.TaxNumber,
            Country = customer.Country,
            CountryName = customer.CountryName,
            City = customer.City,
            CityName = customer.CityName,
            District = customer.District,
            DistrictName = customer.DistrictName,
            PostalCode = customer.PostalCode,
            Address = customer.Address,
            ContactPersonId = contactPersonId,
            CustomerStatus = CustomerStatus.Active,
            CustomerId = customerId,
            CustomerNumber = customerNumber,
            CreateDate = DateTime.Now,
            CreatedBy = "BOA",
            ExternalCustomerId = customer.ExternalCustomerId
        };
    }

    private static MerchantPool PopulateMerchantPool(CreateBoaMerchantCommand command)
    {
        return new MerchantPool
        {
            MerchantPoolStatus = MerchantPoolStatus.Completed,
            MerchantType = command.MerchantType,
            ParentMerchantId = command.ParentMerchantId,
            ParentMerchantName = string.Empty,
            ParentMerchantNumber = string.Empty,
            IsInvoiceCommissionReflected = command.IsInvoiceCommissionReflected ?? false,
            MerchantName = command.MerchantName,
            CompanyType = command.Customer.CompanyType,
            CommercialTitle = command.Customer.CommercialTitle,
            WebSiteUrl = command.WebSiteUrl,
            MonthlyTurnover = command.MonthlyTurnover,
            PostalCode = command.Customer.PostalCode,
            Address = command.Customer.Address,
            PhoneCode = command.PhoneCode,
            Country = command.Customer.Country,
            CountryName = command.Customer.CountryName,
            City = command.Customer.City,
            CityName = command.Customer.CityName,
            District = command.Customer.District,
            DistrictName = command.Customer.DistrictName,
            TaxAdministration = command.Customer.TaxAdministration,
            TaxNumber = command.Customer.TaxNumber,
            TradeRegistrationNumber = command.Customer.TradeRegistrationNumber ?? string.Empty,
            Iban = command.MerchantIban,
            BankCode = command.MerchantIbanBankCode,
            PostingPaymentChannel = command.PostingPaymentChannel,
            WalletNumber = command.MerchantWalletNumber,
            CurrencyCode = "TRY",
            Channel = "BOA",
            Email = command.Customer.AuthorizedPerson.Email,
            CompanyEmail = command.Customer.AuthorizedPerson.CompanyEmail ?? string.Empty,
            AuthorizedPersonIdentityNumber = command.Customer.AuthorizedPerson.IdentityNumber,
            AuthorizedPersonName = command.Customer.AuthorizedPerson.Name,
            AuthorizedPersonSurname = command.Customer.AuthorizedPerson.Surname,
            AuthorizedPersonBirthDate = command.Customer.AuthorizedPerson.BirthDate,
            AuthorizedPersonCompanyPhoneNumber = command.Customer.AuthorizedPerson.CompanyPhoneNumber,
            AuthorizedPersonMobilePhoneNumber = command.Customer.AuthorizedPerson.MobilePhoneNumber,
            AuthorizedPersonMobilePhoneNumberSecond = command.Customer.AuthorizedPerson.MobilePhoneNumberSecond,
            CreateDate = DateTime.Now,
            CreatedBy = "BOA",
            IsPaymentToMainMerchant = false
        };
    }

    private static ContactPerson PopulateNewContactPerson(CreateMerchantContactPerson contactPerson, ContactPersonType personType)
    {
        return new ContactPerson
        {
            ContactPersonType = personType,
            Email = contactPerson.Email,
            CompanyEmail = contactPerson.CompanyEmail,
            IdentityNumber = contactPerson.IdentityNumber,
            Name = contactPerson.Name,
            Surname = contactPerson.Surname,
            BirthDate = contactPerson.BirthDate,
            CompanyPhoneNumber = contactPerson.CompanyPhoneNumber,
            MobilePhoneNumber = contactPerson.MobilePhoneNumber,
            MobilePhoneNumberSecond = contactPerson.MobilePhoneNumberSecond,
            CreateDate = DateTime.Now,
            CreatedBy = "BOA",
            ExternalPersonId = contactPerson.ExternalPersonId
        };
    }

    private static List<MerchantBusinessPartner> PopulateNewMerchantBusinessPartners(List<CreateMerchantBusinessPartner> merchantBusinessPartners,
     Guid merchantId)
    {
        if (merchantBusinessPartners == null || !merchantBusinessPartners.Any())
            return new List<MerchantBusinessPartner>();

        return merchantBusinessPartners.Select(p => new MerchantBusinessPartner
        {
            MerchantId = merchantId,
            FirstName = p.FirstName,
            LastName = p.LastName,
            PhoneNumber = p.PhoneNumber,
            BirthDate = p.BirthDate,
            Email = p.Email,
            IdentityNumber = p.IdentityNumber,
            CreateDate = DateTime.UtcNow,
            CreatedBy = "BOA",
        }).ToList();
    }

    private async Task<ValidationResponse> CheckValidationsAsync(CreateBoaMerchantCommand command, Customer customer)
    {
        if (customer is not null)
        {
            var isActiveMerchantExists = await _context.Merchant.AnyAsync(x =>
                x.RecordStatus == RecordStatus.Active &&
                (x.MerchantStatus != MerchantStatus.Annulment || x.MerchantStatus != MerchantStatus.Reject || x.MerchantStatus != MerchantStatus.Closed) &&
                x.CustomerId == customer.Id
            );
            if (isActiveMerchantExists)
            {
                return await GetValidationResponseAsync(ApiErrorCode.MerchantAlreadyActive, command.Language);
            }
        }

        if (command.MerchantType == MerchantType.SubMerchant &&
            !await _context.Merchant.AnyAsync(m =>
                m.Id == command.ParentMerchantId &&
                m.MerchantStatus == MerchantStatus.Active &&
                m.MerchantType == MerchantType.MainMerchant))
        {
            return await GetValidationResponseAsync(ApiErrorCode.MainMerchantNotFound, command.Language);
        }

        if (!await _context.Mcc.AnyAsync(m => m.Code == command.MccCode))
        {
            return await GetValidationResponseAsync(ApiErrorCode.MccCodeNotFound, command.Language);
        }

        if (command.MerchantIntegratorId.HasValue &&
            !await _context.MerchantIntegrator.AnyAsync(m => m.Id == command.MerchantIntegratorId))
        {
            return await GetValidationResponseAsync(ApiErrorCode.MerchantIntegratorNotFound, command.Language);
        }

        if (!await _context.Bank.AnyAsync(b => b.Code == command.MerchantIbanBankCode))
        {
            return await GetValidationResponseAsync(ApiErrorCode.BankCodeNotFound, command.Language);
        }

        var pricingProfile = await _context.PricingProfile
            .FirstOrDefaultAsync(b => b.PricingProfileNumber == command.PricingProfileNumber && b.ProfileStatus == ProfileStatus.InUse);

        if (pricingProfile is null)
        {
            return await GetValidationResponseAsync(ApiErrorCode.PricingProfileNotFound, command.Language);
        }

        if (pricingProfile.IsPaymentToMainMerchant)
        {
            return await GetValidationResponseAsync(ApiErrorCode.InvalidMerchantPricingProfile, command.Language);
        }

        if (
                (command.MerchantType is MerchantType.SubMerchant && pricingProfile.ProfileType == ProfileType.Standard)
                ||
                ((command.MerchantType is MerchantType.MainMerchant or MerchantType.StandartMerchant or MerchantType.EasyMerchant) && pricingProfile.ProfileType == ProfileType.SubMerchant)
           )
        {
            return await GetValidationResponseAsync(ApiErrorCode.PricingProfileTypeIsNotValid, command.Language);
        }

        if (command.MerchantLimits.Count > 0)
        {
            var currencyCodes = command.MerchantLimits.Where(s => !string.IsNullOrEmpty(s.CurrencyCode)).Select(s => s.CurrencyCode).ToList();
            var existingCodes = await _context.Currency
                .Where(x => currencyCodes.Contains(x.Code))
                .Select(x => x.Code)
                .ToListAsync();
            var missingCodes = currencyCodes.Except(existingCodes).ToList();
            var nullCurrencyCodes = command.MerchantLimits.Where(s => string.IsNullOrEmpty(s.CurrencyCode)).ToList();
            if (missingCodes.Any() || nullCurrencyCodes.Any())
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidCurrencyInMerchantLimit, command.Language);
            }
        }

        var activeUser = await _context.MerchantUser
            .FirstOrDefaultAsync(b =>
                (b.MobilePhoneNumber == command.AdminUser.MobilePhoneNumber || b.Email == command.AdminUser.Email)
                && b.RecordStatus == RecordStatus.Active);

        var activeSubMerchantUser = await _context.SubMerchantUser
            .FirstOrDefaultAsync(b =>
                (b.MobilePhoneNumber == command.AdminUser.MobilePhoneNumber || b.Email == command.AdminUser.Email)
                && b.RecordStatus == RecordStatus.Active);

        if (activeUser is not null || activeSubMerchantUser is not null)
        {
            return await GetValidationResponseAsync(ApiErrorCode.AdminUserCredentialsAlreadyExists, command.Language);
        }

        var groupedLimits = command.MerchantLimits.GroupBy(x => new { x.TransactionLimitType, x.LimitType });
        foreach (var group in groupedLimits)
        {
            var daily = group.FirstOrDefault(x => x.Period == Period.Daily);
            var monthly = group.FirstOrDefault(x => x.Period == Period.Monthly);

            if (daily is not null && monthly is not null)
            {
                if (daily.MaxAmount > monthly.MaxAmount || daily.MaxPiece > monthly.MaxPiece)
                {
                    return await GetValidationResponseAsync(ApiErrorCode.MerchantLimitDailyMaxValueExceeded, command.Language);
                }

                if (monthly.MaxAmount < daily.MaxAmount || monthly.MaxPiece < daily.MaxPiece)
                {
                    return await GetValidationResponseAsync(ApiErrorCode.MerchantLimitMonthlyMaxValueExceeded, command.Language);
                }
            }
        }

        return new ValidationResponse { IsValid = true };
    }

    private async Task<ValidationResponse> UserBlacklistControlAsync(SearchByNameRequest searchRequest)
    {
        var matchRate = await _parameterService.GetParameterAsync("FraudParameters", "MatchRate");

        var blackListTitleControl = await _searchService.GetSearchByName(searchRequest);

        if ((blackListTitleControl.MatchStatus == MatchStatus.PotentialMatch || blackListTitleControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListTitleControl.MatchRate >= Convert.ToInt32(matchRate.ParameterValue))
        {
            var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

            var exceptionMessage = _localizer.GetString("UserInBlacklistException");

            return new ValidationResponse
            {
                Code = GenericErrorCode,
                IsValid = false,
                Message = exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue)
            };
        }

        return new ValidationResponse { IsValid = true, Message = blackListTitleControl.ReferenceNumber };
    }

    private async Task<ValidationResponse> CheckBlacklistAsync(CreateBoaMerchantCommand command)
    {
        var requests = new List<SearchByNameRequest>();

        if (command.Customer?.AuthorizedPerson != null)
        {
            requests.Add(new SearchByNameRequest
            {
                Name = $"{command.Customer.AuthorizedPerson.Name} {command.Customer.AuthorizedPerson.Surname}",
                BirthYear = command.Customer.AuthorizedPerson.BirthDate.Year.ToString(),
                SearchType = SearchType.Corporate,
                FraudChannelType = command.FraudChannelType
            });
        }

        if (command.MerchantBusinessPartner != null && command.MerchantBusinessPartner.Any())
        {
            foreach (var partner in command.MerchantBusinessPartner)
            {
                requests.Add(new SearchByNameRequest
                {
                    Name = $"{partner.FirstName} {partner.LastName}",
                    BirthYear = partner.BirthDate.Year.ToString(),
                    SearchType = SearchType.Corporate,
                    FraudChannelType = command.FraudChannelType
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(command.Customer?.CommercialTitle))
        {
            requests.Add(new SearchByNameRequest
            {
                Name = command.Customer.CommercialTitle,
                SearchType = SearchType.Corporate,
                FraudChannelType = command.FraudChannelType
            });
        }

        foreach (var req in requests)
        {
            var result = await UserBlacklistControlAsync(req);
            if (!result.IsValid)
                return result;
        }

        return new ValidationResponse { IsValid = true };
    }

    private async Task<ValidationResponse> GetValidationResponseAsync(string errorCode, string languageCode)
    {
        var merchantResponse = await _errorCodeService.GetApiResponseCode(errorCode, languageCode);

        return new ValidationResponse
        {
            Code = merchantResponse.ResponseCode,
            IsValid = false,
            Message = merchantResponse.DisplayMessage
        };
    }

    private async Task<CreateCustomerResponse> CreateCustomerAsync(CreateBoaMerchantCommand command, Guid merchantId)
    {
        var product = new CustomerProductDto
        {
            OpeningDate = DateTime.Now,
            MerchantId = merchantId,
            ProductType = ProductType.PF,
            CustomerProductStatus = CustomerProductStatus.Active
        };

        var customerRequest = CreateCustomerRequest(command);

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
    private CreateCustomerRequest CreateCustomerRequest(CreateBoaMerchantCommand command)
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
        address.District = command.Customer.DistrictName;
        address.CountryId = command.Customer.Country;
        address.Country = command.Customer.CountryName;
        address.City = command.Customer.CityName;
        address.CityId = command.Customer.City;
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
    private static List<CustomerPhoneDto> CreatePhonesDto(CreateBoaMerchantCommand command)
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
    private static List<CustomerEmailDto> CreateEmailsDto(CreateBoaMerchantCommand command)
    {
        var emailList = new List<CustomerEmailDto>();

        if (!string.IsNullOrEmpty(command.Customer.AuthorizedPerson.CompanyEmail))
        {
            var customerCompanyEmail = new CustomerEmailDto()
            {
                Email = command.Customer.AuthorizedPerson.CompanyEmail,
                EmailType = HttpProviders.CustomerManagement.Models.Enums.EmailType.Company,
                Primary = false,
            };
            emailList.Add(customerCompanyEmail);
        }

        var customerIndividualEmail = new CustomerEmailDto()
        {
            Email = command.Customer.AuthorizedPerson.Email,
            EmailType = HttpProviders.CustomerManagement.Models.Enums.EmailType.Individual,
            Primary = true,
        };
        emailList.Add(customerIndividualEmail);

        return emailList;
    }

    public async Task<BoaMerchantDto> GetMerchantByNumberAsync(string merchantNumber)
    {
        var merchant = await _context.Merchant.Include(b => b.Customer)
            .ThenInclude(b => b.AuthorizedPerson).Include(b => b.MerchantBankAccounts.Where(b => b.RecordStatus == RecordStatus.Active))
            .Include(b => b.MerchantVposList.Where(b => b.RecordStatus == RecordStatus.Active)
                .OrderBy(b => b.Priority))
            .ThenInclude(b => b.Vpos).Include(b => b.MerchantIntegrator)
            .Include(b => b.MerchantScores).Include(b => b.TechnicalContact)
            .Include(b => b.MerchantDocuments.Where(b => b.RecordStatus == RecordStatus.Active && b.MerchantTransactionId == null)
                .OrderByDescending(b => b.UpdateDate))
            .Include(b => b.MerchantUsers.Where(b => b.RecordStatus == RecordStatus.Active))
            .Include(b => b.MerchantLimits.Where(b => b.RecordStatus == RecordStatus.Active))
            .Include(b => b.MerchantWallets.Where(a => a.RecordStatus == RecordStatus.Active))
            .Include(b => b.MerchantBusinessPartner.Where(a => a.RecordStatus == RecordStatus.Active))
            .FirstOrDefaultAsync(b => b.Number == merchantNumber);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), merchantNumber);
        }

        return _mapper.Map<BoaMerchantDto>(merchant);
    }

}