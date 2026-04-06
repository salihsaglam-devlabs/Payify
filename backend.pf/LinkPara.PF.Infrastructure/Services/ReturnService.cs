using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Calendar;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.MoneyTransfer.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Commons.Models.Payments;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Commons.Models.Returns;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Application.Features.MerchantLimits.Queries.GetFilterMerchantLimits;
using LinkPara.PF.Application.Features.MerchantReturnPools;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.Payments.Commands.ManualReturn;
using LinkPara.PF.Application.Features.Payments.Commands.Return;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.PF.Infrastructure.Services.VposServices;
using LinkPara.SharedModels.BusModels.Commands.PF;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Channels;
using System.Transactions;
using TransactionStatus = LinkPara.PF.Domain.Enums.TransactionStatus;

namespace LinkPara.PF.Infrastructure.Services;

public class ReturnService : IReturnService
{
    private const string GenericErrorCode = "99";
    private const string GenericSuccessCode = "00";

    private readonly ILogger<ReverseService> _logger;
    private readonly ICurrencyService _currencyService;
    private readonly IMerchantService _merchantService;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IGenericRepository<MerchantVpos> _merchantVposRepository;
    private readonly IGenericRepository<BankTransaction> _bankTransactionRepository;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<MerchantInstallmentTransaction> _merchantInstallmentTransactionRepository;
    private readonly IGenericRepository<MerchantBlockage> _merchantBlockageRepository;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IResponseCodeService _errorCodeService;
    private readonly IGenericRepository<MerchantApiValidationLog> _apiRequestValidationLogRepository;
    private readonly IGenericRepository<TimeoutTransaction> _timeoutTransactionRepository;
    private readonly IOrderNumberGeneratorService _orderNumberGeneratorService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ILimitService _limitService;
    private readonly IMerchantReturnPoolService _merchantReturnPoolService;
    private readonly VposServiceFactory _vposServiceFactory;
    private readonly IMerchantLimitService _merchantLimitService;
    private readonly IStringLocalizer _exceptionLocalizer;
    private readonly IParameterService _parameterService;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<MerchantUser> _merchantUserRepository;
    private readonly ICalendarService _calendarService;
    private readonly IGenericRepository<PostingTransaction> _postingTransactionRepository;
    private readonly IGenericRepository<PricingProfileItem> _pricingProfileItemRepository;
    private readonly IUserService _userService;
    private readonly IBus _bus;
    private readonly IFraudService _fraudService;
    private readonly IOnUsPaymentService _onUsPaymentService;
    private readonly IGenericRepository<SubMerchant> _subMerchantRepository;
    private readonly IGenericRepository<SubMerchantLimit> _subMerchantLimitRepository;
    private readonly IGenericRepository<SubMerchantUser> _subMerchantUserRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IMapper _mapper;
    private readonly IBasePaymentService _basePaymentService;
    private readonly IVaultClient _vaultClient;
    private readonly IPfReturnTransactionService _pfReturnTransactionService;

    public ReturnService(
        ILogger<ReverseService> logger,
        ICurrencyService currencyService,
        IMerchantService merchantService,
        IGenericRepository<Vpos> vposRepository,
        IGenericRepository<BankTransaction> bankTransactionRepository,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IServiceScopeFactory scopeFactory,
        IResponseCodeService errorCodeService,
        IGenericRepository<MerchantApiValidationLog> apiRequestValidationLogRepository,
        IGenericRepository<TimeoutTransaction> timeoutTransactionRepository,
        IOrderNumberGeneratorService orderNumberGeneratorService,
        IApplicationUserService applicationUserService,
        ILimitService limitService,
        IMerchantReturnPoolService merchantReturnPoolService,
        VposServiceFactory vposServiceFactory,
        IMerchantLimitService merchantLimitService,
        IStringLocalizerFactory factory,
        IParameterService parameterService,
        IContextProvider contextProvider,
        IGenericRepository<MerchantUser> merchantUserRepository,
        ICalendarService calendarService,
        IGenericRepository<MerchantBlockage> merchantBlockageRepository,
        IGenericRepository<PostingTransaction> postingTransactionRepository,
        IGenericRepository<PricingProfileItem> pricingProfileItemRepository,
        IUserService userService,
        IBus bus,
        IFraudService fraudService,
        IOnUsPaymentService onUsPaymentService,
        IGenericRepository<SubMerchant> subMerchantRepository,
        IGenericRepository<SubMerchantLimit> subMerchantLimitRepository,
        IMapper mapper,
        IGenericRepository<SubMerchantUser> subMerchantUserRepository,
        IGenericRepository<Merchant> merchantRepository,
        IBasePaymentService basePaymentService,
        IGenericRepository<MerchantVpos> merchantVposRepository,
        IVaultClient vaultClient,
        IPfReturnTransactionService pfReturnTransactionService,
        IGenericRepository<MerchantInstallmentTransaction> merchantInstallmentTransactionRepository)
    {
        _logger = logger;
        _currencyService = currencyService;
        _vposRepository = vposRepository;
        _merchantService = merchantService;
        _bankTransactionRepository = bankTransactionRepository;
        _merchantTransactionRepository = merchantTransactionRepository;
        _scopeFactory = scopeFactory;
        _errorCodeService = errorCodeService;
        _apiRequestValidationLogRepository = apiRequestValidationLogRepository;
        _timeoutTransactionRepository = timeoutTransactionRepository;
        _orderNumberGeneratorService = orderNumberGeneratorService;
        _applicationUserService = applicationUserService;
        _limitService = limitService;
        _merchantReturnPoolService = merchantReturnPoolService;
        _vposServiceFactory = vposServiceFactory;
        _merchantLimitService = merchantLimitService;
        _parameterService = parameterService;
        _contextProvider = contextProvider;
        _exceptionLocalizer = factory.Create("Exceptions", "LinkPara.PF.API");
        _merchantUserRepository = merchantUserRepository;
        _calendarService = calendarService;
        _merchantBlockageRepository = merchantBlockageRepository;
        _postingTransactionRepository = postingTransactionRepository;
        _pricingProfileItemRepository = pricingProfileItemRepository;
        _userService = userService;
        _bus = bus;
        _fraudService = fraudService;
        _onUsPaymentService = onUsPaymentService;
        _subMerchantRepository = subMerchantRepository;
        _subMerchantLimitRepository = subMerchantLimitRepository;
        _mapper = mapper;
        _subMerchantUserRepository = subMerchantUserRepository;
        _merchantRepository = merchantRepository;
        _basePaymentService = basePaymentService;
        _merchantVposRepository = merchantVposRepository;
        _vaultClient = vaultClient;
        _pfReturnTransactionService = pfReturnTransactionService;
        _merchantInstallmentTransactionRepository = merchantInstallmentTransactionRepository;
    }

    public async Task<ReturnResponse> ReturnAsync(ReturnCommand request)
    {
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? userId : _applicationUserService.ApplicationUserId.ToString();

        try
        {
            var merchant = await _merchantService.GetByIdAsync(request.MerchantId);

            if (merchant is null)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, request.LanguageCode,
                    request.ConversationId);
            }

            var parentMerchantFinancialTransaction = true;
            if (merchant.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
            {
                var parentMerchant = await _merchantRepository.GetByIdAsync(merchant.ParentMerchantId);
                if (parentMerchant is not null)
                {
                    parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
                }
            }

            if (!merchant.PaymentReturnAllowed) //submerchant
            {
                return await GetValidationResponseAsync(ApiErrorCode.NoReturnPaymentAllowed, request.LanguageCode,
                    request.ConversationId);
            }

            if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed ||
                !parentMerchantFinancialTransaction)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, request.LanguageCode,
                    request.ConversationId);
            }

            var referenceBankTransaction = await _bankTransactionRepository.GetAll()
                .FirstOrDefaultAsync(s =>
                    s.RecordStatus == RecordStatus.Active &&
                    s.OrderId == request.OrderId);

            if (referenceBankTransaction is null)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidReferenceNumber, request.LanguageCode,
                    request.ConversationId);
            }

            SetAmount(request, referenceBankTransaction);

            var returnSettings = _vaultClient.GetSecretValue<ReturnSettingsModel>("PFSecrets", "ReturnSettings", null);

            if (returnSettings.IsManuelReturnAllowed)
            {
                var errorMessage =
                    await _errorCodeService.GetApiResponseCode(ApiErrorCode.ManuelReturnAllowed, request.LanguageCode);
                var referenceNumber = (merchant.Number + referenceBankTransaction.OrderId).PadLeft(20, '0');
                var message = errorMessage.DisplayMessage
                    .Replace("{Iban}", returnSettings.ReturnBankAccountIban)
                    .Replace("{Amount}", request.Amount.ToString())
                    .Replace("{Description}", referenceNumber);

                var incomingReturnTransactionsList = await _pfReturnTransactionService.GetListAsync(new()
                {
                    Description = referenceNumber,
                    MinAmount = request.Amount,
                    MaxAmount = request.Amount,
                    TransactionStatus = PfReturnTransactionStatus.Pending
                });

                if (incomingReturnTransactionsList != null &&
                    incomingReturnTransactionsList.TotalCount == 0)
                {
                    return new ReturnResponse
                    {
                        IsSucceed = false,
                        ErrorCode = errorMessage.ResponseCode,
                        ErrorMessage = message,
                        ApprovalStatus = ReturnApprovalStatus.None,
                        ReturnMessage = referenceNumber,
                        ResponseMessage = returnSettings.ReturnBankAccountIban,
                        ReturnAmount = request.Amount
                    };
                }
            }

            var limitReturnAmount = await CheckReturnAmountAsync(merchant, request.Amount);

            if (limitReturnAmount < request.Amount)
            {
                if (returnSettings.IsManuelExcessReturnAllowed)
                {
                    var errorMessage =
                        await _errorCodeService.GetApiResponseCode(ApiErrorCode.ManuelReturnAllowed,
                            request.LanguageCode);
                    var referenceNumber = (merchant.Number + referenceBankTransaction.OrderId).PadLeft(20, '0');
                    var message = errorMessage.DisplayMessage
                        .Replace("{Iban}", returnSettings.ReturnBankAccountIban)
                        .Replace("{Amount}", request.Amount.ToString())
                        .Replace("{Description}", referenceNumber);

                    var incomingReturnTransactionsList = await _pfReturnTransactionService.GetListAsync(new()
                    {
                        Description = referenceNumber,
                        MinAmount = request.Amount,
                        MaxAmount = request.Amount,
                        TransactionStatus = PfReturnTransactionStatus.Pending
                    });

                    if (incomingReturnTransactionsList != null &&
                        incomingReturnTransactionsList.TotalCount == 0)
                    {
                        return new ReturnResponse
                        {
                            IsSucceed = false,
                            ErrorCode = errorMessage.ResponseCode,
                            ErrorMessage = message,
                            ApprovalStatus = ReturnApprovalStatus.None,
                            ReturnMessage = referenceNumber,
                            ResponseMessage = returnSettings.ReturnBankAccountIban,
                            ReturnAmount = request.Amount
                        };
                    }
                }

                if (!merchant.IsExcessReturnAllowed)
                {
                    return await GetValidationResponseAsync(ApiErrorCode.DailyReturnAmountCannotGreaterAuthAmount,
                        request.LanguageCode, request.ConversationId);
                }
            }

            var merchantBlockage = await _merchantBlockageRepository
                .GetAll()
                .FirstOrDefaultAsync(s => s.MerchantId == request.MerchantId
                                          && s.RecordStatus == RecordStatus.Active);

            if (merchantBlockage != null)
            {
                var isBlockedTransaction = await _postingTransactionRepository.GetAll()
                    .AnyAsync(s => s.MerchantId == merchant.Id
                                   && s.OrderId == request.OrderId
                                   && s.BlockageStatus == BlockageStatus.Blocked);

                if (isBlockedTransaction)
                {
                    return await GetValidationResponseAsync(ApiErrorCode.CannotReturnMerchantHasBlockedAmount,
                        request.LanguageCode, request.ConversationId);
                }
            }

            var referenceMerchantTransaction = await _merchantTransactionRepository.GetAll()
                .Include(s => s.AcquireBank)
                .FirstOrDefaultAsync(s =>
                    s.RecordStatus == RecordStatus.Active &&
                    s.PfTransactionSource == PfTransactionSource.VirtualPos &&
                    (s.TransactionStatus != TransactionStatus.Fail &&
                     s.TransactionStatus != TransactionStatus.Returned) &&
                    s.Id == referenceBankTransaction.MerchantTransactionId &&
                    s.MerchantId == merchant.Id &&
                    !s.IsReverse);

            if (referenceMerchantTransaction is null)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidReferenceNumber, request.LanguageCode,
                    request.ConversationId);
            }

            if (referenceMerchantTransaction.IsChargeback || referenceMerchantTransaction.IsSuspecious)
            {
                return await GetValidationResponseAsync(ApiErrorCode.TransactionNotReturnable, request.LanguageCode,
                    request.ConversationId);
            }

            if (referenceMerchantTransaction.IsOnUsPayment &&
                referenceMerchantTransaction.TransactionDate == DateTime.Today)
            {
                return await GetValidationResponseAsync(ApiErrorCode.TransactionCannotBeRefundedToday,
                    request.LanguageCode, request.ConversationId);
            }

            var subMerchant = new SubMerchant();
            if (referenceMerchantTransaction.SubMerchantId.HasValue &&
                referenceMerchantTransaction.SubMerchantId != Guid.Empty)
            {
                subMerchant = await _subMerchantRepository.GetByIdAsync(referenceMerchantTransaction.SubMerchantId);

                if (subMerchant is null)
                {
                    return await GetValidationResponseAsync(ApiErrorCode.InvalidSubMerchant, request.LanguageCode,
                        request.ConversationId);
                }

                if (!subMerchant.PaymentReturnAllowed)
                {
                    return await GetValidationResponseAsync(ApiErrorCode.SubMerchantNoReturnPaymentAllowed,
                        request.LanguageCode, request.ConversationId);
                }

                if (subMerchant.RecordStatus != RecordStatus.Active)
                {
                    return await GetValidationResponseAsync(ApiErrorCode.InvalidSubMerchantStatus, request.LanguageCode,
                        request.ConversationId);
                }
            }

            var totalRefundAmount = await _merchantTransactionRepository.GetAll() //submerchant
                .Where(s =>
                    s.ReturnedTransactionId == referenceMerchantTransaction.Id.ToString() &&
                    s.MerchantId == merchant.Id &&
                    s.TransactionStatus == TransactionStatus.Success)
                .SumAsync(s => s.Amount);

            var totalReturnPoolAmount =
                (await _merchantReturnPoolService.GetMerchantReturnPoolByOrderIdAsync(referenceMerchantTransaction
                    .OrderId))
                .Where(s => s.ReturnStatus == ReturnStatus.Pending && s.Id != request.MerchantReturnPoolId)
                .Sum(p => p.Amount);

            var totalAmount = totalRefundAmount + request.Amount;

            if ((totalRefundAmount + totalReturnPoolAmount + request.Amount) > referenceBankTransaction.Amount)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidReturnAmount, request.LanguageCode,
                    request.ConversationId);
            }

            //submerchant
            if (referenceMerchantTransaction.SubMerchantId.HasValue &&
                referenceMerchantTransaction.SubMerchantId != Guid.Empty)
            {
                var totalRefundAmounts = await _merchantTransactionRepository.GetAll()
                    .Where(s =>
                        s.ReturnedTransactionId == referenceMerchantTransaction.Id.ToString() &&
                        s.MerchantId == merchant.Id &&
                        s.SubMerchantId == referenceMerchantTransaction.SubMerchantId &&
                        s.TransactionStatus == TransactionStatus.Success)
                    .SumAsync(s => s.Amount);

                var totalReturnPoolAmounts =
                    (await _merchantReturnPoolService.GetMerchantReturnPoolByOrderIdAsync(referenceMerchantTransaction
                        .OrderId))
                    .Where(s => s.ReturnStatus == ReturnStatus.Pending && s.Id != request.MerchantReturnPoolId)
                    .Sum(p => p.Amount);

                var totalAmounts = totalRefundAmounts + request.Amount;

                if ((totalRefundAmounts + totalReturnPoolAmounts + request.Amount) > referenceBankTransaction.Amount)
                {
                    return await GetValidationResponseAsync(ApiErrorCode.InvalidReturnAmount, request.LanguageCode,
                        request.ConversationId);
                }
            }

            var remainingReturnAmount = referenceBankTransaction.Amount -
                                        (totalRefundAmount + totalReturnPoolAmount + request.Amount);

            var vpos = await _vposRepository.GetAll()
                .Include(s => s.AcquireBank)
                .Include(s => s.VposBankApiInfos)
                .ThenInclude(s => s.Key)
                .FirstOrDefaultAsync(s => s.Id == referenceBankTransaction.VposId);

            if (vpos is null)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidReferenceNumber, request.LanguageCode,
                    request.ConversationId);
            }

            if (!merchant.IsReturnApproved || request.IsAdminApproved)
            {
                var merchantLimits =
                    (await _merchantLimitService.GetFilterListAsync(
                        new GetFilterMerchantLimitsQuery
                        {
                            MerchantId = merchant.Id
                        }))
                    .Items
                    .Where(s => s.TransactionLimitType == TransactionLimitType.Return)
                    .ToList();

                if (merchantLimits.Any())
                {
                    var returnLimitResult = await CheckMerchantReturnPoolLimitsAsync(merchant, merchantLimits,
                        request.Amount, request.ConversationId);

                    if (!returnLimitResult.IsSucceed)
                    {
                        return returnLimitResult;
                    }
                }

                //submerchant
                if (referenceMerchantTransaction.SubMerchantId.HasValue &&
                    referenceMerchantTransaction.SubMerchantId != Guid.Empty)
                {
                    var returnLimitResult = await CheckSubMerchantReturnPoolLimitsAsync(
                        referenceMerchantTransaction.SubMerchantId.Value, request.Amount, request.ConversationId);

                    if (!returnLimitResult.IsSucceed)
                    {
                        return returnLimitResult;
                    }
                }

                if (!await _fraudService.CheckFraudAsync(new FraudTransactionDetail
                {
                    Amount = referenceBankTransaction.Amount,
                    BeneficiaryNumber = merchant.Number,
                    Beneficiary = merchant.Name,
                    BeneficiaryBankID = referenceBankTransaction.AcquireBankCode.ToString(),
                    OriginatorNumber = referenceBankTransaction.CardNumber,
                    Originator = referenceMerchantTransaction.CardHolderName,
                    OriginatorBankID = referenceBankTransaction.IssuerBankCode.ToString() ?? string.Empty,
                    FraudSource = FraudSource.Pos,
                    Direction = Direction.Inbound,
                    AmountCurrencyCode = referenceBankTransaction.Currency,
                    BeneficiaryAccountCurrencyCode = referenceBankTransaction.Currency,
                    OriginatorAccountCurrencyCode = referenceBankTransaction.Currency,
                    Channel = _contextProvider.CurrentContext.Channel,
                    TransactionType = TransactionType.Return.ToString(),
                    MccCode = Convert.ToInt32(merchant.MccCode)
                }, "PfReturn", request.ClientIpAddress))
                {
                    return await GetValidationResponseAsync(ApiErrorCode.PotentialFraud, request.LanguageCode,
                        request.ConversationId);
                }

                return await ReturnAsync(request, referenceMerchantTransaction, referenceBankTransaction, vpos,
                    merchant, totalAmount, parseUserId, remainingReturnAmount, subMerchant);
            }
            else
            {
                referenceMerchantTransaction.ReturnStatus = ReturnStatus.Pending;
                referenceMerchantTransaction.LastModifiedBy = parseUserId;
                await _merchantTransactionRepository.UpdateAsync(referenceMerchantTransaction);
                return await SaveMerchantReturnPool(request, referenceMerchantTransaction);
            }
        }
        catch (PreValidationException exception)
        {
            var code = exception.Code;
            var message = exception.Message;

            _logger.LogError($"Return PreValidation failed with code : {code}, " +
                             $"Message: {code}");

            await InsertValidationLogAsync(request, new ValidationResponse
            {
                Code = code,
                Message = message,
                IsValid = false
            });

            return new ReturnResponse
            {
                IsSucceed = false,
                ErrorCode = code,
                ConversationId = request.ConversationId,
                ErrorMessage = message,
                ApprovalStatus = ReturnApprovalStatus.None,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Return PreValidation failed with code : {GenericErrorCode}, " +
                             $"Message: {exception.Message}");

            return new ReturnResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = request.ConversationId,
                ApprovalStatus = ReturnApprovalStatus.None,
            };
        }
    }

    private async Task<ReturnResponse> CheckSubMerchantReturnPoolLimitsAsync(Guid subMerchantId, decimal amount,
        string conversationId)
    {
        var subMerchantLimits = _subMerchantLimitRepository.GetAll().Where(b =>
            b.SubMerchantId == subMerchantId && b.TransactionLimitType == TransactionLimitType.Return).ToList();

        if (subMerchantLimits.Any())
        {
            var mapList = _mapper.Map<List<SubMerchantLimitDto>>(subMerchantLimits);

            var monthlyReturnTransactions = await GetMerchantTransactionsAsync(subMerchantId);

            var dailyReturnLimitResult = CheckSubMerchantDailyReturnLimit(mapList, monthlyReturnTransactions, amount);

            if (dailyReturnLimitResult.IsSuccess == false)
            {
                return PopulateReturnResponse(dailyReturnLimitResult, Period.Daily, conversationId);
            }

            var monthlyReturnLimitResult =
                CheckSubMerchantMonthlyLimitExceeded(mapList, monthlyReturnTransactions, amount);

            if (monthlyReturnLimitResult.IsSuccess == false)
            {
                return PopulateReturnResponse(monthlyReturnLimitResult, Period.Monthly, conversationId);
            }
        }

        return new ReturnResponse()
        {
            IsSucceed = true
        };
    }

    private async Task<List<MerchantTransactionDto>> GetMerchantTransactionsAsync(Guid subMerchantId)
    {
        var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var merchantTransactions = await _merchantTransactionRepository.GetAll()
            .Where(s => s.SubMerchantId == subMerchantId
                        && s.RecordStatus == RecordStatus.Active
                        && s.CreateDate >= firstDayOfMonth
                        && s.CreateDate <= DateTime.Now
                        && s.TransactionType == TransactionType.Return)
            .ToListAsync();

        return _mapper.Map<List<MerchantTransactionDto>>(merchantTransactions);
    }

    private async Task<ReturnResponse> CheckMerchantReturnPoolLimitsAsync(MerchantDto merchant,
        List<MerchantLimitDto> merchantLimits, decimal amount, string conversationId)
    {
        var monthlyReturnTransactions = await _merchantReturnPoolService
            .GetMerchantMonthlyReturnTransactionsAsync(merchant.Id);

        var dailyReturnLimitResult = CheckDailyReturnLimit(merchantLimits, monthlyReturnTransactions, amount);

        if (dailyReturnLimitResult.IsSuccess == false)
        {
            return PopulateReturnResponse(dailyReturnLimitResult, Period.Daily, conversationId);
        }

        var monthlyReturnLimitResult = CheckMonthlyLimitExceeded(merchantLimits, monthlyReturnTransactions, amount);

        if (monthlyReturnLimitResult.IsSuccess == false)
        {
            return PopulateReturnResponse(monthlyReturnLimitResult, Period.Monthly, conversationId);
        }

        return new ReturnResponse()
        {
            IsSucceed = true
        };
    }

    private ReturnResponse PopulateReturnResponse(ReturnLimitResult returnLimitResult, Period period,
        string conversationId)
    {
        string errorMessage;
        string errorCode;

        if (period == Period.Daily)
        {
            if (returnLimitResult.LimitType == LimitType.Amount)
            {
                errorCode = ApiErrorCode.DailyLimitAmountExceeded;
                errorMessage = _exceptionLocalizer.GetString("DailyLimitAmountExceeded");
            }
            else
            {
                errorCode = ApiErrorCode.DailyLimitCountExceeded;
                errorMessage = _exceptionLocalizer.GetString("DailyLimitCountExceeded");
            }
        }
        else
        {
            if (returnLimitResult.LimitType == LimitType.Amount)
            {
                errorCode = ApiErrorCode.MonthlyLimitAmountExceeded;
                errorMessage = _exceptionLocalizer.GetString("MonthlyLimitAmountExceeded");
            }
            else
            {
                errorCode = ApiErrorCode.MonthlyLimitCountExceeded;
                errorMessage = _exceptionLocalizer.GetString("MonthlyLimitCountExceeded");
            }
        }

        return new ReturnResponse
        {
            IsSucceed = string.IsNullOrEmpty(errorCode),
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            ConversationId = conversationId,
            ApprovalStatus = ReturnApprovalStatus.None
        };
    }

    private static ReturnLimitResult CheckMonthlyLimitExceeded(IEnumerable<MerchantLimitDto> merchantLimits,
        List<MerchantTransactionDto> monthlyReturnTransactions, decimal amount)
    {
        var monthlyLimits = merchantLimits
            .Where(s => s.Period == Period.Monthly)
            .ToList();

        var returnLimitResult = new ReturnLimitResult()
        {
            IsSuccess = true,
            Period = Period.Monthly
        };

        var successfulTransactions = monthlyReturnTransactions
            .Where(s => s.TransactionStatus == TransactionStatus.Success)
            .ToList();

        if (monthlyLimits.Any())
        {
            foreach (var monthlyLimit in monthlyLimits)
            {
                if (monthlyLimit.LimitType == LimitType.Count
                    && successfulTransactions.Count >= monthlyLimit.MaxPiece)
                {
                    returnLimitResult.IsSuccess = false;
                    returnLimitResult.LimitType = LimitType.Count;
                    break;
                }

                if (monthlyLimit.LimitType == LimitType.Amount
                    && successfulTransactions.Sum(s => s.Amount)
                    + amount > monthlyLimit.MaxAmount)
                {
                    returnLimitResult.IsSuccess = false;
                    returnLimitResult.LimitType = LimitType.Amount;
                    break;
                }
            }
        }

        return returnLimitResult;
    }

    private static ReturnLimitResult CheckSubMerchantMonthlyLimitExceeded(
        IEnumerable<SubMerchantLimitDto> subMerchantLimits,
        List<MerchantTransactionDto> monthlyReturnTransactions, decimal amount)
    {
        var monthlyLimits = subMerchantLimits
            .Where(s => s.Period == Period.Monthly)
            .ToList();

        var returnLimitResult = new ReturnLimitResult()
        {
            IsSuccess = true,
            Period = Period.Monthly
        };

        var successfulTransactions = monthlyReturnTransactions
            .Where(s => s.TransactionStatus == TransactionStatus.Success)
            .ToList();

        if (monthlyLimits.Any())
        {
            foreach (var monthlyLimit in monthlyLimits)
            {
                if (monthlyLimit.LimitType == LimitType.Count
                    && successfulTransactions.Count >= monthlyLimit.MaxPiece)
                {
                    returnLimitResult.IsSuccess = false;
                    returnLimitResult.LimitType = LimitType.Count;
                    break;
                }

                if (monthlyLimit.LimitType == LimitType.Amount
                    && successfulTransactions.Sum(s => s.Amount)
                    + amount > monthlyLimit.MaxAmount)
                {
                    returnLimitResult.IsSuccess = false;
                    returnLimitResult.LimitType = LimitType.Amount;
                    break;
                }
            }
        }

        return returnLimitResult;
    }

    private static ReturnLimitResult CheckSubMerchantDailyReturnLimit(
        IEnumerable<SubMerchantLimitDto> subMerchantLimits,
        List<MerchantTransactionDto> monthlyReturnTransactions, decimal amount)
    {
        var returnLimitResult = new ReturnLimitResult()
        {
            IsSuccess = true,
            Period = Period.Daily
        };

        var dailyLimits = subMerchantLimits
            .Where(s => s.Period == Period.Daily)
            .ToList();

        var dailyMerchantSuccessTransactionList = monthlyReturnTransactions
            .Where(s =>
                s.TransactionStatus == TransactionStatus.Success
                && s.TransactionDate >= DateTime.Today)
            .ToList();


        if (dailyLimits.Any())
        {
            foreach (var dailyLimit in dailyLimits)
            {
                if (dailyLimit.LimitType == LimitType.Count &&
                    dailyMerchantSuccessTransactionList.Count >= dailyLimit.MaxPiece)
                {
                    returnLimitResult.IsSuccess = false;
                    returnLimitResult.LimitType = LimitType.Count;
                    break;
                }

                if (dailyLimit.LimitType == LimitType.Amount
                    && dailyMerchantSuccessTransactionList.Sum(s => s.Amount) + amount > dailyLimit.MaxAmount)
                {
                    returnLimitResult.IsSuccess = false;
                    returnLimitResult.LimitType = LimitType.Amount;
                    break;
                }
            }
        }

        return returnLimitResult;
    }

    private static ReturnLimitResult CheckDailyReturnLimit(IEnumerable<MerchantLimitDto> merchantLimits,
        List<MerchantTransactionDto> monthlyReturnTransactions, decimal amount)
    {
        var returnLimitResult = new ReturnLimitResult()
        {
            IsSuccess = true,
            Period = Period.Daily
        };

        var dailyLimits = merchantLimits
            .Where(s => s.Period == Period.Daily)
            .ToList();

        var dailyMerchantSuccessTransactionList = monthlyReturnTransactions
            .Where(s =>
                s.TransactionStatus == TransactionStatus.Success
                && s.TransactionDate >= DateTime.Today)
            .ToList();


        if (dailyLimits.Any())
        {
            foreach (var dailyLimit in dailyLimits)
            {
                if (dailyLimit.LimitType == LimitType.Count &&
                    dailyMerchantSuccessTransactionList.Count >= dailyLimit.MaxPiece)
                {
                    returnLimitResult.IsSuccess = false;
                    returnLimitResult.LimitType = LimitType.Count;
                    break;
                }

                if (dailyLimit.LimitType == LimitType.Amount
                    && dailyMerchantSuccessTransactionList.Sum(s => s.Amount) + amount > dailyLimit.MaxAmount)
                {
                    returnLimitResult.IsSuccess = false;
                    returnLimitResult.LimitType = LimitType.Amount;
                    break;
                }
            }
        }

        return returnLimitResult;
    }

    private async Task<ReturnResponse> SaveMerchantReturnPool(ReturnCommand request,
        MerchantTransaction referenceMerchantTransaction)
    {
        Currency currency = await _currencyService.GetByNumberAsync(referenceMerchantTransaction.Currency);
        await _merchantReturnPoolService.AddAsync(new MerchantReturnPoolDto
        {
            MerchantId = request.MerchantId,
            ClientIpAddress = request.ClientIpAddress,
            LanguageCode = request.LanguageCode,
            Amount = request.Amount,
            ConversationId = request.ConversationId,
            OrderId = request.OrderId,
            BankCode = referenceMerchantTransaction.AcquireBank.Code,
            BankName = referenceMerchantTransaction.AcquireBank.Name,
            BankStatus = null,
            CardNumber = referenceMerchantTransaction.CardNumber,
            CurrencyCode = currency.Code,
            IsTopUpPayment = request.IsTopUpPayment
        });

        return new ReturnResponse
        {
            IsSucceed = true,
            ConversationId = request.ConversationId,
            ReturnMessage = request.LanguageCode.ToUpper() != "TR"
                ? "Your return request need to approve. Our team will proceed about the return request."
                : "İade talebinizin onaylanması gerekiyor. Ekibimiz iade talebiyle ilgili işlem yapacaktır.",
            ErrorCode = string.Empty,
            ErrorMessage = string.Empty,
            ApprovalStatus = ReturnApprovalStatus.PendingApproval
        };
    }

    private async Task<ReturnResponse> ReturnAsync(ReturnCommand request,
        MerchantTransaction referenceMerchantTransaction,
        BankTransaction referenceBankTransaction, Vpos vpos, MerchantDto merchant, decimal totalRefundAmount,
        string parseUserId, decimal remainingReturnAmount, SubMerchant subMerchant)
    {
        var merchantTransaction = await SaveMerchantTransactionAsync(request, subMerchant, parseUserId);
        var bankTransaction = PopulateInitialBankTransaction(request, merchantTransaction);
        PosRefundResponse posResponse = null;

        try
        {
            var originalAuthProcess = await _merchantTransactionRepository.GetAll()
                .Where(s => s.ConversationId == referenceMerchantTransaction.ConversationId)
                .OrderBy(s => s.TransactionStartDate)
                .FirstOrDefaultAsync();

            var orgOrderId = referenceBankTransaction.OrderId;

            bankTransaction.OrderId =
                await _orderNumberGeneratorService.GenerateForBankTransactionAsync(
                    referenceBankTransaction.AcquireBankCode, merchant.Number);
            bankTransaction.Currency = referenceBankTransaction.Currency;
            bankTransaction.CardNumber = referenceBankTransaction.CardNumber;
            bankTransaction.VposId = referenceBankTransaction.VposId;
            bankTransaction.MerchantCode = referenceBankTransaction.MerchantCode;
            bankTransaction.SubMerchantCode = referenceBankTransaction.SubMerchantCode;
            bankTransaction.IssuerBankCode = referenceBankTransaction.IssuerBankCode;
            bankTransaction.AcquireBankCode = referenceBankTransaction.AcquireBankCode;

            merchantTransaction.Currency = referenceMerchantTransaction.Currency;
            merchantTransaction.InstallmentCount = referenceMerchantTransaction.InstallmentCount;
            merchantTransaction.ThreeDSessionId = referenceMerchantTransaction.ThreeDSessionId;
            merchantTransaction.Is3ds = referenceMerchantTransaction.Is3ds;
            merchantTransaction.CardNumber = referenceMerchantTransaction.CardNumber;
            merchantTransaction.BinNumber = referenceMerchantTransaction.BinNumber;
            merchantTransaction.HasCvv = referenceMerchantTransaction.HasCvv;
            merchantTransaction.HasExpiryDate = referenceMerchantTransaction.HasExpiryDate;
            merchantTransaction.IsAmex = referenceMerchantTransaction.IsAmex;
            merchantTransaction.IsInternational = referenceMerchantTransaction.IsInternational;
            merchantTransaction.VposId = referenceMerchantTransaction.VposId;
            merchantTransaction.VposName = referenceMerchantTransaction.VposName;
            merchantTransaction.IssuerBankCode = referenceMerchantTransaction.IssuerBankCode;
            merchantTransaction.AcquireBankCode = referenceMerchantTransaction.AcquireBankCode;
            merchantTransaction.BankCommissionRate = referenceMerchantTransaction.BankCommissionRate;
            merchantTransaction.CardTransactionType = referenceMerchantTransaction.CardTransactionType;
            merchantTransaction.ReturnedTransactionId = referenceMerchantTransaction.Id.ToString();
            merchantTransaction.CardType = referenceMerchantTransaction.CardType;
            merchantTransaction.LanguageCode = referenceMerchantTransaction.LanguageCode;
            merchantTransaction.OrderId = bankTransaction.OrderId;
            merchantTransaction.PointCommissionRate = referenceMerchantTransaction.PointCommissionRate;
            merchantTransaction.ServiceCommissionRate = referenceMerchantTransaction.ServiceCommissionRate;
            merchantTransaction.IsOnUsPayment = referenceMerchantTransaction.IsOnUsPayment;
            merchantTransaction.IsInsurancePayment = referenceMerchantTransaction.IsInsurancePayment;
            merchantTransaction.SubMerchantId = referenceMerchantTransaction.SubMerchantId;

            var pricingProfileItem = await _pricingProfileItemRepository.GetAll()
                    .Include(p => p.PricingProfile)
                    .FirstOrDefaultAsync(p => p.Id == referenceMerchantTransaction.PricingProfileItemId);

            if (request.Amount == referenceMerchantTransaction.Amount)
            {
                merchantTransaction.BankCommissionAmount = referenceMerchantTransaction.BankCommissionAmount;
                merchantTransaction.PfCommissionAmount = referenceMerchantTransaction.PfCommissionAmount;
                merchantTransaction.PfNetCommissionAmount = referenceMerchantTransaction.PfNetCommissionAmount;
                merchantTransaction.PfCommissionRate = referenceMerchantTransaction.PfCommissionRate;
                merchantTransaction.PfPerTransactionFee = referenceMerchantTransaction.PfPerTransactionFee;
                merchantTransaction.AmountWithoutCommissions = referenceMerchantTransaction.AmountWithoutCommissions;
                merchantTransaction.AmountWithoutBankCommission =
                    referenceMerchantTransaction.AmountWithoutBankCommission;
                merchantTransaction.BsmvAmount = referenceMerchantTransaction.BsmvAmount;
                merchantTransaction.PointCommissionAmount = referenceMerchantTransaction.PointCommissionAmount;
                merchantTransaction.ServiceCommissionAmount = referenceMerchantTransaction.ServiceCommissionAmount;
                merchantTransaction.ParentMerchantCommissionAmount =
                    referenceMerchantTransaction.ParentMerchantCommissionAmount;
                merchantTransaction.ParentMerchantCommissionRate =
                    referenceMerchantTransaction.ParentMerchantCommissionRate;
                merchantTransaction.AmountWithoutParentMerchantCommission =
                    referenceMerchantTransaction.AmountWithoutParentMerchantCommission;
            }
            else
            {                

                merchantTransaction.PfPerTransactionFee = remainingReturnAmount == 0
                    ? pricingProfileItem.PricingProfile.PerTransactionFee
                    : 0;
                merchantTransaction.BankCommissionAmount =
                    (request.Amount * referenceMerchantTransaction.BankCommissionRate) / 100m;
                merchantTransaction.PfCommissionAmount = merchantTransaction.PfPerTransactionFee +
                                                         pricingProfileItem.CommissionRate / 100m *
                                                         merchantTransaction.Amount;
                merchantTransaction.PfNetCommissionAmount = merchantTransaction.PfCommissionAmount -
                                                            merchantTransaction.BankCommissionAmount;
                merchantTransaction.PfCommissionRate = pricingProfileItem.CommissionRate;
                merchantTransaction.ParentMerchantCommissionAmount = pricingProfileItem.ParentMerchantCommissionRate /
                    100m * merchantTransaction.Amount;
                merchantTransaction.ParentMerchantCommissionRate = pricingProfileItem.ParentMerchantCommissionRate;
                merchantTransaction.AmountWithoutParentMerchantCommission = merchantTransaction.Amount -
                                                                            merchantTransaction
                                                                                .ParentMerchantCommissionAmount;
                merchantTransaction.AmountWithoutCommissions = merchantTransaction.Amount -
                                                               merchantTransaction.PfCommissionAmount -
                                                               merchantTransaction.ParentMerchantCommissionAmount;
                merchantTransaction.AmountWithoutBankCommission =
                    merchantTransaction.Amount - merchantTransaction.BankCommissionAmount;
                merchantTransaction.BsmvAmount =
                    await BsmvAmountCalculateHelper.CalculateBsmvAmount(merchantTransaction.PfNetCommissionAmount,
                        _parameterService);
                merchantTransaction.PointCommissionAmount =
                    (request.Amount * merchantTransaction.PointCommissionRate) / 100m;
                merchantTransaction.ServiceCommissionAmount =
                    (request.Amount * merchantTransaction.ServiceCommissionRate) / 100m;
            }

            merchantTransaction.PricingProfileItemId = referenceMerchantTransaction.PricingProfileItemId;
            merchantTransaction.PfPaymentDate = referenceMerchantTransaction.PfPaymentDate;
            merchantTransaction.BankPaymentDate = DateTime.Now.AddDays(1);
            merchantTransaction.LastModifiedBy = parseUserId;

            if (referenceMerchantTransaction.PfPaymentDate <= DateTime.Now)
            {
                merchantTransaction.PfPaymentDate = DateTime.Now.AddDays(1);
            }

            var currency = await _currencyService.GetByNumberAsync(referenceMerchantTransaction.Currency);

            var merchantVpos = await GetSubMerchantCode(merchantTransaction.VposId, merchantTransaction.MerchantId);

            try
            {
                if (merchantTransaction.IsOnUsPayment)
                {
                    posResponse =
                        await _onUsPaymentService.ReturnOnUsPayment(request, referenceMerchantTransaction, currency);
                }
                else
                {
                    var bankService = _vposServiceFactory.GetVposServices(vpos, merchant.Id,
                        referenceMerchantTransaction.IsInsurancePayment);
                    posResponse = await bankService.Refund(new PosRefundRequest
                    {
                        Currency = referenceMerchantTransaction.Currency,
                        CurrencyCode = currency.Code,
                        OrderNumber = bankTransaction.OrderId,
                        OrgOrderNumber = orgOrderId,
                        LanguageCode = request.LanguageCode,
                        Amount = request.Amount,
                        SubMerchantCode = referenceBankTransaction.SubMerchantCode,
                        CardBrand = CardHelper.GetCardBrand(referenceBankTransaction.CardNumber),
                        ClientIp = request.ClientIpAddress,
                        SubMerchantId = merchant.Number,
                        SubMerchantName = merchant.Name,
                        SubMerchantCity = merchant.Customer.CityName,
                        SubMerchantTaxNumber = merchant.Customer.TaxNumber,
                        SubMerchantCountry = merchant.Customer.Country.ToString(),
                        SubMerchantMcc = merchant.MccCode,
                        SubMerchantUrl = merchant.WebSiteUrl,
                        SubMerchantGlobalMerchantId = merchant.GlobalMerchantId,
                        SubMerchantPostalCode = merchant.Customer.PostalCode,
                        TotalAmount = referenceBankTransaction.Amount,
                        BankOrderId = referenceBankTransaction.BankOrderId,
                        ProvisionNumber = referenceBankTransaction.ApprovalCode,
                        RRN = referenceBankTransaction.RrnNumber,
                        Stan = referenceBankTransaction.Stan,
                        OrderDate = referenceMerchantTransaction.TransactionDate,
                        OrgAuthProcessOrderNo = originalAuthProcess.OrderId,
                        IsTopUpPayment = request.IsTopUpPayment,
                        ServiceProviderPspMerchantId = merchantVpos.ServiceProviderPspMerchantId
                    });
                }
            }
            catch (TaskCanceledException exception)
                when (exception.InnerException is TimeoutException)
            {
                await InsertTimeoutTransactionAsync(merchantTransaction, referenceBankTransaction, bankTransaction,
                    request.ClientIpAddress);

                return new ReturnResponse
                {
                    IsSucceed = false,
                    ErrorCode = GenericErrorCode,
                    ErrorMessage = "InternalError",
                    ConversationId = request.ConversationId,
                    ApprovalStatus = ReturnApprovalStatus.None,
                };
            }

            if (posResponse.IsSuccess && referenceMerchantTransaction.IsPerInstallment == true)
            {
                await CalculateInstallmentTransactions(referenceMerchantTransaction, pricingProfileItem, remainingReturnAmount, parseUserId, merchantTransaction.Id);
            }

            return posResponse.IsSuccess
                ? await MarkAsCompletedAsync(merchantTransaction, bankTransaction, referenceMerchantTransaction,
                    referenceBankTransaction, posResponse, request.Amount, totalRefundAmount, parseUserId,
                    merchant.Number)
                : await MarkAsFailedAsync(merchantTransaction, bankTransaction, posResponse, request.LanguageCode);
        }
        catch (Exception exception)
        {
            if (posResponse is not null)
            {
                return await RetryDbUpdateAsync(merchantTransaction, bankTransaction, referenceMerchantTransaction,
                    referenceBankTransaction, posResponse, request.Amount, request.LanguageCode, totalRefundAmount,
                    request.ClientIpAddress, parseUserId, merchant.Number);
            }

            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();

            merchantTransaction.ResponseCode =
                (exception is ApiException apiException)
                    ? apiException.Code
                    : GenericErrorCode;
            merchantTransaction.ResponseDescription = exception.Message;
            merchantTransaction.TransactionStatus = TransactionStatus.Fail;
            merchantTransaction.TransactionEndDate = DateTime.Now;
            dbContext.Update(merchantTransaction);

            bankTransaction.TransactionEndDate = DateTime.Now;
            bankTransaction.BankResponseCode = string.Empty;
            bankTransaction.BankResponseDescription = string.Empty;
            bankTransaction.TransactionStatus = TransactionStatus.Fail;
            await dbContext.AddAsync(bankTransaction);

            await dbContext.SaveChangesAsync();

            return new ReturnResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = request.ConversationId,
                ApprovalStatus = ReturnApprovalStatus.None
            };
        }
    }
    private async Task<bool> CalculateInstallmentTransactions(
   MerchantTransaction merchantTransaction,
   PricingProfileItem pricingProfileItem,
   decimal remainingReturnAmount,
   string parseUserId,
   Guid newMerchantTransaction)
    {
        try
        {
            decimal baseInstallmentAmount = Math.Floor(merchantTransaction.Amount / merchantTransaction.InstallmentCount * 100) / 100m;
            decimal totalDistributed = baseInstallmentAmount * merchantTransaction.InstallmentCount;
            decimal remainder = merchantTransaction.Amount - totalDistributed;

            for (int i = 1; i <= merchantTransaction.InstallmentCount; i++)
            {
                bool isFirstInstallment = i == 1;

                decimal installmentAmount = isFirstInstallment
                    ? baseInstallmentAmount + remainder
                    : baseInstallmentAmount;

                var referenceMerchantInstallmentTransaction = await _merchantInstallmentTransactionRepository.GetAll().Where(b => b.MerchantTransactionId == merchantTransaction.Id && b.InstallmentCount == i).FirstOrDefaultAsync();

                var installmentTransaction = await PopulateInitialMerchantInstallmentTransaction(referenceMerchantInstallmentTransaction, i, installmentAmount,  pricingProfileItem, remainingReturnAmount, parseUserId, newMerchantTransaction);

            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SaveMerchantInstallmentTransactionException: {exception}");
            return false;
        }


        return true;
    }
    private async Task<bool> PopulateInitialMerchantInstallmentTransaction(MerchantInstallmentTransaction referenceMerchantInstallmentTransaction,
       int installmentNumber, decimal amount,
    PricingProfileItem pricingProfileItem,
    decimal remainingReturnAmount,
    string parseUserId,
    Guid newMerchantTransaction)
    {
        var user = await _merchantUserRepository.GetAll()
            .Where(s => s.UserId == Guid.Parse(parseUserId))
            .Select(s => new { FullName = $"{s.Name} {s.Surname}" })
            .FirstOrDefaultAsync();

        if (user is null)
        {
            user = await _subMerchantUserRepository.GetAll()
                .Where(s => s.UserId == Guid.Parse(parseUserId))
                .Select(s => new { FullName = $"{s.Name} {s.Surname}" })
                .FirstOrDefaultAsync();
        }

        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();

        var merchantInstallmentTransaction = new MerchantInstallmentTransaction
        {
            RecordStatus = RecordStatus.Active,
            CreateDate = DateTime.Now,
            CreatedBy = parseUserId,
            CreatedNameBy = user is not null ? user.FullName : null,
            IpAddress = referenceMerchantInstallmentTransaction.IpAddress,
            TransactionStartDate = referenceMerchantInstallmentTransaction.TransactionStartDate,
            TransactionStatus = referenceMerchantInstallmentTransaction.TransactionStatus,
            TransactionDate = referenceMerchantInstallmentTransaction.TransactionDate,
            TransactionType = referenceMerchantInstallmentTransaction.TransactionType,
            ConversationId = referenceMerchantInstallmentTransaction.ConversationId,
            MerchantId = referenceMerchantInstallmentTransaction.MerchantId,
            IntegrationMode = referenceMerchantInstallmentTransaction.IntegrationMode,
            IsPreClose = referenceMerchantInstallmentTransaction.IsPreClose,
            IsReverse = referenceMerchantInstallmentTransaction.IsReverse,
            IsReturn = referenceMerchantInstallmentTransaction.IsReturn,
            IsManualReturn = referenceMerchantInstallmentTransaction.IsManualReturn,
            IsOnUsPayment = referenceMerchantInstallmentTransaction.IsOnUsPayment,
            IsInsurancePayment = referenceMerchantInstallmentTransaction.IsInsurancePayment,
            ReturnAmount = referenceMerchantInstallmentTransaction.ReturnAmount,
            BankCommissionRate = referenceMerchantInstallmentTransaction.BankCommissionRate,
            Currency = referenceMerchantInstallmentTransaction.Currency,
            PointAmount = referenceMerchantInstallmentTransaction.PointAmount,
            InstallmentCount = installmentNumber,
            ThreeDSessionId = referenceMerchantInstallmentTransaction.ThreeDSessionId,
            Is3ds = referenceMerchantInstallmentTransaction.Is3ds,
            CardNumber = referenceMerchantInstallmentTransaction.CardNumber,
            BinNumber = referenceMerchantInstallmentTransaction.BinNumber,
            HasCvv = referenceMerchantInstallmentTransaction.HasCvv,
            HasExpiryDate = referenceMerchantInstallmentTransaction.HasExpiryDate,
            IsAmex = referenceMerchantInstallmentTransaction.IsAmex,
            IsInternational = referenceMerchantInstallmentTransaction.IsInternational,
            VposId = referenceMerchantInstallmentTransaction.VposId,
            IssuerBankCode = referenceMerchantInstallmentTransaction.IssuerBankCode,
            AcquireBankCode = referenceMerchantInstallmentTransaction.AcquireBankCode,
            CardTransactionType = referenceMerchantInstallmentTransaction.CardTransactionType,
            CardHolderName = referenceMerchantInstallmentTransaction.CardHolderName,
            MerchantCustomerName = referenceMerchantInstallmentTransaction.MerchantCustomerName,
            MerchantCustomerPhoneNumber = referenceMerchantInstallmentTransaction.MerchantCustomerPhoneNumber,
            MerchantCustomerPhoneCode = referenceMerchantInstallmentTransaction.MerchantCustomerPhoneCode,
            LanguageCode = referenceMerchantInstallmentTransaction.LanguageCode,
            BatchStatus = referenceMerchantInstallmentTransaction.BatchStatus,
            OrderId = referenceMerchantInstallmentTransaction.OrderId,
            PostingItemId = referenceMerchantInstallmentTransaction.PostingItemId,
            BlockageStatus = referenceMerchantInstallmentTransaction.BlockageStatus,
            LastChargebackActivityDate = referenceMerchantInstallmentTransaction.LastChargebackActivityDate,
            IsTopUpPayment = referenceMerchantInstallmentTransaction.IsTopUpPayment,
            PfTransactionSource = referenceMerchantInstallmentTransaction.PfTransactionSource,
            CardHolderIdentityNumber = referenceMerchantInstallmentTransaction.CardHolderIdentityNumber,
            EndOfDayStatus = referenceMerchantInstallmentTransaction.EndOfDayStatus,
            SubMerchantId = referenceMerchantInstallmentTransaction.SubMerchantId,
            SubMerchantName = referenceMerchantInstallmentTransaction.SubMerchantName,
            SubMerchantNumber = referenceMerchantInstallmentTransaction.SubMerchantNumber,
            CardType = referenceMerchantInstallmentTransaction.CardType,
            Description = referenceMerchantInstallmentTransaction.Description,
            IsChargeback = referenceMerchantInstallmentTransaction.IsChargeback,
            IsSuspecious = referenceMerchantInstallmentTransaction.IsSuspecious,
            MerchantPhysicalPosId = referenceMerchantInstallmentTransaction.MerchantPhysicalPosId,
            VposName = referenceMerchantInstallmentTransaction.VposName,
            MerchantTransactionId = newMerchantTransaction,
            PreCloseDate = referenceMerchantInstallmentTransaction.PreCloseDate,
            PreCloseTransactionId = referenceMerchantInstallmentTransaction.PreCloseTransactionId,
            ProvisionNumber = referenceMerchantInstallmentTransaction.ProvisionNumber,
            ResponseCode = referenceMerchantInstallmentTransaction.ResponseCode,
            ResponseDescription = referenceMerchantInstallmentTransaction.ResponseDescription,
            ReturnDate = referenceMerchantInstallmentTransaction.ReturnDate,
            ReturnedTransactionId = referenceMerchantInstallmentTransaction.Id.ToString(),
            ReturnStatus = referenceMerchantInstallmentTransaction.ReturnStatus,
            ReverseDate = referenceMerchantInstallmentTransaction.ReverseDate,
            TransactionEndDate = referenceMerchantInstallmentTransaction.TransactionEndDate,
            SuspeciousDescription = referenceMerchantInstallmentTransaction.SuspeciousDescription,

        };
        merchantInstallmentTransaction.PointCommissionRate = referenceMerchantInstallmentTransaction.PointCommissionRate;
        merchantInstallmentTransaction.ServiceCommissionRate = referenceMerchantInstallmentTransaction.ServiceCommissionRate;

        if (merchantInstallmentTransaction.Amount == amount)
        {
            merchantInstallmentTransaction.BankCommissionAmount = referenceMerchantInstallmentTransaction.BankCommissionAmount;

            merchantInstallmentTransaction.PfCommissionAmount = referenceMerchantInstallmentTransaction.PfCommissionAmount;

            merchantInstallmentTransaction.PfNetCommissionAmount = referenceMerchantInstallmentTransaction.PfNetCommissionAmount;

            merchantInstallmentTransaction.PfCommissionRate = referenceMerchantInstallmentTransaction.PfCommissionRate;
            merchantInstallmentTransaction.PfPerTransactionFee = referenceMerchantInstallmentTransaction.PfPerTransactionFee;

            merchantInstallmentTransaction.ParentMerchantCommissionAmount = referenceMerchantInstallmentTransaction.ParentMerchantCommissionAmount;

            merchantInstallmentTransaction.ParentMerchantCommissionRate = referenceMerchantInstallmentTransaction.ParentMerchantCommissionRate;

            merchantInstallmentTransaction.AmountWithoutCommissions = referenceMerchantInstallmentTransaction.AmountWithoutCommissions;

            merchantInstallmentTransaction.AmountWithoutBankCommission = referenceMerchantInstallmentTransaction.AmountWithoutBankCommission;

            merchantInstallmentTransaction.AmountWithoutParentMerchantCommission = referenceMerchantInstallmentTransaction.AmountWithoutParentMerchantCommission;

            merchantInstallmentTransaction.BsmvAmount = referenceMerchantInstallmentTransaction.BsmvAmount;

            merchantInstallmentTransaction.PointCommissionAmount = referenceMerchantInstallmentTransaction.PointCommissionAmount;

            merchantInstallmentTransaction.ServiceCommissionAmount = referenceMerchantInstallmentTransaction.ServiceCommissionAmount;
        }
        else
        {
            merchantInstallmentTransaction.BankCommissionAmount = (merchantInstallmentTransaction.BankCommissionRate / 100m) * amount;

            merchantInstallmentTransaction.PfPerTransactionFee = remainingReturnAmount == 0
                ? pricingProfileItem.PricingProfile.PerTransactionFee
                : 0;

            merchantInstallmentTransaction.PfCommissionAmount = merchantInstallmentTransaction.PfPerTransactionFee
                + pricingProfileItem.CommissionRate / 100m * amount;

            merchantInstallmentTransaction.PfNetCommissionAmount = merchantInstallmentTransaction.PfCommissionAmount
                - merchantInstallmentTransaction.BankCommissionAmount;

            merchantInstallmentTransaction.PfCommissionRate = pricingProfileItem.CommissionRate;

            merchantInstallmentTransaction.ParentMerchantCommissionAmount =
                pricingProfileItem.ParentMerchantCommissionRate / 100m * amount;

            merchantInstallmentTransaction.ParentMerchantCommissionRate =
                pricingProfileItem.ParentMerchantCommissionRate;

            merchantInstallmentTransaction.AmountWithoutCommissions = amount
                - merchantInstallmentTransaction.PfCommissionAmount
                - merchantInstallmentTransaction.ParentMerchantCommissionAmount;

            merchantInstallmentTransaction.AmountWithoutBankCommission = amount
                - merchantInstallmentTransaction.BankCommissionAmount;

            merchantInstallmentTransaction.AmountWithoutParentMerchantCommission = amount
                - merchantInstallmentTransaction.ParentMerchantCommissionAmount;


            merchantInstallmentTransaction.BsmvAmount = await BsmvAmountCalculateHelper
                .CalculateBsmvAmount(merchantInstallmentTransaction.PfNetCommissionAmount, _parameterService);

            merchantInstallmentTransaction.PointCommissionAmount =
                (amount * merchantInstallmentTransaction.PointCommissionRate) / 100m;

            merchantInstallmentTransaction.ServiceCommissionAmount =
                (amount * merchantInstallmentTransaction.ServiceCommissionRate) / 100m;
        }

        merchantInstallmentTransaction.Amount = amount;
        merchantInstallmentTransaction.PricingProfileItemId = referenceMerchantInstallmentTransaction.PricingProfileItemId;
        merchantInstallmentTransaction.PfPaymentDate = merchantInstallmentTransaction.PfPaymentDate;
        if (referenceMerchantInstallmentTransaction.PfPaymentDate <= DateTime.Now)
        {
            merchantInstallmentTransaction.PfPaymentDate = DateTime.Now.AddDays(1);
        }

        merchantInstallmentTransaction.BankPaymentDate = DateTime.Now.AddDays(1);

        await dbContext.AddAsync(merchantInstallmentTransaction);

        var transactionStatus = amount == referenceMerchantInstallmentTransaction.Amount
                ? TransactionStatus.Returned
                : TransactionStatus.PartiallyReturned;

        referenceMerchantInstallmentTransaction.ReturnAmount = amount;
        referenceMerchantInstallmentTransaction.ReturnDate = DateTime.Now;
        referenceMerchantInstallmentTransaction.IsReturn = true;
        referenceMerchantInstallmentTransaction.TransactionStatus = transactionStatus;
        referenceMerchantInstallmentTransaction.LastModifiedBy = parseUserId;
        referenceMerchantInstallmentTransaction.ReturnStatus = ReturnStatus.Approved;

        dbContext.Update(referenceMerchantInstallmentTransaction);

        await dbContext.SaveChangesAsync();

        return true;
    }
    private async Task<MerchantVpos> GetSubMerchantCode(Guid vposId, Guid merchantId)
    {
        var subMerchant = await _merchantVposRepository
            .GetAll()
            .FirstOrDefaultAsync(s => s.RecordStatus == RecordStatus.Active &&
                                      s.TerminalStatus == TerminalStatus.Active &&
                                      s.VposId == vposId &&
                                      s.MerchantId == merchantId);

        return subMerchant ?? new MerchantVpos();
    }

    private async Task<MerchantTransaction> SaveMerchantTransactionAsync(ReturnCommand request, SubMerchant subMerchant,
        string parseUserId)
    {
        var user = await _merchantUserRepository.GetAll()
            .Where(s => s.UserId == Guid.Parse(parseUserId))
            .Select(s => new { FullName = $"{s.Name} {s.Surname}" })
            .FirstOrDefaultAsync();

        if (user is null)
        {
            user = await _subMerchantUserRepository.GetAll()
                .Where(s => s.UserId == Guid.Parse(parseUserId))
                .Select(s => new { FullName = $"{s.Name} {s.Surname}" })
                .FirstOrDefaultAsync();
        }

        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();

        var merchantTransaction = new MerchantTransaction
        {
            RecordStatus = RecordStatus.Active,
            CreateDate = DateTime.Now,
            CreatedNameBy = user is not null ? user.FullName : null,
            CreatedBy = parseUserId,
            IpAddress = request.ClientIpAddress,
            TransactionStartDate = DateTime.Now,
            TransactionStatus = TransactionStatus.Pending,
            TransactionType = TransactionType.Return,
            TransactionDate = DateTime.Now.Date,
            ConversationId = request.ConversationId,
            MerchantId = request.MerchantId,
            IntegrationMode = IntegrationMode.Api,
            IsPreClose = false,
            IsReverse = false,
            IsReturn = false,
            IsManualReturn = false,
            IsOnUsPayment = false,
            IsInsurancePayment = false,
            ReturnAmount = 0,
            BankCommissionAmount = 0,
            BankCommissionRate = 0,
            Currency = 0,
            Amount = request.Amount,
            PointAmount = 0,
            PointCommissionRate = 0,
            PointCommissionAmount = 0,
            ServiceCommissionRate = 0,
            ServiceCommissionAmount = 0,
            InstallmentCount = 0,
            ThreeDSessionId = string.Empty,
            Is3ds = false,
            CardNumber = string.Empty,
            BinNumber = string.Empty,
            HasCvv = false,
            HasExpiryDate = false,
            IsAmex = false,
            IsInternational = false,
            VposId = Guid.Empty,
            IssuerBankCode = 0,
            AcquireBankCode = 0,
            CardTransactionType = null,
            BatchStatus = BatchStatus.Pending,
            ReturnStatus = ReturnStatus.NoAction,
            OrderId = string.Empty,
            PostingItemId = Guid.Empty,
            BlockageStatus = BlockageStatus.None,
            LastChargebackActivityDate = DateTime.MinValue,
            IsTopUpPayment = request.IsTopUpPayment,
            PfTransactionSource = PfTransactionSource.VirtualPos,
            EndOfDayStatus = EndOfDayStatus.Pending
        };

        if (!string.IsNullOrEmpty(subMerchant.Name))
        {
            merchantTransaction.SubMerchantId = subMerchant.Id;
            merchantTransaction.SubMerchantName = subMerchant.Name;
            merchantTransaction.SubMerchantNumber = subMerchant.Number;
        }

        await dbContext.AddAsync(merchantTransaction);
        await dbContext.SaveChangesAsync();

        return merchantTransaction;
    }

    private async Task<decimal> CheckReturnAmountAsync(MerchantDto merchant, decimal amount) //Submerchant
    {
        DateTime today = DateTime.Now.Date;

        var transactionAmounts = await _merchantTransactionRepository.GetAll()
            .Where(s => s.TransactionDate == today
                        && s.MerchantId == merchant.Id
                        && (s.TransactionStatus != TransactionStatus.Fail &&
                            s.TransactionStatus != TransactionStatus.Reversed)
                        && s.BatchStatus != BatchStatus.Completed
                        && s.RecordStatus == RecordStatus.Active)
            .GroupBy(s => s.TransactionType)
            .Select(g => new
            {
                TransactionType = g.Key,
                TotalAmount = g.Sum(s => s.Amount)
            })
            .ToListAsync();

        var authPostAuthTotal = transactionAmounts
            .Where(r => r.TransactionType == TransactionType.Auth || r.TransactionType == TransactionType.PostAuth)
            .Sum(r => r.TotalAmount);

        return authPostAuthTotal -
               transactionAmounts
                   .Where(r => r.TransactionType == TransactionType.Return)
                   .Sum(r => r.TotalAmount);
    }

    private static BankTransaction PopulateInitialBankTransaction(ReturnCommand request,
        MerchantTransaction merchantTransaction)
    {
        var bankTransaction = new BankTransaction
        {
            RecordStatus = RecordStatus.Active,
            CreateDate = DateTime.Now,
            CreatedBy = request.MerchantId.ToString(),
            TransactionStartDate = DateTime.Now,
            TransactionStatus = TransactionStatus.Pending,
            TransactionType = TransactionType.Return,
            Amount = request.Amount,
            PointAmount = 0,
            InstallmentCount = 0,
            Is3ds = false,
            IsReverse = false,
            OrderId = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
            MerchantTransactionId = merchantTransaction.Id,
            EndOfDayStatus = EndOfDayStatus.Pending
        };
        return bankTransaction;
    }

    private async Task<ReturnResponse> MarkAsCompletedAsync(MerchantTransaction merchantTransaction,
        BankTransaction bankTransaction, MerchantTransaction referenceMerchantTransaction,
        BankTransaction referenceBankTransaction, PosRefundResponse posResponse,
        decimal amount, decimal totalRefundAmount, string parseUserId, string merchantNumber)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var transactionStatus = amount == referenceBankTransaction.Amount
                ? TransactionStatus.Returned
                : TransactionStatus.PartiallyReturned;

            referenceMerchantTransaction.ReturnAmount = totalRefundAmount;
            referenceMerchantTransaction.ReturnDate = DateTime.Now;
            referenceMerchantTransaction.IsReturn = true;
            referenceMerchantTransaction.TransactionStatus = transactionStatus;
            referenceMerchantTransaction.LastModifiedBy = parseUserId;
            referenceMerchantTransaction.ReturnStatus = ReturnStatus.Approved;

            dbContext.Update(referenceMerchantTransaction);

            merchantTransaction.TransactionEndDate = DateTime.Now;
            merchantTransaction.ResponseCode = GenericSuccessCode;
            merchantTransaction.ResponseDescription = GenericSuccessCode;
            merchantTransaction.TransactionStatus = TransactionStatus.Success;
            merchantTransaction.ProvisionNumber = posResponse.AuthCode;

            if (await _basePaymentService.GetIsPaymentDateWillBeShiftedAsync(merchantTransaction.TransactionEndDate,
                    merchantTransaction.AcquireBankCode))
            {
                merchantTransaction.BankPaymentDate = merchantTransaction.BankPaymentDate.AddDays(1);
            }

            dbContext.Update(merchantTransaction);

            referenceBankTransaction.TransactionStatus = transactionStatus;
            referenceBankTransaction.LastModifiedBy = parseUserId;

            dbContext.Update(referenceBankTransaction);

            bankTransaction.TransactionEndDate = DateTime.Now;
            bankTransaction.BankOrderId = posResponse.OrderNumber;
            bankTransaction.ApprovalCode = posResponse.AuthCode;
            bankTransaction.RrnNumber = posResponse.RrnNumber;
            bankTransaction.BankResponseCode = posResponse.ResponseCode;
            bankTransaction.BankResponseDescription = posResponse.ResponseMessage;
            bankTransaction.TransactionStatus = TransactionStatus.Success;
            bankTransaction.BankTransactionDate = posResponse.TrxDate;

            await dbContext.AddAsync(bankTransaction);

            await dbContext.SaveChangesAsync();

            transactionScope.Complete();
        });

        try
        {
            await PublishDecrementLimitAsync(referenceMerchantTransaction.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError($"PublishDecrementLimitException: {exception}");
        }

        try
        {
            var referenceNumber = (merchantNumber + referenceBankTransaction.OrderId).PadLeft(20, '0');
            var returnSettings = _vaultClient.GetSecretValue<ReturnSettingsModel>("PFSecrets", "ReturnSettings", null);
            if (returnSettings.IsManuelReturnAllowed || returnSettings.IsManuelExcessReturnAllowed)
            {
                await _pfReturnTransactionService.VerifyPfReturnTransactionAsync(new()
                {
                    Amount = amount,
                    MerchantNumber = merchantNumber,
                    OrderNumber = referenceBankTransaction.OrderId,
                    Description = referenceNumber
                });
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"VerifyPfReturnTransactionException after ReturnCompleted: {exception}");
        }

        return new ReturnResponse
        {
            IsSucceed = true,
            ConversationId = merchantTransaction.ConversationId,
            ErrorCode = string.Empty,
            ErrorMessage = string.Empty,
            ApprovalStatus = ReturnApprovalStatus.Approved,
            ProvisionNumber = posResponse?.AuthCode,
            ResponseCode = posResponse?.ResponseCode,
            ResponseMessage = posResponse?.ResponseMessage
        };
    }

    private async Task PublishDecrementLimitAsync(Guid merchantTransactionId)
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.DecrementLimits"));
        await endpoint.Send(new IncrementLimits
        {
            MerchantTransactionId = merchantTransactionId
        }, tokenSource.Token);
    }

    private async Task<ReturnResponse> MarkAsFailedAsync(MerchantTransaction merchantTransaction,
        BankTransaction bankTransaction,
        PosRefundResponse posResponse,
        string languageCode)
    {
        var merchantError =
            await _errorCodeService.GetMerchantResponseCodeByBankCodeAsync(bankTransaction.AcquireBankCode,
                posResponse.ResponseCode, posResponse.ResponseMessage,
                languageCode);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            merchantTransaction.ResponseCode = merchantError.ResponseCode;
            merchantTransaction.ResponseDescription = merchantError.DisplayMessage;
            merchantTransaction.TransactionEndDate = DateTime.Now;
            merchantTransaction.TransactionStatus = TransactionStatus.Fail;
            dbContext.Update(merchantTransaction);

            bankTransaction.BankResponseCode = posResponse.ResponseCode;
            bankTransaction.BankResponseDescription = posResponse.ResponseMessage;
            bankTransaction.TransactionEndDate = DateTime.Now;
            bankTransaction.TransactionStatus = TransactionStatus.Fail;
            bankTransaction.BankTransactionDate = posResponse.TrxDate;

            await dbContext.AddAsync(bankTransaction);

            await dbContext.SaveChangesAsync();

            transactionScope.Complete();
        });
        return new ReturnResponse
        {
            IsSucceed = false,
            ErrorCode = merchantError.ResponseCode,
            ErrorMessage = merchantError.DisplayMessage,
            ConversationId = merchantTransaction.ConversationId,
            ApprovalStatus = ReturnApprovalStatus.None,
            ResponseCode = posResponse?.ResponseCode,
            ResponseMessage = posResponse?.ResponseMessage
        };
    }

    private async Task InsertValidationLogAsync(ReturnCommand request, ValidationResponse validationResponse)
    {
        await _apiRequestValidationLogRepository.AddAsync(new MerchantApiValidationLog
        {
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            Amount = request.Amount,
            Currency = string.Empty,
            MerchantId = request.MerchantId,
            CardToken = string.Empty,
            ConversationId = request.ConversationId,
            InstallmentCount = 0,
            LanguageCode = request.LanguageCode,
            PointAmount = 0,
            TransactionType = TransactionType.Return,
            ClientIpAddress = request.ClientIpAddress,
            OriginalReferenceNumber = request.OrderId,
            ThreeDSessionId = string.Empty,
            ErrorCode = validationResponse.Code,
            ErrorMessage = validationResponse.Message
        });
    }

    private async Task<ReturnResponse> GetValidationResponseAsync(string errorCode, string languageCode,
        string conversationId)
    {
        var apiResponse = await _errorCodeService.GetApiResponseCode(errorCode, languageCode);

        return new ReturnResponse
        {
            IsSucceed = false,
            ErrorCode = apiResponse.ResponseCode,
            ConversationId = conversationId,
            ErrorMessage = apiResponse.DisplayMessage,
            ApprovalStatus = ReturnApprovalStatus.None,
        };
    }

    private async Task InsertTimeoutTransactionAsync(MerchantTransaction merchantTransaction,
        BankTransaction referenceBankTransaction,
        BankTransaction bankTransaction, string clientIpAddress)
    {
        await _timeoutTransactionRepository.AddAsync(new TimeoutTransaction
        {
            MerchantId = merchantTransaction.MerchantId,
            ConversationId = merchantTransaction.ConversationId,
            TransactionType = merchantTransaction.TransactionType,
            TimeoutTransactionStatus = TimeoutTransactionStatus.Pending,
            TransactionDate = DateTime.Now,
            VposId = merchantTransaction.VposId,
            MerchantTransactionId = merchantTransaction.Id,
            BankTransactionId = bankTransaction.Id,
            AcquireBankCode = merchantTransaction.AcquireBankCode,
            OriginalOrderId = referenceBankTransaction.OrderId,
            OrderId = bankTransaction.OrderId,
            SubMerchantCode = referenceBankTransaction.SubMerchantCode,
            Currency = merchantTransaction.Currency,
            LanguageCode = merchantTransaction.LanguageCode,
            Amount = referenceBankTransaction.Amount,
            CardNumber = referenceBankTransaction.CardNumber,
            CreateDate = DateTime.Now,
            CreatedBy = "BATCH",
            RecordStatus = RecordStatus.Active,
            Description = "Transaction Timeout",
            ClientIpAddress = clientIpAddress
        });

        merchantTransaction.TransactionStatus = TransactionStatus.Fail;

        await _merchantTransactionRepository.UpdateAsync(merchantTransaction);
    }

    private async Task<ReturnResponse> RetryDbUpdateAsync(MerchantTransaction merchantTransaction,
        BankTransaction bankTransaction, MerchantTransaction referenceMerchantTransaction,
        BankTransaction referenceBankTransaction, PosRefundResponse posResponse,
        decimal amount,
        string languageCode,
        decimal totalRefundAmount,
        string clientIpAddress,
        string parseUserId,
        string merchantNumber)
    {
        try
        {
            return posResponse.IsSuccess
                ? await MarkAsCompletedAsync(merchantTransaction, bankTransaction, referenceMerchantTransaction,
                    referenceBankTransaction, posResponse, amount, totalRefundAmount, parseUserId, merchantNumber)
                : await MarkAsFailedAsync(merchantTransaction, bankTransaction, posResponse, languageCode);
        }
        catch (Exception exception)
        {
            if (posResponse.IsSuccess)
            {
                _logger.LogError(
                    $"Return MarkAsCompleteError: ReferenceMerchantTransactionId : {referenceMerchantTransaction.Id}" +
                    $" Error : {exception}");

                await InsertTimeoutTransactionAsync(merchantTransaction, referenceBankTransaction, bankTransaction,
                    clientIpAddress);
            }
            else
            {
                _logger.LogError(
                    $"Return MarkAsFailedError: ReferenceMerchantTransactionId : {referenceMerchantTransaction.Id} " +
                    $"Error : {exception}");
            }

            return new ReturnResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = merchantTransaction.ConversationId,
                ApprovalStatus = ReturnApprovalStatus.None
            };
        }
    }

    private static void SetAmount(ReturnCommand request, BankTransaction refBankTransaction)
    {
        request.Amount = request.Amount == 0
            ? refBankTransaction.Amount
            : request.Amount;
    }

    public async Task ManualReturnAsync(ManualReturnCommand request)
    {
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId ?? _applicationUserService.ApplicationUserId.ToString();

        try
        {
            var merchantTransaction = await _merchantTransactionRepository
                .GetAll()
                .Include(s => s.AcquireBank)
                .FirstOrDefaultAsync(s =>
                    s.RecordStatus == RecordStatus.Active &&
                    s.PfTransactionSource == PfTransactionSource.VirtualPos &&
                    (s.TransactionStatus != TransactionStatus.Fail &&
                     s.TransactionStatus != TransactionStatus.Returned) &&
                    s.Id == request.MerchantTransactionId &&
                    !s.IsReverse);

            if (merchantTransaction is null)
            {
                throw new NotFoundException(nameof(MerchantTransaction), request.MerchantTransactionId);
            }

            if (merchantTransaction.IsChargeback || merchantTransaction.IsSuspecious)
            {
                throw new TransactionNotReturnableException();
            }

            var merchant = await _merchantService.GetByIdAsync(merchantTransaction.MerchantId);

            var parentMerchantFinancialTransaction = true;
            if (merchant.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
            {
                var parentMerchant = await _merchantRepository.GetByIdAsync(merchant.ParentMerchantId);
                if (parentMerchant is not null)
                {
                    parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
                }
            }

            var referenceBankTransaction = await _bankTransactionRepository.GetAll()
                .FirstOrDefaultAsync(s =>
                    s.RecordStatus == RecordStatus.Active &&
                    s.OrderId == merchantTransaction.OrderId);

            if (referenceBankTransaction is null)
            {
                throw new InvalidReferenceNumberException();
            }

            request.Amount = request.Amount == 0 ? referenceBankTransaction.Amount : request.Amount;

            if (!merchant.PaymentReturnAllowed)
            {
                throw new NoReturnPaymentAllowedException();
            }

            if (!merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
            {
                throw new InvalidMerchantStatusException();
            }

            var limitReturnAmount = merchant.IsExcessReturnAllowed
                ? request.Amount
                : await CheckReturnAmountAsync(merchant, request.Amount);

            if (limitReturnAmount < request.Amount)
            {
                throw new DailyReturnAmountCannotGreaterAuthAmountException();
            }

            var merchantLimits =
                (await _merchantLimitService.GetFilterListAsync(
                    new GetFilterMerchantLimitsQuery
                    {
                        MerchantId = merchant.Id
                    }))
                .Items
                .Where(s => s.TransactionLimitType == TransactionLimitType.Return)
                .ToList();

            if (merchantLimits.Any())
            {
                var returnLimitResult = await CheckMerchantReturnPoolLimitsAsync(merchant, merchantLimits,
                    request.Amount, merchantTransaction.ConversationId);

                if (!returnLimitResult.IsSucceed)
                {
                    throw new CustomApiException(returnLimitResult.ResponseCode, returnLimitResult.ResponseMessage);
                }
            }

            var merchantBlockage = await _merchantBlockageRepository
                .GetAll()
                .FirstOrDefaultAsync(s => s.MerchantId == merchant.Id
                                          && s.RecordStatus == RecordStatus.Active);

            if (merchantBlockage != null)
            {
                var isBlockedTransaction = await _postingTransactionRepository.GetAll()
                    .AnyAsync(s => s.MerchantId == merchant.Id
                                   && s.OrderId == merchantTransaction.OrderId
                                   && s.BlockageStatus == BlockageStatus.Blocked);

                if (isBlockedTransaction)
                {
                    throw new CannotReturnMerchantHasBlockedAmountException();
                }
            }

            var totalRefundAmount = await _merchantTransactionRepository.GetAll()
                .Where(s =>
                    s.ReturnedTransactionId == merchantTransaction.Id.ToString() &&
                    s.MerchantId == merchant.Id &&
                    s.TransactionStatus == TransactionStatus.Success)
                .SumAsync(s => s.Amount);

            var totalReturnPoolAmount =
                (await _merchantReturnPoolService.GetMerchantReturnPoolByOrderIdAsync(merchantTransaction.OrderId))
                .Where(s => s.ReturnStatus == ReturnStatus.Pending)
                .Sum(p => p.Amount);

            if ((totalRefundAmount + totalReturnPoolAmount + request.Amount) > referenceBankTransaction.Amount)
            {
                throw new InvalidReturnAmountException();
            }

            var remainingReturnAmount = referenceBankTransaction.Amount -
                                        (totalRefundAmount + totalReturnPoolAmount + request.Amount);
            var returnTransaction = await PopulateReturnTransactionAsync(merchantTransaction, merchant, request,
                parseUserId, referenceBankTransaction.AcquireBankCode, remainingReturnAmount);

            if (await _basePaymentService.GetIsPaymentDateWillBeShiftedAsync(returnTransaction.TransactionEndDate,
                    returnTransaction.AcquireBankCode))
            {
                returnTransaction.BankPaymentDate = returnTransaction.BankPaymentDate.AddDays(1);
            }

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();
            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var transactionStatus = request.Amount == referenceBankTransaction.Amount
                    ? TransactionStatus.Returned
                    : TransactionStatus.PartiallyReturned;

                merchantTransaction.ReturnAmount += request.Amount;
                merchantTransaction.ReturnDate = DateTime.Now;
                merchantTransaction.IsReturn = true;
                merchantTransaction.TransactionStatus = transactionStatus;
                merchantTransaction.LastModifiedBy = parseUserId;
                merchantTransaction.ReturnStatus = ReturnStatus.Approved;

                dbContext.Update(merchantTransaction);

                await dbContext.AddAsync(returnTransaction);

                referenceBankTransaction.TransactionStatus = transactionStatus;
                referenceBankTransaction.LastModifiedBy = parseUserId;
                dbContext.Update(referenceBankTransaction);

                await _limitService.IncrementMerchantDailyUsageAsync(returnTransaction);
                await _limitService.IncrementMerchantMonthlyUsageAsync(returnTransaction);

                if (returnTransaction.SubMerchantId is not null)
                {
                    await _limitService.IncrementSubMerchantDailyUsageAsync(returnTransaction);
                    await _limitService.IncrementSubMerchantMonthlyUsageAsync(returnTransaction);
                }

                await dbContext.SaveChangesAsync();
                transactionScope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"ManualReturn exception occured : {exception}");
            throw;
        }
    }

    private async Task<MerchantTransaction> PopulateReturnTransactionAsync(
        MerchantTransaction merchantTransaction,
        MerchantDto merchant,
        ManualReturnCommand request,
        string parseUserId,
        int acquireBankCode,
        decimal remainingReturnAmount)
    {
        var user = await _userService.GetUserAsync(Guid.Parse(parseUserId));
        var createdNameBy = $"{user?.FirstName} {user?.LastName}";

        var orderId =
            await _orderNumberGeneratorService.GenerateForBankTransactionAsync(acquireBankCode, merchant.Number);

        var returnTransaction = new MerchantTransaction
        {
            RecordStatus = RecordStatus.Active,
            CreateDate = DateTime.Now,
            CreatedNameBy = createdNameBy,
            CreatedBy = parseUserId,
            IpAddress = _contextProvider.CurrentContext.ClientIpAddress,
            TransactionStartDate = DateTime.Now,
            TransactionEndDate = DateTime.Now,
            TransactionStatus = TransactionStatus.Success,
            TransactionType = TransactionType.Return,
            TransactionDate = DateTime.Now.Date,
            ConversationId = merchantTransaction.ConversationId,
            MerchantId = merchant.Id,
            IntegrationMode = IntegrationMode.Api,
            IsPreClose = false,
            IsReverse = false,
            IsReturn = false,
            IsManualReturn = true,
            IsOnUsPayment = false,
            IsInsurancePayment = merchantTransaction.IsInsurancePayment,
            ReturnAmount = 0,
            BankCommissionRate = merchantTransaction.BankCommissionRate,
            Currency = merchantTransaction.Currency,
            Amount = request.Amount,
            PointAmount = 0,
            InstallmentCount = merchantTransaction.InstallmentCount,
            ThreeDSessionId = merchantTransaction.ThreeDSessionId,
            Is3ds = merchantTransaction.Is3ds,
            CardType = merchantTransaction.CardType,
            CardNumber = merchantTransaction.CardNumber,
            BinNumber = merchantTransaction.BinNumber,
            HasCvv = merchantTransaction.HasCvv,
            HasExpiryDate = merchantTransaction.HasExpiryDate,
            IsAmex = merchantTransaction.IsAmex,
            IsInternational = merchantTransaction.IsInternational,
            VposId = merchantTransaction.VposId,
            VposName = merchantTransaction.VposName,
            IssuerBankCode = merchantTransaction.IssuerBankCode,
            AcquireBankCode = merchantTransaction.AcquireBankCode,
            CardTransactionType = merchantTransaction.CardTransactionType,
            BatchStatus = BatchStatus.Pending,
            ReturnStatus = ReturnStatus.Approved,
            ReturnedTransactionId = merchantTransaction.Id.ToString(),
            OrderId = $"MAN-{orderId}",
            LanguageCode = merchantTransaction.LanguageCode,
            PricingProfileItemId = merchantTransaction.PricingProfileItemId,
            PfPaymentDate = await _calendarService.NextWorkDayAsync(DateTime.Now, "TUR"),
            BankPaymentDate = DateTime.Now.AddDays(1),
            LastModifiedBy = parseUserId,
            ResponseCode = GenericSuccessCode,
            ResponseDescription = GenericSuccessCode,
            PostingItemId = Guid.Empty,
            BlockageStatus = BlockageStatus.None,
            LastChargebackActivityDate = DateTime.MinValue,
            PfTransactionSource = merchantTransaction.PfTransactionSource,
            CardHolderIdentityNumber = merchantTransaction.CardHolderIdentityNumber,
            EndOfDayStatus = EndOfDayStatus.Pending
        };

        if (request.Amount == merchantTransaction.Amount)
        {
            returnTransaction.BankCommissionAmount = merchantTransaction.BankCommissionAmount;
            returnTransaction.PfCommissionAmount = merchantTransaction.PfCommissionAmount;
            returnTransaction.PfNetCommissionAmount = merchantTransaction.PfNetCommissionAmount;
            returnTransaction.PfCommissionRate = merchantTransaction.PfCommissionRate;
            returnTransaction.PfPerTransactionFee = merchantTransaction.PfPerTransactionFee;
            returnTransaction.AmountWithoutCommissions = merchantTransaction.AmountWithoutCommissions;
            returnTransaction.AmountWithoutBankCommission = merchantTransaction.AmountWithoutBankCommission;
            returnTransaction.BsmvAmount = merchantTransaction.BsmvAmount;
            returnTransaction.PointAmount = merchantTransaction.PointAmount;
            returnTransaction.PointCommissionRate = merchantTransaction.PointCommissionRate;
            returnTransaction.PointCommissionAmount = merchantTransaction.PointCommissionAmount;
            returnTransaction.ServiceCommissionRate = merchantTransaction.ServiceCommissionRate;
            returnTransaction.ServiceCommissionAmount = merchantTransaction.ServiceCommissionAmount;
            returnTransaction.ParentMerchantCommissionAmount = merchantTransaction.ParentMerchantCommissionAmount;
            returnTransaction.ParentMerchantCommissionRate = merchantTransaction.ParentMerchantCommissionRate;
            returnTransaction.AmountWithoutParentMerchantCommission =
                merchantTransaction.AmountWithoutParentMerchantCommission;
        }
        else
        {
            var pricingProfileItem = await _pricingProfileItemRepository.GetAll()
                .Include(p => p.PricingProfile)
                .FirstOrDefaultAsync(p => p.Id == merchantTransaction.PricingProfileItemId);

            returnTransaction.PfPerTransactionFee =
                remainingReturnAmount == 0 ? pricingProfileItem.PricingProfile.PerTransactionFee : 0;
            returnTransaction.BankCommissionAmount = (request.Amount * merchantTransaction.BankCommissionRate) / 100m;
            returnTransaction.PfCommissionAmount = returnTransaction.PfPerTransactionFee +
                                                   pricingProfileItem.CommissionRate / 100m * returnTransaction.Amount;
            returnTransaction.PfNetCommissionAmount =
                returnTransaction.PfCommissionAmount - returnTransaction.BankCommissionAmount;
            returnTransaction.PfCommissionRate = pricingProfileItem.CommissionRate;
            returnTransaction.ParentMerchantCommissionAmount =
                pricingProfileItem.ParentMerchantCommissionRate / 100m * returnTransaction.Amount;
            returnTransaction.ParentMerchantCommissionRate = pricingProfileItem.ParentMerchantCommissionRate;
            returnTransaction.AmountWithoutParentMerchantCommission =
                returnTransaction.Amount - returnTransaction.ParentMerchantCommissionAmount;
            returnTransaction.AmountWithoutCommissions = returnTransaction.Amount -
                                                         returnTransaction.PfCommissionAmount -
                                                         returnTransaction.ParentMerchantCommissionAmount;
            returnTransaction.AmountWithoutBankCommission =
                returnTransaction.Amount - returnTransaction.BankCommissionAmount;
            returnTransaction.BsmvAmount =
                await BsmvAmountCalculateHelper.CalculateBsmvAmount(returnTransaction.PfNetCommissionAmount,
                    _parameterService);
            returnTransaction.PointCommissionRate = merchantTransaction.PointCommissionRate;
            returnTransaction.PointCommissionAmount =
                (returnTransaction.Amount * merchantTransaction.PointCommissionRate) / 100m;
            returnTransaction.ServiceCommissionRate = merchantTransaction.ServiceCommissionRate;
            returnTransaction.ServiceCommissionAmount =
                (returnTransaction.Amount * merchantTransaction.ServiceCommissionRate) / 100m;
        }

        return returnTransaction;
    }
}