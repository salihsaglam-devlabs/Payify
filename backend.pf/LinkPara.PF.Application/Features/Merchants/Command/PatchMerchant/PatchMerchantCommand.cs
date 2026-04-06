using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
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
using LinkPara.HttpProviders.KKB;
using LinkPara.HttpProviders.KKB.Models;
using LinkPara.HttpProviders.KPS;
using LinkPara.HttpProviders.KPS.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Customers;
using LinkPara.PF.Application.Commons.Models.IdentityModels;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Authorization.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using LinkPara.HttpProviders.Location;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using LinkPara.SharedModels.Boa.Enums;

namespace LinkPara.PF.Application.Features.Merchants.Command.PatchMerchant;

public class PatchMerchantCommand : IRequest<UpdateMerchantRequest>
{
    public Guid Id { get; set; }
    public JsonPatchDocument<UpdateMerchantRequest> Merchant { get; set; }
}

public class PatchMerchantCommandHandler : IRequestHandler<PatchMerchantCommand, UpdateMerchantRequest>
{
    private readonly IGenericRepository<Merchant> _repository;
    private readonly IGenericRepository<PricingProfile> _pricingProfileRepository;
    private readonly IMapper _mapper;
    private readonly IMerchantService _merchantService;
    private readonly ILogger<Merchant> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IUserService _userService;
    private readonly IVaultClient _vaultClient;
    private readonly IStringLocalizer _localizer;
    private readonly IIksPfService _iksPfService;
    private readonly IParameterService _parameterService;
    private readonly IKKBService _kkbService;
    private readonly IKpsService _kpsService;
    private readonly ISearchService _searchService;
    private readonly IGenericRepository<MerchantBankAccount> _merchantBankAccountRepository;
    private readonly IGenericRepository<MerchantWallet> _merchantWalletRepository;
    private readonly ICustomerService _customerService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ILocationService _locationService;
    private readonly IBus _bus;


    public PatchMerchantCommandHandler(IGenericRepository<Merchant> repository,
        IGenericRepository<PricingProfile> pricingProfileRepository,
        IMapper mapper,
        IMerchantService merchantService, ILogger<Merchant> logger,
        IAuditLogService auditLogService, IUserService userService,
        IVaultClient vaultClient,
        IStringLocalizerFactory factory,
        IIksPfService iksPfService,
        IParameterService parameterService,
        IKKBService kkbService,
        IKpsService kpsService, ISearchService searchService,
        IGenericRepository<MerchantBankAccount> merchantBankAccountRepository,
        IGenericRepository<MerchantWallet> merchantWalletRepository,
        ICustomerService customerService,
        IApplicationUserService applicationUserService,
        ILocationService locationService,
        IBus bus)
    {
        _repository = repository;
        _pricingProfileRepository = pricingProfileRepository;
        _merchantBankAccountRepository = merchantBankAccountRepository;
        _mapper = mapper;
        _merchantService = merchantService;
        _logger = logger;
        _auditLogService = auditLogService;
        _userService = userService;
        _vaultClient = vaultClient;
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
        _iksPfService = iksPfService;
        _parameterService = parameterService;
        _kkbService = kkbService;
        _kpsService = kpsService;
        _searchService = searchService;
        _customerService = customerService;
        _applicationUserService = applicationUserService;
        _locationService = locationService;
        _bus = bus;
        _merchantWalletRepository = merchantWalletRepository;
    }

    public async Task<UpdateMerchantRequest> Handle(PatchMerchantCommand request, CancellationToken cancellationToken)
    {
        var merchant = await _repository.GetAll().Include(b => b.Customer)
           .ThenInclude(b => b.AuthorizedPerson)
           .Include(b => b.MerchantBankAccounts.Where(a => a.RecordStatus == RecordStatus.Active))
           .Include(b => b.MerchantWallets.Where(a => a.RecordStatus == RecordStatus.Active))
           .Include(b => b.MerchantVposList.Where(a => a.RecordStatus == RecordStatus.Active)
           .OrderBy(a => a.Priority)).ThenInclude(c => c.Vpos)
           .Include(b => b.MerchantIntegrator)
           .Include(b => b.MerchantScores).Include(b => b.TechnicalContact)
           .Include(b => b.MerchantDocuments.Where(b => b.RecordStatus == RecordStatus.Active)
           .OrderByDescending(a => a.UpdateDate))
           .Include(b => b.MerchantUsers.Where(a => a.RecordStatus == RecordStatus.Active))
           .Include(b => b.MerchantBusinessPartner)
           .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken: cancellationToken);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), request.Id);
        }

        var isIksEnabled =
            await _vaultClient.GetSecretValueAsync<bool>("SharedSecrets", "ServiceState", "IksEnabled");

        var isKkbEnabled =
            await _vaultClient.GetSecretValueAsync<bool>("SharedSecrets", "ServiceState", "KkbEnabled");

        var isKpsEnabled =
            await _vaultClient.GetSecretValueAsync<bool>("SharedSecrets", "ServiceState", "PFKpsEnabled");

        UpdateMerchantRequest merchantMap = new();
        try
        {
            if (request.Merchant.Operations.Any(b => b.OperationType == OperationType.Add))
            {
                request = ReplacePatch(request);
            }

            if (isIksEnabled)
            {
                await UpdateIksTerminalAsync(request, merchant);
            }

            if (request.Merchant.Operations.Any(b => b.OperationType == OperationType.Replace))
            {
                await CheckValidationsAsync(request, merchant, isKkbEnabled, isKpsEnabled);
            }

            var oldTitle = merchant.Customer.CommercialTitle;
            var oldStatus = merchant.MerchantStatus;
            var oldAuthorizedPerson = $"{merchant.Customer.AuthorizedPerson.Name}{merchant.Customer.AuthorizedPerson.Surname}";
            var oldPricingProfileNumber = merchant.PricingProfileNumber;
            
            merchantMap = _mapper.Map<UpdateMerchantRequest>(merchant);

            request.Merchant.ApplyTo(merchantMap);
            _mapper.Map(merchantMap, merchant);
            await CheckUserAsync(merchant);
            
            var identityChecked = false;
            foreach (var item in request.Merchant.Operations)
            {
                var split = item.path.Split('/');

                if (isKpsEnabled && split.Contains("authorizedPerson") && !identityChecked)
                {
                    var isValid = await ValidateIdentityAsync(merchant);
                    if (!isValid)
                    {
                        _logger.LogError("PatchMerchantCommandError: Identity is not valid");
                        throw new IdentityValidationFailedException();
                    }
                    identityChecked = true;
                }

            }
            
            if (oldPricingProfileNumber != merchant.PricingProfileNumber)
            {
                var inUsePricingProfileType = await _pricingProfileRepository.GetAll()
                    .Where(w =>
                        w.PricingProfileNumber == merchant.PricingProfileNumber &&
                        w.ProfileStatus == ProfileStatus.InUse)
                    .Select(s => s.ProfileType)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if ((merchant.MerchantType is MerchantType.EasyMerchant or MerchantType.SubMerchant &&
                     inUsePricingProfileType == ProfileType.Standard) ||
                    (merchant.MerchantType is MerchantType.MainMerchant or MerchantType.StandartMerchant &&
                     inUsePricingProfileType == ProfileType.SubMerchant))
                {
                    throw new InvalidPricingProfileTypeException();
                }
            }
            
            var pricingProfile = await _pricingProfileRepository.GetAll()
                .FirstOrDefaultAsync(f => 
                    f.PricingProfileNumber == merchant.PricingProfileNumber && 
                    f.IsPaymentToMainMerchant == merchant.IsPaymentToMainMerchant &&
                    f.RecordStatus == RecordStatus.Active, cancellationToken);

            if (pricingProfile is null)
            {
                throw new InvalidMerchantPricingProfileException();
            }

            var IsBlacklistCheckEnabled = await _vaultClient.GetSecretValueAsync<bool>("SharedSecrets", "ServiceState", "BlacklistEnabled");
            if (IsBlacklistCheckEnabled)
            {
                if (!oldTitle.Equals(merchant.Customer.CommercialTitle))
                {
                    SearchByNameRequest searchTitleRequest = new()
                    {
                        Name = merchant.Customer.CommercialTitle,
                        SearchType = SearchType.Corporate,
                        FraudChannelType = FraudChannelType.Backoffice
                    };

                    await UserBlacklistControlAsync(searchTitleRequest);
                }

                if (!oldAuthorizedPerson.Equals($"{merchant.Customer.AuthorizedPerson.Name}{merchant.Customer.AuthorizedPerson.Surname}"))
                {
                    SearchByNameRequest searchRequest = new()
                    {
                        Name = $"{merchant.Customer.AuthorizedPerson.Name} {merchant.Customer.AuthorizedPerson.Surname}",
                        BirthYear = merchant.Customer.AuthorizedPerson.BirthDate.Year.ToString(),
                        SearchType = SearchType.Corporate,
                        FraudChannelType = FraudChannelType.Backoffice
                    };

                    await UserBlacklistControlAsync(searchRequest);
                }
            }

            if ((oldStatus == MerchantStatus.PendingIKS || oldStatus == MerchantStatus.Pending || oldStatus == MerchantStatus.Draft || oldStatus == MerchantStatus.RiskApproval) && merchant.MerchantStatus == MerchantStatus.Active)
            {
                var createCustomerResponse = await CreateCustomerAsync(merchant);
                var customerId = merchant.CustomerId;

                merchant.Customer.CustomerNumber = createCustomerResponse.CustomerNumber;
                merchant.Customer.CustomerId = createCustomerResponse.CustomerId;

                await PublishCustomerNumberUpdateAsync(createCustomerResponse, customerId);
            }

            if (merchant.MerchantStatus == MerchantStatus.Reject)
            {
                merchant.Customer.CustomerStatus = CustomerStatus.Rejected;
            }

            if (merchant.MerchantStatus == MerchantStatus.Active)
            {
                merchant.AnnulmentAdditionalInfo = null;
                merchant.Customer.CustomerStatus = CustomerStatus.Active;

                await CreateUserAsync(merchant);
            }

            if ((merchant.MerchantStatus == MerchantStatus.Active || merchant.MerchantStatus == MerchantStatus.Closed)
                && isIksEnabled)
            {
                await _iksPfService.IKSSaveMerchantAsync(merchant);
            }

            if (merchant.PosType == PosType.Physical)
            {
                merchant.Is3dRequired = false;
                merchant.IsManuelPayment3dRequired = false;
                merchant.HalfSecureAllowed = false;
                merchant.InstallmentAllowed = false;
                merchant.InternationalCardAllowed = false;
                merchant.PreAuthorizationAllowed = false;
                merchant.IsLinkPayment3dRequired = false;
                merchant.PaymentReturnAllowed = false;
                merchant.PaymentReverseAllowed = false;
                merchant.IsHostedPayment3dRequired = false;
                merchant.IsReturnApproved = false;
                merchant.IsExcessReturnAllowed = false;
                merchant.IsCvvPaymentAllowed = false;
                merchant.InsurancePaymentAllowed = false;
                merchant.IntegrationMode = IntegrationMode.Unknown;
                merchant.HostingTaxNo = string.Empty;
                merchant.HostingTradeName = string.Empty;
                merchant.HostingUrl = string.Empty;
            }

            await _merchantService.PatchMerchant(merchant);

            var operation = request.Merchant.Operations.FirstOrDefault(b => b.value?.ToString() == "RiskApproval");
            if (operation != null && operation.path.ToLower().Contains("merchantstatus"))
            {
                await SendMailToRiskGroup(merchant);
            }

            List<MerchantHistoryDto> merchantHistoryList = new();
            foreach (var item in request.Merchant.Operations)
            {
                MerchantHistoryDto merchantHistory = new();

                merchantHistory.MerchantId = request.Id;
                merchantHistory.NewData = item.value is null ? string.Empty : item.value.ToString();
                merchantHistory.OldData = item.from is null ? string.Empty : item.from.ToString();
                merchantHistory.Detail = item.path is null ? string.Empty : item.path.ToString();
                if (!string.IsNullOrEmpty(item.from))
                    merchantHistory.PermissionOperationType = PermissionOperationType.Update;
                else
                    merchantHistory.PermissionOperationType = PermissionOperationType.Create;

                merchantHistoryList.Add(merchantHistory);
            }

            await _merchantService.UpdateMerchantHistory(merchantHistoryList);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "MerchantPatchError : {Exception}", exception);
            if (exception.Message.Contains("IKSGeneralError"))
            {
                throw new IKSGeneralException(_localizer.GetString("IKSGeneralException").Value);
            }
            throw;
        }
        merchantMap.MerchantDocuments = merchantMap.MerchantDocuments.Where(b => b.RecordStatus == RecordStatus.Active).ToList();

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "UpdateMerchant",
            SourceApplication = "PF",
            Resource = "Merchant",
            Details = new Dictionary<string, string>
            {
                   {"Id", merchant.Id.ToString() },
                   {"Name", merchant.Name},
                   {"Number", merchant.Number}
            }
        });

        return merchantMap;
    }
    private async Task<CreateCustomerResponse> CreateCustomerAsync(Merchant merchant)
    {
        var product = new CustomerProductDto
        {
            OpeningDate = DateTime.Now,
            MerchantId = merchant.Id,
            ProductType = ProductType.PF,
            CustomerProductStatus = CustomerProductStatus.Inactive
        };

        var customerRequest = CreateCustomerRequest(merchant);

        customerRequest.CustomerType = merchant.Customer.CompanyType switch
        {
            CompanyType.Individual => CustomerType.Individual,
            CompanyType.Corporate => CustomerType.Corporate,
            CompanyType.Enterprise => CustomerType.Enterprise,
            _ => customerRequest.CustomerType
        };

        customerRequest.CreateCustomerProducts.Add(product);

        return await _customerService.CreateCustomerAsync(customerRequest);
    }
    private CreateCustomerRequest CreateCustomerRequest(Merchant merchant)
    {

        var address = new CustomerAddressDto();
        var productList = new List<CustomerProductDto>();
        var addressList = new List<CustomerAddressDto>();
        var customerRequest = new CreateCustomerRequest();

        if (merchant.Customer.CompanyType == CompanyType.Individual)
        {
            customerRequest.FirstName = merchant.Customer.AuthorizedPerson.Name;
            customerRequest.LastName = merchant.Customer.AuthorizedPerson.Surname;
            customerRequest.BirthDate = merchant.Customer.AuthorizedPerson.BirthDate;
            customerRequest.NationCountry = "TR";
            customerRequest.NationCountryId = "TR";
            customerRequest.IdentityNumber = merchant.Customer.AuthorizedPerson.IdentityNumber;
            customerRequest.DocumentType = DocumentType.Identity;
        }
        customerRequest.CommercialTitle = merchant.Customer.CommercialTitle;
        customerRequest.TradeRegistrationNumber = merchant.Customer.TradeRegistrationNumber;
        customerRequest.TaxNumber = merchant.Customer.TaxNumber;
        customerRequest.TaxAdministration = merchant.Customer.TaxAdministration;
        customerRequest.UserId = _applicationUserService.ApplicationUserId;

        address.DistrictId = merchant.Customer.District;
        address.District = merchant.Customer.DistrictName;
        address.CountryId = merchant.Customer.Country;
        address.Country = merchant.Customer.CountryName;
        address.City = merchant.Customer.CityName;
        address.CityId = merchant.Customer.City;
        address.Address = merchant.Customer.Address;
        address.PostalCode = merchant.Customer.PostalCode;
        address.AddressType = AddressType.Company;
        addressList.Add(address);

        customerRequest.CreateCustomerAddresses = addressList;
        customerRequest.CreateCustomerProducts = productList;
        customerRequest.CreateCustomerPhones = CreatePhonesDto(merchant);
        customerRequest.CreateCustomerEmails = CreateEmailsDto(merchant);

        return customerRequest;
    }
    private static List<CustomerPhoneDto> CreatePhonesDto(Merchant merchant)
    {
        var phoneList = new List<CustomerPhoneDto>();
        var customerIndividualPhone = new CustomerPhoneDto()
        {
            PhoneCode = merchant.PhoneCode,
            PhoneNumber = merchant.Customer.AuthorizedPerson.MobilePhoneNumber,
            Primary = true,
            PhoneType = PhoneType.Individual,
        };
        var customerCompanyPhone = new CustomerPhoneDto()
        {
            PhoneCode = merchant.PhoneCode,
            PhoneNumber = merchant.Customer.AuthorizedPerson.CompanyPhoneNumber,
            Primary = false,
            PhoneType = PhoneType.Company,
        };

        phoneList.Add(customerIndividualPhone);
        phoneList.Add(customerCompanyPhone);

        return phoneList;
    }
    private static List<CustomerEmailDto> CreateEmailsDto(Merchant merchant)
    {
        var emailList = new List<CustomerEmailDto>();
        var customerCompanyEmail = new CustomerEmailDto()
        {
            Email = merchant.Customer.AuthorizedPerson.CompanyEmail,
            EmailType = HttpProviders.CustomerManagement.Models.Enums.EmailType.Company,
            Primary = true,

        };
        var customerIndividualEmail = new CustomerEmailDto()
        {
            Email = merchant.Customer.AuthorizedPerson.Email,
            EmailType = HttpProviders.CustomerManagement.Models.Enums.EmailType.Individual,
            Primary = false,

        };
        emailList.Add(customerIndividualEmail);
        emailList.Add(customerCompanyEmail);

        return emailList;
    }
    private async Task PublishCustomerNumberUpdateAsync(CreateCustomerResponse response, Guid customerId)
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.UpdateCustomerNumber"));
        await endpoint.Send(new UpdateCustomerNumber
        {
            CustomerId = customerId,
            CustomerManagementId = response.CustomerId,
            CustomerNumber = response.CustomerNumber
        }, tokenSource.Token);
    }
    private async Task UserBlacklistControlAsync(SearchByNameRequest searchRequest)
    {
        var matchRate = await _parameterService.GetParameterAsync("FraudParameters", "MatchRate");

        var blackListTitleControl = await _searchService.GetSearchByName(searchRequest);

        if ((blackListTitleControl.MatchStatus == MatchStatus.PotentialMatch || blackListTitleControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListTitleControl.MatchRate >= Convert.ToInt32(matchRate.ParameterValue))
        {
            var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

            var exceptionMessage = _localizer.GetString("UserInBlacklistException");

            throw new UserInBlacklistException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
        }
    }
    private async Task UpdateIksTerminalAsync(PatchMerchantCommand request, Merchant merchant)
    {
        if (request.Merchant.Operations.Any(b => b.OperationType == OperationType.Replace)
            && merchant.MerchantStatus == MerchantStatus.Active)
        {
            var merchantVposReplaceList = request.Merchant.Operations.Where(b => b.OperationType == OperationType.Replace
                && b.path.Contains("merchantVposList")
                && b.path.Contains("vposId")).ToList();

            foreach (var item in merchantVposReplaceList)
            {
                var oldVposId = Guid.Parse(item.from);
                var oldMerchantVpos = merchant.MerchantVposList.FirstOrDefault(x => x.VposId == oldVposId);
                if (oldMerchantVpos is not null)
                {
                    await _iksPfService.IKSUpdateTerminalAsync(merchant, oldMerchantVpos);
                }
            }

            var passiveMerchantVposReplaceList = request.Merchant.Operations.Where(b =>
                b.OperationType == OperationType.Replace
                && b.path.Contains("merchantVposList")
                && b.path.Contains("recordStatus")).ToList();

            foreach (var item in passiveMerchantVposReplaceList)
            {
                var oldMerchantVpos = merchant.MerchantVposList[ExtractANumber(item.path)];
                if (oldMerchantVpos is not null)
                {
                    oldMerchantVpos.RecordStatus = RecordStatus.Passive;
                    oldMerchantVpos.TerminalStatus = TerminalStatus.Passive;
                    await _iksPfService.IKSUpdateTerminalAsync(merchant, oldMerchantVpos);
                }
            }
        }
    }
    private async Task<bool> ValidateIbanAsync(Merchant merchant, string newIban)
    {
        var validateIban = new ValidateIbanRequest()
        {
            Iban = newIban,
            IdentityNo = merchant.Customer.CompanyType == CompanyType.Individual
        ? merchant.Customer.AuthorizedPerson.IdentityNumber
        : merchant.Customer.TaxNumber,
        };
        var res = await _kkbService.ValidateIban(validateIban);
        return res.IsValid;
    }
    private async Task<bool> ValidateIdentityAsync(Merchant merchant)
    {
        var validateIdentity = new ValidateIdentityRequest()
        {
            IdentityNo = merchant.Customer.AuthorizedPerson.IdentityNumber,
            Name = merchant.Customer.AuthorizedPerson.Name,
            Surname = merchant.Customer.AuthorizedPerson.Surname,
            BirthDate = merchant.Customer.AuthorizedPerson.BirthDate
        };
        var res = await _kpsService.ValidateIdentity(validateIdentity);
        return res.IsValid;
    }
    private async Task CheckValidationsAsync(PatchMerchantCommand request, Merchant merchant, bool isKkbEnabled, bool isKpsEnabled)
    {
        var bankAccountOperations =
            request.Merchant.Operations.Where(s => s.path.Contains("merchantBankAccounts")).ToList();
        if (bankAccountOperations.Any(s => s.path.Contains("iban")))
        {
            var bankAccountIbanOperation = bankAccountOperations.FirstOrDefault(s => s.path.Contains("iban"));
            var newIban = bankAccountIbanOperation?.value.ToString()?.Trim().Replace(" ", string.Empty);
            var activeMerchantBankAccount = merchant.MerchantBankAccounts.FirstOrDefault(m => m.RecordStatus == RecordStatus.Active);
            var bankAccountBankCodeOperation = bankAccountOperations.FirstOrDefault(s => s.path.Contains("bankCode"));
            var newBankCode = bankAccountBankCodeOperation?.value is null ? -1 : Convert.ToInt32(bankAccountBankCodeOperation?.value);
            if (activeMerchantBankAccount is null && bankAccountBankCodeOperation is null)
            {
                throw new InvalidOperationException();
            }
            
            if (!string.IsNullOrEmpty(newIban) && isKkbEnabled)
            {
                var isValid = await ValidateIbanAsync(merchant, newIban);
                if (!isValid)
                {
                    _logger.LogError("PatchMerchantCommandError: IBAN is not valid : {NewIban}", newIban);
                    throw new IbanValidationFailedException();
                }
            }

            if (!string.IsNullOrEmpty(newIban) || newBankCode != -1)
            {
                merchant.MerchantBankAccounts.ForEach(b => { b.RecordStatus = RecordStatus.Passive; });

                var newMerchantBankAccount = new MerchantBankAccount
                {
                    Iban = newIban,
                    BankCode = newBankCode,
                    MerchantId = merchant.Id,
                    RecordStatus = RecordStatus.Active,
                    CreateDate = DateTime.Now,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                };
            
                merchant.MerchantBankAccounts.Add(newMerchantBankAccount);
                merchant.MerchantBankAccounts = merchant.MerchantBankAccounts.OrderByDescending(s => s.CreateDate).ToList();

                await _merchantBankAccountRepository.AddAsync(newMerchantBankAccount);
                await _merchantBankAccountRepository.UpdateRangeAsync(merchant.MerchantBankAccounts);
            }
            else
            {
                activeMerchantBankAccount.RecordStatus = RecordStatus.Passive;
                await _merchantBankAccountRepository.UpdateAsync(activeMerchantBankAccount);
                request.Merchant.Operations.FirstOrDefault(b => b.path.Contains("bankCode")).value = activeMerchantBankAccount.BankCode;
            }

            await _bus.Publish(new MerchantIbanChanged
            {
                MerchantName = merchant.Name,
                MerchantNumber = merchant.Number,
                OldIban = activeMerchantBankAccount?.Iban,
                NewIban = newIban
            }, CancellationToken.None);

            request.Merchant.Operations.FirstOrDefault(b => b.path.Contains("iban")).value = newIban;
        }

        if (bankAccountOperations.Any(s => s.path.Contains("bankCode")))
        {
            var activeMerchantBankAccount = merchant.MerchantBankAccounts.FirstOrDefault(m => m.RecordStatus == RecordStatus.Active);
            var bankAccountBankCodeOperation = bankAccountOperations.FirstOrDefault(s => s.path.Contains("bankCode"));

            if (activeMerchantBankAccount is not null)
            {
                var newBankCode = Convert.ToInt32(bankAccountBankCodeOperation?.value);

                if (newBankCode != -1)
                {
                    activeMerchantBankAccount.BankCode = newBankCode;
                }
                else
                {
                    activeMerchantBankAccount.RecordStatus = RecordStatus.Passive;
                    request.Merchant.Operations.FirstOrDefault(b => b.path.Contains("bankCode")).value = activeMerchantBankAccount.BankCode;
                }
                
                await _merchantBankAccountRepository.UpdateAsync(activeMerchantBankAccount);
                await _merchantBankAccountRepository.UpdateRangeAsync(merchant.MerchantBankAccounts);
            }
        }

        if (request.Merchant.Operations.Any(s => s.path.Contains("walletNumber")))
        {
            var walletOperation = request.Merchant.Operations.FirstOrDefault(s => s.path.Contains("walletNumber"));
            var newWallet = walletOperation?.value.ToString()?.Trim().Replace(" ", string.Empty);

            merchant.MerchantWallets.ForEach(b => { b.RecordStatus = RecordStatus.Passive; });

            var newMerchantWallet = new MerchantWallet
            {
                WalletNumber = newWallet,
                MerchantId = merchant.Id,
                RecordStatus = RecordStatus.Active,
                CreateDate = DateTime.Now
            };
            await _merchantWalletRepository.AddAsync(newMerchantWallet);
            await _merchantWalletRepository.UpdateRangeAsync(merchant.MerchantWallets);

            merchant.MerchantWallets.Add(newMerchantWallet);
            merchant.MerchantWallets = merchant.MerchantWallets.OrderByDescending(s => s.CreateDate).ToList();

            request.Merchant.Operations.FirstOrDefault(b => b.path.Contains("walletNumber")).value = newWallet;
        }
        var customerOperations =
            request.Merchant.Operations.Where(s => s.path.Contains("customer")).ToList();
        if (customerOperations.Count > 0)
        {
            var address = customerOperations.FirstOrDefault(s => s.path.Equals("/customer/address"));
            var postalCode = customerOperations.FirstOrDefault(s => s.path.Equals("/customer/postalCode"));
            var district = customerOperations.FirstOrDefault(s => s.path.Equals("/customer/district"));
            var districtName = customerOperations.FirstOrDefault(s => s.path.Equals("/customer/districtName"));
            var city = customerOperations.FirstOrDefault(s => s.path.Equals("/customer/city"));
            var cityName = customerOperations.FirstOrDefault(s => s.path.Equals("/customer/cityName"));

            if (address is not null || postalCode is not null || district is not null || city is not null)
            {
                var addAddressRequest = new AddAddressRequest
                {
                    CustomerId = merchant.Customer.CustomerId,
                    CountryId = merchant.Customer.Country,
                    Country = merchant.Customer.CountryName,
                    CityId = city is null ? merchant.Customer.City : Convert.ToInt32(city?.value),
                    City = cityName is null ? merchant.Customer.CityName : cityName?.value.ToString(),
                    DistrictId = district is null ? merchant.Customer.District : Convert.ToInt32(district?.value),
                    District = districtName is null ? merchant.Customer.DistrictName : districtName?.value.ToString(),
                    PostalCode = postalCode is null ? merchant.Customer.PostalCode : postalCode?.value.ToString(),
                    Address = address is null ? merchant.Customer.Address : address?.value.ToString(),
                    Primary = true,
                    CityIso2 = null,
                    CountryIso2 = null,
                    AddressType = AddressType.Company
                };
                var getCities = await _locationService.GetCityByCode(addAddressRequest.CountryId);
                var getCityIso2 = getCities.FirstOrDefault(x => x.CityCode == merchant.Customer.City);
                addAddressRequest.CityIso2 = getCityIso2?.Iso2;
                await _customerService.AddCustomerAddressAsync(addAddressRequest);
            }
        }
    }
    private static PatchMerchantCommand ReplacePatch(PatchMerchantCommand request)
    {
        foreach (var item in request.Merchant.Operations)
        {
            var split = item.path.Split('/');
            if (split.Contains("merchantVposList") && split.Contains("recordStatus"))
            {
                continue;
            }

            for (int i = 0; i <= split.Length - 1; i++)
            {
                if (split.Length - 1 == i)
                {
                    item.path = item.path.Replace(split[i], "-");
                }
            }
        }
        return request;
    }
    private async Task CheckUserAsync(Merchant merchant)
    {
        var users = merchant.MerchantUsers.Where(b => b.UserId == Guid.Empty && b.RecordStatus == RecordStatus.Active).ToList();

        if (users.Any())
        {
            foreach (var item in users)
            {
                var userPhone = await _userService.GetAllUsersAsync(new GetUsersRequest
                {
                    PhoneNumber = item.MobilePhoneNumber,
                    UserType = UserType.Corporate,
                });

                var userEmail = await _userService.GetAllUsersAsync(new GetUsersRequest
                {
                    Email = item.Email,
                    UserType = UserType.Corporate,
                });
                if (userPhone.Items.Any() || userEmail.Items.Any())
                {
                    throw new AlreadyInUseException($"{item.Name} {item.Surname}");
                }
            }
        }
    }
    private async Task<Merchant> CreateUserAsync(Merchant merchant)
    {
        var users = merchant.MerchantUsers.Where(b => b.UserId == Guid.Empty && b.RecordStatus == RecordStatus.Active).ToList();

        if (users.Any())
        {
            foreach (var item in users)
            {
                CreateUserRequest createUserRequest = new()
                {
                    Email = item.Email,
                    FirstName = item.Name,
                    LastName = item.Surname,
                    BirthDate = item.BirthDate,
                    PhoneCode = merchant.PhoneCode,
                    PhoneNumber = item.MobilePhoneNumber,
                    UserType = UserType.Corporate,
                    Roles = new List<Guid> { Guid.Parse(item.RoleId) },
                    UserName = string.Concat(UserTypePrefix.Corporate, merchant.PhoneCode.Replace("+", ""), item.MobilePhoneNumber),
                    IsBlacklistControl = false
                };

                var result = await _userService.CreateUserAsync(createUserRequest);
                item.UserId = result.UserId;
                item.MerchantId = merchant.Id;
            }
        }
        return merchant;
    }
    private async Task SendMailToRiskGroup(Merchant merchant)
    {
        try
        {
            await _bus.Publish(new RiskApproval
            {
                MerchantName = $"{merchant.Name}"
            });
        }
        catch (Exception e)
        {
            _logger.LogError("SendRiskApprovalMailError Exception : {e}", e);
        }
    }
    private static int ExtractANumber(string value)
    {
        string temp = string.Empty;
        int result = 0;

        for (int i = 0; i < value.Length; i++)
        {
            if (Char.IsDigit(value[i]))
                temp += value[i];
        }

        if (temp.Length > 0)
            result = int.Parse(temp);

        return result;
    }
}