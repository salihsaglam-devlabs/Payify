using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.KKB;
using LinkPara.HttpProviders.KKB.Models;
using LinkPara.HttpProviders.KPS;
using LinkPara.HttpProviders.KPS.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.Banks;
using LinkPara.PF.Application.Features.MerchantPools;
using LinkPara.PF.Application.Features.MerchantPools.Command.ApproveMerchantPool;
using LinkPara.PF.Application.Features.MerchantPools.Command.SaveMerchantPool;
using LinkPara.PF.Application.Features.MerchantPools.Queries.GetFilterMerchantPool;
using LinkPara.PF.Application.Features.MerchantPools.Queries.GetMerchantPoolById;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Transactions;
using LinkPara.HttpProviders.IKS.Models.Enums;
using LinkPara.PF.Application.Commons.Helpers;

namespace LinkPara.PF.Infrastructure.Services;

public class MerchantPoolService : IMerchantPoolService
{
    private readonly ILogger<MerchantPoolService> _logger;
    private readonly IGenericRepository<MerchantPool> _repository;
    private readonly IGenericRepository<Bank> _bankRepository;
    private readonly PfDbContext _context;
    private readonly IMapper _mapper;
    private readonly IApiKeyGenerator _apiKeyGenerator;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ISearchService _searchService;
    private readonly IGenericRepository<MerchantCounter> _counterRepository;
    private readonly IParameterService _parameterService;
    private readonly IVaultClient _vaultClient;
    private readonly IStringLocalizer _localizer;
    private readonly IAccountingService _accountingService;
    private readonly IKKBService _kkbService;
    private readonly IKpsService _kpsService;

    private const string Prefix = "11";

    public MerchantPoolService(ILogger<MerchantPoolService> logger,
        IGenericRepository<MerchantPool> repository,
        IGenericRepository<Bank> bankRepository,
        PfDbContext context,
        IMapper mapper,
        IApiKeyGenerator apiKeyGenerator,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IApplicationUserService applicationUserService,
        ISearchService searchService,
        IGenericRepository<MerchantCounter> counterRepository,
        IParameterService parameterService,
        IVaultClient vaultClient,
        IStringLocalizerFactory factory,
        IAccountingService accountingService,
        IKKBService kkbService,
        IKpsService kpsService)
    {
        _logger = logger;
        _repository = repository;
        _bankRepository = bankRepository;
        _context = context;
        _mapper = mapper;
        _apiKeyGenerator = apiKeyGenerator;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _applicationUserService = applicationUserService;
        _searchService = searchService;
        _counterRepository = counterRepository;
        _parameterService = parameterService;
        _vaultClient = vaultClient;
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
        _accountingService = accountingService;
        _kkbService = kkbService;
        _kpsService = kpsService;
    }

    public async Task<ApproveMerchantPoolResponse> ApproveMerchantPool(ApproveMerchantPoolCommand request)
    {
        var merchantPool = await _repository.GetByIdAsync(request.MerchantPoolId);

        if (merchantPool is null)
        {
            throw new NotFoundException(nameof(merchantPool), request.MerchantPoolId);
        }

        var IsBlacklistCheckEnabled =
            _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "BlacklistEnabled");

        if (IsBlacklistCheckEnabled)
        {
            await UserBlacklistControlAsync(merchantPool);
        }

        Guid newMerchantId = Guid.Empty;

        switch (request.IsApprove)
        {
            case true:
                newMerchantId = await CreateNewMerchantAsync(merchantPool);
                break;
            default:
                await RejectMerchantRequestAsync(merchantPool, request);
                break;
        }

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "ApproveMerchantPool",
                SourceApplication = "PF",
                Resource = "MerchantPool",
                UserId = request.UserId,
                Details = new Dictionary<string, string>
                {
                    {"MerchantPoolId", request.MerchantPoolId.ToString()},
                    {"RejectReason", request.RejectReason},
                    {"ParameterValue", request.ParameterValue}
                }
            });

        return new ApproveMerchantPoolResponse
        {
            IsApproved = request.IsApprove,
            MerchantId = newMerchantId
        };
    }

    private async Task UserBlacklistControlAsync(MerchantPool merchantPool)
    {
        var matchRate = await _parameterService.GetParameterAsync("FraudParameters", "MatchRate");

        SearchByNameRequest searchRequest = new()
        {
            Name = $"{merchantPool.AuthorizedPersonName} {merchantPool.AuthorizedPersonSurname}",
            BirthYear = merchantPool.AuthorizedPersonBirthDate.Year.ToString(),
            SearchType = SearchType.Corporate,
            FraudChannelType = FraudChannelType.Backoffice
        };

        var blackListControl = await _searchService.GetSearchByName(searchRequest);

        if ((blackListControl.MatchStatus == MatchStatus.PotentialMatch || blackListControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListControl.MatchRate >= Convert.ToInt32(matchRate.ParameterValue))
        {
            await BlacklistResponseAsync(merchantPool);
        }
        else
        {
            SearchByNameRequest searchTitleRequest = new()
            {
                Name = merchantPool.CommercialTitle,
                SearchType = SearchType.Corporate,
                FraudChannelType = FraudChannelType.Backoffice
            };

            var blackListTitleControl = await _searchService.GetSearchByName(searchTitleRequest);

            if ((blackListTitleControl.MatchStatus == MatchStatus.PotentialMatch || blackListTitleControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListTitleControl.MatchRate >= Convert.ToInt32(matchRate.ParameterValue))
            {
                await BlacklistResponseAsync(merchantPool);
            }
        }
    }
    private async Task BlacklistResponseAsync(MerchantPool merchantPool)
    {
        merchantPool.RejectReason = "Fraud";
        merchantPool.ParameterValue = "Fraud";
        merchantPool.MerchantPoolStatus = MerchantPoolStatus.Rejected;
        merchantPool.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();

        await _repository.UpdateAsync(merchantPool);

        var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

        var exceptionMessage = _localizer.GetString("UserInBlacklistException");

        throw new UserInBlacklistException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
    }
    private async Task<Guid> CreateNewMerchantAsync(MerchantPool merchantPool)
    {
        Guid merchantId = default;

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {

                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var contactPerson = PopulateNewContactPerson(merchantPool, parseUserId.ToString());

                await _context.ContactPerson.AddAsync(contactPerson);
                
                var customer = PopulateNewCustomer(merchantPool, contactPerson.Id, parseUserId.ToString());

                await _context.Customer.AddAsync(customer);
                
                var merchantNumber = await GenerateMerchantNumberAsync();

                var defaultMoneyTransferTime = await MoneyTransferHourHelper.GetMoneyTransferHourAsync(_parameterService, _logger);
                
                var newMerchant = new Merchant
                {
                    MerchantStatus = MerchantStatus.Draft,
                    MerchantType = merchantPool.MerchantType,
                    FinancialTransactionAllowed = false,
                    PaymentAllowed = false,
                    Name = merchantPool.MerchantName,
                    WebSiteUrl = merchantPool.WebSiteUrl,
                    MerchantPoolId = merchantPool.Id,
                    AgreementDate = DateTime.Now,
                    MonthlyTurnover = merchantPool.MonthlyTurnover,
                    PhoneCode = merchantPool.PhoneCode,
                    CustomerId = customer.Id,
                    Language = "TR",
                    IntegrationMode = IntegrationMode.Unknown,
                    ApplicationChannel = ApplicationChannel.Sales,
                    PostingPaymentChannel = merchantPool.PostingPaymentChannel,
                    Number = merchantNumber,
                    CreatedBy = parseUserId.ToString(),
                    Information = FraudChannelType.Backoffice.ToString(),
                    IsPaymentToMainMerchant = merchantPool.IsPaymentToMainMerchant,
                    BusinessModel = BusinessModel.W,
                    BusinessActivity = string.Empty,
                    EstablishmentDate = DateTime.MinValue,
                    BranchCount = 0,
                    EmployeeCount = 0,
                    PosType = merchantPool.PosType,
                    MoneyTransferStartHour = merchantPool.MoneyTransferStartHour,
                    MoneyTransferStartMinute = merchantPool.MoneyTransferStartMinute
                };


                if (merchantPool.PosType == PosType.Physical)
                {
                    newMerchant.Is3dRequired = false;
                    newMerchant.IsManuelPayment3dRequired = false;
                    newMerchant.HalfSecureAllowed = false;
                    newMerchant.InstallmentAllowed = false;
                    newMerchant.InternationalCardAllowed = false;
                    newMerchant.PreAuthorizationAllowed = false;
                    newMerchant.IsLinkPayment3dRequired = false;
                    newMerchant.PaymentReturnAllowed = false;
                    newMerchant.PaymentReverseAllowed = false;
                    newMerchant.IsHostedPayment3dRequired = false;
                    newMerchant.IsReturnApproved = false;
                    newMerchant.IsExcessReturnAllowed = false;
                    newMerchant.IsCvvPaymentAllowed = false;
                    newMerchant.InsurancePaymentAllowed = false;
                }

                if (merchantPool.ParentMerchantId.HasValue && merchantPool.ParentMerchantId != Guid.Empty)
                {
                    newMerchant.ParentMerchantId = merchantPool.ParentMerchantId;
                    newMerchant.ParentMerchantName = merchantPool.ParentMerchantName;
                    newMerchant.ParentMerchantNumber = merchantPool.ParentMerchantNumber;
                }

                if (merchantPool.MerchantType == MerchantType.MainMerchant)
                {
                    newMerchant.IsInvoiceCommissionReflected = merchantPool.IsInvoiceCommissionReflected;
                }

                await _context.Merchant.AddAsync(newMerchant);

                if (merchantPool.BankCode != null)
                {
                    var merchantBankAccount = new MerchantBankAccount
                    {
                        Iban = merchantPool.Iban,
                        BankCode = (int)merchantPool.BankCode,
                        MerchantId = newMerchant.Id,
                        RecordStatus = RecordStatus.Active,
                        CreateDate = DateTime.Now,
                        CreatedBy = parseUserId.ToString()
                    };

                    await _context.MerchantBankAccount.AddAsync(merchantBankAccount);
                }

                if (!string.IsNullOrEmpty(merchantPool.WalletNumber))
                {
                    var merchantWallet = new MerchantWallet
                    {
                        WalletNumber = merchantPool.WalletNumber.Trim().Replace(" ", string.Empty),
                        MerchantId = newMerchant.Id,
                        RecordStatus = RecordStatus.Active,
                        CreateDate = DateTime.Now,
                        CreatedBy = parseUserId.ToString()
                    };
                
                    await _context.MerchantWallet.AddAsync(merchantWallet);
                }
                
                
                merchantPool.MerchantPoolStatus = MerchantPoolStatus.Completed;
                merchantPool.LastModifiedBy = parseUserId.ToString();
                _context.MerchantPool.Update(merchantPool);
                
                var merchantApiKeyDto = await _apiKeyGenerator.Generate(newMerchant.Id);

                var merchantApiKey = new MerchantApiKey
                {
                    CreatedBy = parseUserId.ToString(),
                    PrivateKeyEncrypted = merchantApiKeyDto.PrivateKeyEncrypted,
                    PublicKey = merchantApiKeyDto.PublicKey,
                    MerchantId = merchantApiKeyDto.MerchantId,
                    CreateDate = DateTime.Now
                };

                await _context.MerchantApiKey.AddAsync(merchantApiKey);
                await _context.SaveChangesAsync();
                scope.Complete();
                await _accountingService.CreateCustomerAsync(newMerchant, customer, contactPerson);
                merchantId = newMerchant.Id;
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantPoolApproveError : {exception}");

            switch (exception)
            {
                case ApiException apiException:
                    throw new CustomApiException(apiException.Code, apiException.Message);
                default:
                    throw;
            }
        }

        return merchantId;
    }
    private async Task RejectMerchantRequestAsync(MerchantPool merchantPool, ApproveMerchantPoolCommand request)
    {
        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                merchantPool.RejectReason = request.RejectReason;
                merchantPool.ParameterValue = request.ParameterValue;
                merchantPool.MerchantPoolStatus = MerchantPoolStatus.Rejected;
                merchantPool.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();

                await _repository.UpdateAsync(merchantPool);
                scope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantPoolRejectError : {exception}");
            throw;
        }
    }

    private static Customer PopulateNewCustomer(MerchantPool merchantPool, Guid contactPersonId, string appUserId)
    {
        var customer = new Customer
        {
            CommercialTitle = merchantPool.CommercialTitle,
            CompanyType = merchantPool.CompanyType,
            TradeRegistrationNumber = merchantPool.TradeRegistrationNumber,
            TaxAdministration = merchantPool.TaxAdministration,
            TaxNumber = merchantPool.TaxNumber,
            Country = merchantPool.Country,
            CountryName = merchantPool.CountryName,
            City = merchantPool.City,
            CityName = merchantPool.CityName,
            District = merchantPool.District,
            DistrictName = merchantPool.DistrictName,
            PostalCode = merchantPool.PostalCode,
            Address = merchantPool.Address,
            ContactPersonId = contactPersonId,
            CustomerStatus = CustomerStatus.Pending,
            CustomerId = Guid.Empty,
            CustomerNumber = 0,
            CreateDate = DateTime.Now,
            CreatedBy = appUserId
        };
        return customer;
    }
    private static ContactPerson PopulateNewContactPerson(MerchantPool merchantPool, string appUserId)
    {
        var contactPerson = new ContactPerson
        {
            ContactPersonType = ContactPersonType.AuthorizedPerson,
            Email = merchantPool.Email,
            CompanyEmail = merchantPool.CompanyEmail,
            IdentityNumber = merchantPool.AuthorizedPersonIdentityNumber,
            Name = merchantPool.AuthorizedPersonName,
            Surname = merchantPool.AuthorizedPersonSurname,
            BirthDate = merchantPool.AuthorizedPersonBirthDate,
            CompanyPhoneNumber = merchantPool.AuthorizedPersonCompanyPhoneNumber,
            MobilePhoneNumber = merchantPool.AuthorizedPersonMobilePhoneNumber,
            MobilePhoneNumberSecond = merchantPool.AuthorizedPersonMobilePhoneNumberSecond,
            CreateDate = DateTime.Now,
            CreatedBy = appUserId
        };
        return contactPerson;
    }

    public async Task<MerchantPoolDto> GetByIdAsync(GetMerchantPoolByIdQuery request)
    {
        var merchantPool = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.Id == request.Id);

        if (merchantPool is null)
        {
            throw new NotFoundException(nameof(MerchantPool), request.Id);
        }
        
        var poolDto = _mapper.Map<MerchantPoolDto>(merchantPool);
        if (poolDto.BankCode is not null)
        {
            var bank = await _bankRepository.GetAll()
                .FirstOrDefaultAsync(b => b.Code == merchantPool.BankCode);
            poolDto.Bank = _mapper.Map<BankDto>(bank);
        }

        return poolDto;
    }

    public async Task<PaginatedList<MerchantPoolDto>> GetFilterListAsync(GetFilterMerchantPoolQuery request)
    {
        var merchantPoolList = _repository.GetAll();

        if (!string.IsNullOrEmpty(request.Q))
        {
            merchantPoolList = merchantPoolList.Where(b => b.MerchantName.ToLower().Contains(request.Q.ToLower()));
        }

        if (request.CompanyType is not null)
        {
            merchantPoolList = merchantPoolList
                .Where(b => b.CompanyType == request.CompanyType);
        }

        if (request.CreateDateStart is not null)
        {
            merchantPoolList = merchantPoolList.Where(b => b.CreateDate
                                                           >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            merchantPoolList = merchantPoolList.Where(b => b.CreateDate
                                                           <= request.CreateDateEnd);
        }

        if (request.MerchantPoolStatus is not null)
        {
            merchantPoolList = merchantPoolList
                .Where(b => b.MerchantPoolStatus == request.MerchantPoolStatus);
        }

        if (request.MerchantType is not null)
        {
            merchantPoolList = merchantPoolList
                .Where(b => b.MerchantType == request.MerchantType);
        }

        if (request.PosType is not null)
        {
            merchantPoolList = merchantPoolList
                .Where(b => b.PosType == request.PosType);
        }

        if (request.MoneyTransferStartHourStart is not null)
        {
            merchantPoolList = merchantPoolList
                .Where(b => b.MoneyTransferStartHour >= request.MoneyTransferStartHourStart);
        }

        if (request.MoneyTransferStartHourFinish is not null)
        {
            merchantPoolList = merchantPoolList
                .Where(b => b.MoneyTransferStartHour <= request.MoneyTransferStartHourFinish);
        }

        if (request.MoneyTransferStartMinuteStart is not null)
        {
            merchantPoolList = merchantPoolList
                .Where(b => b.MoneyTransferStartMinute >= request.MoneyTransferStartMinuteStart);
        }

        if (request.MoneyTransferStartMinuteFinish is not null) 
        {
            merchantPoolList = merchantPoolList
                .Where(b => b.MoneyTransferStartMinute <= request.MoneyTransferStartMinuteFinish);
        }

        return await merchantPoolList
            .PaginatedListWithMappingAsync<MerchantPool,MerchantPoolDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task SaveAsync(SaveMerchantPoolCommand request)
    {
        if(request.CompanyType == CompanyType.Individual)
        {
            request.TaxNumber = request.AuthorizedPersonIdentityNumber;
        }

        if (await IsExistAsync(request))
            throw new MerchantCommercialTitleNotMatchException();

        if (request.BankCode is not null)
        {
            var bank = await _bankRepository.GetAll()
                .FirstOrDefaultAsync(b => b.Code == request.BankCode);

            if (bank is null)
            {
                throw new NotFoundException(nameof(Bank), request.BankCode);
            }
        }
        
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        var merchantPool = _mapper.Map<MerchantPool>(request);
        merchantPool.Channel = _contextProvider.CurrentContext.Channel;
        merchantPool.CurrencyCode = "TRY";
        merchantPool.MerchantPoolStatus = MerchantPoolStatus.Waiting;
        merchantPool.CreatedBy = _applicationUserService.ApplicationUserId.ToString();
        merchantPool.WalletNumber = merchantPool.WalletNumber?.Trim().Replace(" ", string.Empty);
        merchantPool.IsPaymentToMainMerchant = request.IsPaymentToMainMerchant;

        await CheckValidationsAsync(merchantPool);

        await _repository.AddAsync(merchantPool);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "SaveMerchantPool",
                SourceApplication = "PF",
                Resource = "MerchantPool",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                    {"MerchantName", request.MerchantName},
                    {"Email", request.Email},
                    {"BankCode", request.BankCode.ToString()},
                    {"Iban", request.Iban},
                    {"PostingPaymentChannel", request.PostingPaymentChannel.ToString()},
                    {"WalletNumber", request.WalletNumber},
                }
            });

    }

    private async Task<bool> IsExistAsync(SaveMerchantPoolCommand request)
    {
        return await _repository.GetAll().AnyAsync(x =>
            x.TaxNumber == request.TaxNumber &&
            x.CommercialTitle != request.CommercialTitle &&
            x.RecordStatus == RecordStatus.Active &&
            x.MerchantPoolStatus != MerchantPoolStatus.Rejected
        );
    }

    public async Task<string> GenerateMerchantNumberAsync()
    {
            var merchantCounter = new MerchantCounter();
            merchantCounter.CreateDate = DateTime.Now;
            merchantCounter.CreatedBy = _applicationUserService.ApplicationUserId.ToString();

            await _counterRepository.AddAsync(merchantCounter);

            var merchantNumber = $"{Prefix}{(merchantCounter.NumberCounter).ToString().PadLeft(8, '0')}";
            
            return merchantNumber;
    }
    private async Task CheckValidationsAsync(MerchantPool merchant)
    {
        var isKkbEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "KkbEnabled");

        var isKpsEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "PFKpsEnabled");

        if (isKkbEnabled && !string.IsNullOrEmpty(merchant.Iban))
        {
            var isValid = await ValidateIbanAsync(merchant);

            if (!isValid)
            {
                _logger.LogError($"SaveMerchantPooCommandError: IBAN is not valid : {merchant.Iban}");
                throw new IbanValidationFailedException();
            }
        }

        if (isKpsEnabled)
        {
            var isValid = await ValidateIdentityAsync(merchant);
            if (!isValid)
            {
                _logger.LogError($"SaveMerchantPooCommandError: Identity is not valid");
                throw new IdentityValidationFailedException();
            }
        }
    }
    private async Task<bool> ValidateIbanAsync(MerchantPool merchant)
    {
        var validateIban = new ValidateIbanRequest()
        {
            Iban = merchant.Iban,
            IdentityNo = merchant.CompanyType == CompanyType.Individual
        ? merchant.AuthorizedPersonIdentityNumber
        : merchant.TaxNumber,
        };
        var res = await _kkbService.ValidateIban(validateIban);
        return res.IsValid;
    }
    private async Task<bool> ValidateIdentityAsync(MerchantPool merchant)
    {
        var validateIdentity = new ValidateIdentityRequest()
        {
            IdentityNo = merchant.AuthorizedPersonIdentityNumber,
            Name = merchant.AuthorizedPersonName,
            Surname = merchant.AuthorizedPersonSurname,
            BirthDate = merchant.AuthorizedPersonBirthDate
        };
        var res = await _kpsService.ValidateIdentity(validateIdentity);
        return res.IsValid;
    }
}