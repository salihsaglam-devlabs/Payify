using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Application.Features.CardBins;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;
using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.PF.Application.Features.Tokens;
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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Transactions;
using static MassTransit.Logging.DiagnosticHeaders;
using TransactionStatus = LinkPara.PF.Domain.Enums.TransactionStatus;

namespace LinkPara.PF.Infrastructure.Services.PaymentServices;

public class PaymentService : IPaymentService
{
    private readonly string ThreeDFullSecureStatus = "Y";
    private readonly string ThreeDHalfSecureStatus = "A";

    private const string GenericErrorCode = "99";
    private const string GenericSuccessCode = "00";

    private readonly ILogger<PaymentService> _logger;
    private readonly ICardBinService _binService;
    private readonly ICurrencyService _currencyService;
    private readonly IMerchantService _merchantService;
    private readonly IPosRouterService _posRouterService;
    private readonly ICardTokenService _cardTokenService;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IGenericRepository<VposBankApiInfo> _vposBankApiInfoRepository;
    private readonly IGenericRepository<MerchantVpos> _merchantVpos;
    private readonly IGenericRepository<BankApiKey> _bankApiKeyRepository;
    private readonly IGenericRepository<AcquireBank> _acquireBankRepository;
    private readonly IGenericRepository<BankTransaction> _bankTransactionRepository;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IResponseCodeService _errorCodeService;
    private readonly IGenericRepository<MerchantApiValidationLog> _apiRequestValidationLogRepository;
    private readonly IGenericRepository<ThreeDVerification> _threeDVerificationRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IApplicationUserService _applicationUserService;
    private readonly VposServiceFactory _vposServiceFactory;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;
    private readonly IMccService _mccService;
    private readonly IFraudService _fraudService;
    private readonly IOnUsPaymentService _onUsPaymentService;
    private readonly IBasePaymentService _basePaymentService;
    private readonly IOrderNumberGeneratorService _orderNumberGeneratorService;
    private readonly ISubMerchantService _subMerchantService;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<CardLoyaltyException> _cardLoyaltyExceptionRepository;
    private readonly IVaultClient _vaultClient;
    private readonly IGenericRepository<CostProfile> _costProfile;
    private readonly IGenericRepository<MerchantInstallmentTransaction> _merchantInstallmentTransactionRepository;
    public PaymentService(
        ILogger<PaymentService> logger,
        ICardBinService binService,
        ICurrencyService currencyService,
        IMerchantService merchantService,
        IPosRouterService posRouterService,
        ICardTokenService cardTokenService,
        IGenericRepository<Vpos> vposRepository,
        IGenericRepository<VposBankApiInfo> vposBankApiInfoRepository,
        IGenericRepository<MerchantVpos> merchantVpos,
        IGenericRepository<BankApiKey> bankApiKeyRepository,
        IGenericRepository<AcquireBank> acquireBankRepository,
        IGenericRepository<BankTransaction> bankTransactionRepository,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IServiceScopeFactory scopeFactory,
        IResponseCodeService errorCodeService,
        IGenericRepository<MerchantApiValidationLog> apiRequestValidationLogRepository,
        IGenericRepository<ThreeDVerification> threeDVerificationRepository,
        IContextProvider contextProvider,
        IApplicationUserService applicationUserService,
        VposServiceFactory vposServiceFactory,
        IParameterService parameterService,
        IBus bus,
        IMccService mccService,
        IFraudService fraudService,
        IOnUsPaymentService onUsPaymentService,
        IBasePaymentService basePaymentService,
        IOrderNumberGeneratorService orderNumberGeneratorService,
        ISubMerchantService subMerchantService,
        IGenericRepository<Merchant> merchantRepository,
        IVaultClient vaultClient, IGenericRepository<CardLoyaltyException> cardLoyaltyExceptionRepository, IGenericRepository<CostProfile> costProfile, IGenericRepository<MerchantInstallmentTransaction> merchantInstallmentTransactionRepository)
    {
        _logger = logger;
        _binService = binService;
        _currencyService = currencyService;
        _vposRepository = vposRepository;
        _merchantService = merchantService;
        _posRouterService = posRouterService;
        _cardTokenService = cardTokenService;
        _merchantVpos = merchantVpos;
        _vposBankApiInfoRepository = vposBankApiInfoRepository;
        _bankApiKeyRepository = bankApiKeyRepository;
        _acquireBankRepository = acquireBankRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _merchantTransactionRepository = merchantTransactionRepository;
        _scopeFactory = scopeFactory;
        _errorCodeService = errorCodeService;
        _apiRequestValidationLogRepository = apiRequestValidationLogRepository;
        _threeDVerificationRepository = threeDVerificationRepository;
        _contextProvider = contextProvider;
        _applicationUserService = applicationUserService;
        _vposServiceFactory = vposServiceFactory;
        _parameterService = parameterService;
        _bus = bus;
        _mccService = mccService;
        _fraudService = fraudService;
        _onUsPaymentService = onUsPaymentService;
        _basePaymentService = basePaymentService;
        _orderNumberGeneratorService = orderNumberGeneratorService;
        _subMerchantService = subMerchantService;
        _merchantRepository = merchantRepository;
        _vaultClient = vaultClient;
        _cardLoyaltyExceptionRepository = cardLoyaltyExceptionRepository;
        _costProfile = costProfile;
        _merchantInstallmentTransactionRepository = merchantInstallmentTransactionRepository;
    }

    public async Task<ProvisionResponse> ProvisionAsync(ProvisionCommand request)
    {
        if (request.PaymentType != VposPaymentType.PostAuth)
        {
            var duplicateRecord = await CheckDuplicateRecordAsync(request);

            if (!duplicateRecord.IsSucceed)
            {
                return duplicateRecord;
            }
        }

        request.ClientIpAddress ??= _contextProvider.CurrentContext.ClientIpAddress;

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? userId : _applicationUserService.ApplicationUserId.ToString();

        var merchant = await _merchantService.GetByIdWithOptionsAsync(request.MerchantId, SubQueryOptions.None);

        var parentMerchantFinancialTransaction = true;
        if (merchant.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
        {
            var parentMerchant = await _merchantRepository.GetByIdAsync(merchant.ParentMerchantId);
            if (parentMerchant is not null)
            {
                parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
            }
        }

        var subMerchant = new SubMerchantDto();
        if (request.SubMerchantId.HasValue && request.SubMerchantId.Value != Guid.Empty)
        {
            subMerchant = await _subMerchantService.GetByIdAsync(request.SubMerchantId.Value);
        }

        SetInstallmentNumber(request);

        ValidationResponse validationResponse;

        if (request.IsOnUsPayment == true)
        {
            validationResponse = await _onUsPaymentService.PreValidateOnUsAuth(request, merchant, parentMerchantFinancialTransaction, subMerchant);
        }
        else
        {
            validationResponse = request.PaymentType == VposPaymentType.PostAuth
                ? await PreValidatePostAuth(request, merchant, parentMerchantFinancialTransaction, subMerchant)
                : await PreValidate(request, merchant, parentMerchantFinancialTransaction, subMerchant);
        }

        if (!validationResponse.IsValid)
        {
            _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                             $"Message: {validationResponse.Message}");

            await InsertValidationLogAsync(request, validationResponse);
            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = validationResponse.Code,
                ConversationId = request.ConversationId,
                ErrorMessage = validationResponse.Message,
                OrderId = request.OriginalOrderId,
                Description = validationResponse.Message
            };
        }

        var merchantTransaction = await SaveMerchantTransactionAsync(request, parseUserId, subMerchant);

        //stress testing
        try
        {
            var mockBankEnabled = _vaultClient.GetSecretValue<bool>("PFSecrets", "TransactionSettings", "MockBankTransactionEnabled");

            if (mockBankEnabled)
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();

                Random random = new Random();
                TransactionStatus status = random.Next(2) == 0
                    ? TransactionStatus.Success
                    : TransactionStatus.Fail;

                merchantTransaction.ConversationId = $"TEST_{request.ConversationId}";
                merchantTransaction.TransactionEndDate = DateTime.Now;
                merchantTransaction.ResponseCode = GenericSuccessCode;
                merchantTransaction.ResponseDescription = GenericSuccessCode;
                merchantTransaction.TransactionStatus = status;
                merchantTransaction.LastModifiedBy = parseUserId;
                merchantTransaction.Description = "Test bank transaction";

                dbContext.MerchantTransaction.Update(merchantTransaction);
                await dbContext.SaveChangesAsync();

                await Task.Delay(1000);

                return new ProvisionResponse
                {
                    IsSucceed = status == TransactionStatus.Success,
                    ConversationId = merchantTransaction.ConversationId,
                    ErrorCode = status == TransactionStatus.Success
                        ? GenericSuccessCode
                        : GenericErrorCode,
                    ErrorMessage = status == TransactionStatus.Success
                        ? "Success"
                        : "Fail",
                    OrderId = request.OriginalOrderId,
                    ProvisionNumber = merchantTransaction.ProvisionNumber,
                    Description = merchantTransaction.Description,
                    TransactionId = merchantTransaction.Id
                };
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"MockBankTransactionFail: {exception}");
        }

        if (request.IsOnUsPayment == true)
        {
            return await _onUsPaymentService.InitiateOnUsPaymentAsync(request, merchant, merchantTransaction, parseUserId);
        }

        if (!string.IsNullOrEmpty(request.ThreeDSessionId) && request.PaymentType != VposPaymentType.PostAuth)
        {
            return await ThreeDPaymentAsync(request, merchant, merchantTransaction);
        }

        return request.PaymentType == VposPaymentType.PostAuth
            ? await PostAuthAsync(request, merchant, merchantTransaction, parseUserId)
            : await AuthOrPreAuthAsync(request, merchant, merchantTransaction, parseUserId);
    }

    private async Task InsertValidationLogAsync(ProvisionCommand request, ValidationResponse validationResponse)
    {
        await _apiRequestValidationLogRepository.AddAsync(new MerchantApiValidationLog
        {
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            Amount = request.Amount,
            Currency = request.Currency,
            MerchantId = request.MerchantId,
            CardToken = request.CardToken,
            ConversationId = request.ConversationId,
            InstallmentCount = request.InstallmentCount,
            LanguageCode = request.LanguageCode,
            PointAmount = request.PointAmount,
            TransactionType = request.PaymentType switch
            {
                VposPaymentType.Auth => TransactionType.Auth,
                VposPaymentType.PreAuth => TransactionType.PreAuth,
                _ => TransactionType.PostAuth
            },
            ClientIpAddress = request.ClientIpAddress,
            OriginalReferenceNumber = request.OriginalOrderId,
            ThreeDSessionId = request.ThreeDSessionId,
            ErrorCode = validationResponse.Code ?? "Error Code Not Found!",
            ErrorMessage = validationResponse.Message ?? "Error Message Not Found!"
        });
    }

    private async Task<ValidationResponse> PreValidate(ProvisionCommand request, MerchantDto merchant, bool parentMerchantFinancialTransaction, SubMerchantDto subMerchant)
    {
        if (merchant is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, request.LanguageCode);
        }

        if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, request.LanguageCode);
        }

        if (request.PaymentType == VposPaymentType.PreAuth && !merchant.PreAuthorizationAllowed)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.PreAuthorizationNotAllowed, request.LanguageCode);
        }

        if (request.InstallmentCount > 1 && !merchant.InstallmentAllowed)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InstallmentNotAllowed, request.LanguageCode);
        }

        if (string.IsNullOrEmpty(request.ThreeDSessionId) && merchant.Is3dRequired)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.NonSecureNotAllowed, request.LanguageCode);
        }

        if (request.IntegrationMode == IntegrationMode.ManuelPaymentPage && !merchant.IntegrationMode.ToString().Contains(IntegrationMode.ManuelPaymentPage.ToString()))
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.IntegrationModeNotAllowed, request.LanguageCode);
        }

        if (request.IsInsurancePayment == true && !merchant.InsurancePaymentAllowed)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InsurancePaymentNotAllowed, request.LanguageCode);
        }

        if (request.IsOnUsPayment == true && (request.IsInsurancePayment == true || request.IsTopUpPayment == true) ||
            (request.IsInsurancePayment == true && (request.IsOnUsPayment == true || request.IsTopUpPayment == true)) ||
            (request.IsTopUpPayment == true && (request.IsOnUsPayment == true || request.IsInsurancePayment == true)))
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.MultipleVposTypeNotAllowed, request.LanguageCode);
        }

        if (!string.IsNullOrEmpty(subMerchant.Name))
        {

            if (request.IntegrationMode == IntegrationMode.ManuelPaymentPage && !subMerchant.IsManuelPaymentPageAllowed)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.SubMerchantIntegrationModeNotAllowed, request.LanguageCode);
            }

            if (request.InstallmentCount > 1 && !subMerchant.InstallmentAllowed)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.SubMerchantInstallmentNotAllowed, request.LanguageCode);
            }

            if (string.IsNullOrEmpty(request.ThreeDSessionId) && subMerchant.Is3dRequired)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.SubMerchantNonSecureNotAllowed, request.LanguageCode);
            }

            if (request.PaymentType == VposPaymentType.PreAuth && !subMerchant.PreAuthorizationAllowed)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.PreAuthorizationNotAllowed, request.LanguageCode);
            }

            if (subMerchant.RecordStatus != RecordStatus.Active)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidSubMerchantStatus, request.LanguageCode);
            }
        }

        var currency = await _currencyService.GetByCodeAsync(request.Currency);

        var pricingProfile = _basePaymentService.GetPricingProfileByTransaction(merchant, currency);

        if (pricingProfile is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.PricingProfileItemNotFound, request.LanguageCode);
        }

        var cardToken = await _cardTokenService.GetByToken(request.CardToken);

        if (cardToken is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidToken, request.LanguageCode);
        }

        if (DateTime.Now.CompareTo(cardToken.ExpiryDate) >= 0)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.CardTokenExpired, request.LanguageCode);
        }

        try
        {
            var cardInfo = await _cardTokenService.GetCardDetailsAsync(cardToken);

            var bin = await _binService.GetByNumberAsync(cardInfo.CardNumber);

            if (bin is null && (!merchant.InternationalCardAllowed || (!string.IsNullOrEmpty(subMerchant.Name) && !subMerchant.InternationalCardAllowed)))
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InternationalCardNotAllowed, request.LanguageCode);
            }

            if (bin is not null && request.InstallmentCount > 1 && !await IsMccAllowedAsync(merchant, bin, request.InstallmentCount))
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InstallmentNotAllowed, request.LanguageCode);
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"InvalidCardInfoException: {e}");
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidCardInfo, request.LanguageCode);
        }

        if (!string.IsNullOrEmpty(request.ThreeDSessionId))
        {
            var verification = await _threeDVerificationRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == Guid.Parse(request.ThreeDSessionId) &&
                s.MerchantId == request.MerchantId &&
                s.RecordStatus == RecordStatus.Active &&
                s.CurrentStep == VerificationStep.VerificationFinished);

            if (verification is null)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.SessionNotFound, request.LanguageCode);
            }

            if (verification.Amount != request.Amount)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.ThreeDValidationAmountMismatch, request.LanguageCode);
            }

            if (verification.InstallmentCount != request.InstallmentCount)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidInstallment, request.LanguageCode);
            }

            if (verification.PointAmount != request.PointAmount)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.ThreeDValidationAmountMismatch, request.LanguageCode);
            }

            if (verification.CardToken != request.CardToken)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.ThreeDValidationCardTokenMismatch, request.LanguageCode);
            }

            if (verification.TransactionType.ToString() != request.PaymentType.ToString())
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.ThreeDValidationTransactionTypeMismatch, request.LanguageCode);
            }

            if (verification.SessionExpiryDate <= DateTime.Now)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.ThreeDSessionExpired, request.LanguageCode);
            }

            if (verification.TxnStat != ThreeDHalfSecureStatus && verification.TxnStat != ThreeDFullSecureStatus)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.MdStatusNotSucceeded, request.LanguageCode);
            }

            if (verification.TxnStat == ThreeDHalfSecureStatus && !merchant.HalfSecureAllowed)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.HalfSecureNotAllowed, request.LanguageCode);
            }

            if (currency is not null)
            {
                if (verification.Currency != currency.Number)
                {
                    return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.ThreeDValidationCurrencyMismatch, request.LanguageCode);
                }
            }
        }

        var checkLimit = await _basePaymentService.CheckLimitControlAsync(request, request.PaymentType);

        return !checkLimit.IsValid ? checkLimit : new ValidationResponse { IsValid = true };
    }

    private async Task<bool> IsMccAllowedAsync(MerchantDto merchant, CardBinDto bin, int installmentCount)
    {
        var mcc = await _mccService.GetByCodeAsync(merchant.MccCode);
        var maxAllowedInstallment = bin.CardSubType == CardSubType.Business
            ? mcc.MaxCorporateInstallmentCount
            : mcc.MaxIndividualInstallmentCount;
        return installmentCount <= maxAllowedInstallment;
    }

    private async Task<ValidationResponse> PreValidatePostAuth(ProvisionCommand request, MerchantDto merchant, bool parentMerchantFinancialTransaction, SubMerchantDto subMerchant)
    {
        if (merchant is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, request.LanguageCode);
        }

        if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, request.LanguageCode);
        }

        if (!string.IsNullOrEmpty(subMerchant.Name))
        {

            if (subMerchant.RecordStatus != RecordStatus.Active)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, request.LanguageCode);
            }
        }

        var currency = await _currencyService.GetByCodeAsync(request.Currency);

        var pricingProfile = _basePaymentService.GetPricingProfileByTransaction(merchant, currency);

        if (pricingProfile is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.PricingProfileItemNotFound, request.LanguageCode);
        }

        var checkLimit = await _basePaymentService.CheckLimitControlAsync(request, request.PaymentType);

        if (!checkLimit.IsValid)
        {
            return checkLimit;
        }

        return new ValidationResponse { IsValid = true };
    }
    private static void SetInstallmentNumber(ProvisionCommand request)
    {
        request.InstallmentCount = request.InstallmentCount == 1
            ? 0
            : request.InstallmentCount;
    }

    private async Task<MerchantTransaction> SaveMerchantTransactionAsync(ProvisionCommand request, string parseUserId, SubMerchantDto subMerchant)
    {

        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();

        var user = await dbContext.MerchantUser
                  .Where(s => s.UserId == Guid.Parse(parseUserId))
                  .Select(s => new { FullName = $"{s.Name} {s.Surname}" })
                  .FirstOrDefaultAsync();

        if (user is null)
        {
            user = await dbContext.SubMerchantUser
                  .Where(s => s.UserId == Guid.Parse(parseUserId))
                  .Select(s => new { FullName = $"{s.Name} {s.Surname}" })
                  .FirstOrDefaultAsync();
        }

        var ipAddress = !string.IsNullOrWhiteSpace(request.ClientIpAddress) && request.ClientIpAddress.ToLower() != "null"
                          ? request.ClientIpAddress
                          : _contextProvider.CurrentContext.ClientIpAddress;

        var merchantTransaction = new MerchantTransaction
        {
            RecordStatus = RecordStatus.Active,
            CreateDate = DateTime.Now,
            CreatedBy = parseUserId,
            CreatedNameBy = user is not null ? user.FullName : null,
            IpAddress = ipAddress,
            TransactionStartDate = DateTime.Now,
            TransactionStatus = TransactionStatus.Pending,
            TransactionDate = DateTime.Now.Date,
            TransactionType = request.PaymentType switch
            {
                VposPaymentType.Auth => TransactionType.Auth,
                VposPaymentType.PreAuth => TransactionType.PreAuth,
                _ => TransactionType.PostAuth
            },
            ConversationId = request.ConversationId,
            MerchantId = request.MerchantId,
            IntegrationMode = request.IntegrationMode ?? IntegrationMode.Api,
            IsPreClose = false,
            IsReverse = false,
            IsReturn = false,
            IsManualReturn = false,
            IsOnUsPayment = false,
            IsInsurancePayment = request.IsInsurancePayment ?? false,
            ReturnAmount = 0,
            BankCommissionAmount = 0,
            BankCommissionRate = 0,
            Currency = 0,
            Amount = request.Amount,
            PointAmount = request.PointAmount,
            PointCommissionRate = 0,
            PointCommissionAmount = 0,
            InstallmentCount = request.InstallmentCount,
            ThreeDSessionId = request.ThreeDSessionId,
            Is3ds = !string.IsNullOrEmpty(request.ThreeDSessionId),
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
            CardHolderName = request.CardHolderName,
            MerchantCustomerName = request.MerchantCustomerName != null ? request.MerchantCustomerName : request.CardHolderName,
            MerchantCustomerPhoneNumber = request.MerchantCustomerPhoneNumber,
            MerchantCustomerPhoneCode = request.MerchantCustomerPhoneCode,
            LanguageCode = request.LanguageCode,
            BatchStatus = BatchStatus.Pending,
            OrderId = string.Empty,
            PostingItemId = Guid.Empty,
            BlockageStatus = BlockageStatus.None,
            LastChargebackActivityDate = DateTime.MinValue,
            IsTopUpPayment = request.IsTopUpPayment,
            PfTransactionSource = PfTransactionSource.VirtualPos,
            CardHolderIdentityNumber = request.CardHolderIdentityNumber,
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

    private async Task<ProvisionResponse> PostAuthAsync(ProvisionCommand request, MerchantDto merchant,
        MerchantTransaction merchantTransaction, string parseUserId)
    {
        var bankTransaction = PopulateInitialBankTransaction(request, merchantTransaction);

        PosPaymentProvisionResponse posResponse = null;

        var referenceBankTransaction = await _bankTransactionRepository.GetAll()
            .FirstOrDefaultAsync(s => s.OrderId == request.OriginalOrderId
                                      && s.TransactionType == TransactionType.PreAuth);

        if (referenceBankTransaction is null)
        {
            return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.BankTransactionNotFound, request);
        }

        try
        {
            var referenceMerchantTransaction = await _merchantTransactionRepository.GetAll()
               .FirstOrDefaultAsync(s =>
                   s.RecordStatus == RecordStatus.Active &&
                   s.TransactionStatus == TransactionStatus.Success &&
                   s.IsPreClose == false &&
                   s.MerchantId == merchant.Id &&
                   s.Id == referenceBankTransaction.MerchantTransactionId);

            if (referenceMerchantTransaction is null)
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.MerchantTransactionNotFound, request);
            }

            if (referenceMerchantTransaction.IsReverse || referenceMerchantTransaction.IsReturn)
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.TransactionAlreadyRefunded, request);
            }

            if (referenceMerchantTransaction.Amount < request.Amount && !merchant.IsPostAuthAmountHigherAllowed)
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.InvalidAmount, request);
            }

            var vpos = await _vposRepository.GetAll()
                .Include(b => b.MerchantVposList.Where(a => a.MerchantId == merchant.Id && a.TerminalStatus == TerminalStatus.Active))
                .Include(s => s.AcquireBank)
                .Include(s => s.VposBankApiInfos)
                .ThenInclude(s => s.Key)
                .FirstOrDefaultAsync(s => s.Id == referenceBankTransaction.VposId);

            if (vpos is null)
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.VposNotFound, request);
            }

            var acquirerBank = await _acquireBankRepository
                .GetAll()
                .FirstOrDefaultAsync(s => s.Id == vpos.AcquireBankId);

            if (acquirerBank is null)
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.AcquireBankNotFound, request);
            }

            var bin = await _binService.GetByNumberAsync(referenceMerchantTransaction.BinNumber);
            var ipAddress = !string.IsNullOrWhiteSpace(request.ClientIpAddress) && request.ClientIpAddress.ToLower() != "null"
                           ? request.ClientIpAddress
                           : _contextProvider.CurrentContext.ClientIpAddress;

            // todo: kanallari duzenle hpp-link eklenmeli
            if (!await _fraudService.CheckFraudAsync(new FraudTransactionDetail
            {
                Amount = request.Amount,
                BeneficiaryNumber = merchant.Number,
                Beneficiary = merchant.Name,
                BeneficiaryBankID = referenceBankTransaction.AcquireBankCode.ToString(),
                OriginatorNumber = referenceBankTransaction.CardNumber,
                Originator = request.CardHolderName,
                OriginatorBankID = bin?.BankCode.ToString() ?? string.Empty,
                FraudSource = FraudSource.Pos,
                Direction = Direction.Outbound,
                AmountCurrencyCode = referenceBankTransaction.Currency,
                BeneficiaryAccountCurrencyCode = referenceBankTransaction.Currency,
                OriginatorAccountCurrencyCode = referenceBankTransaction.Currency,
                Channel = _contextProvider.CurrentContext.Channel,
                TransactionType = request.PaymentType.ToString(),
                MccCode = Convert.ToInt32(merchant.MccCode)
            }, "PfProvision", ipAddress))
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.PotentialFraud, request);
            }

            var currency = await _currencyService.GetByNumberAsync(referenceMerchantTransaction.Currency);

            var pricingProfile = _basePaymentService.GetPricingProfileByTransaction(merchant, currency);

            var pricingProfileItem = _basePaymentService.GetPricingProfileItemByTransaction(pricingProfile, merchantTransaction);

            if (pricingProfileItem is null)
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.PricingProfileItemNotFound, request);
            }

            var routeInfo = await _posRouterService.RouteAsync(bin,
                merchant, currency.Code, request.InstallmentCount, request.Amount, referenceBankTransaction.VposId, request.IsInsurancePayment ?? false, request.IsTopUpPayment ?? false);

            bankTransaction.OrderId = await _orderNumberGeneratorService.GenerateForBankTransactionAsync(acquirerBank.BankCode, merchant.Number);
            bankTransaction.Currency = referenceBankTransaction.Currency;
            bankTransaction.CardNumber = referenceBankTransaction.CardNumber;
            bankTransaction.VposId = referenceBankTransaction.VposId;
            bankTransaction.MerchantCode = referenceBankTransaction.MerchantCode;
            bankTransaction.SubMerchantCode = referenceBankTransaction.SubMerchantCode;
            bankTransaction.IssuerBankCode = referenceBankTransaction.IssuerBankCode;
            bankTransaction.AcquireBankCode = referenceBankTransaction.AcquireBankCode;

            merchantTransaction.Currency = referenceMerchantTransaction.Currency;
            merchantTransaction.InstallmentCount = request.InstallmentCount;
            merchantTransaction.ThreeDSessionId = request.ThreeDSessionId;
            merchantTransaction.Is3ds = !string.IsNullOrEmpty(request.ThreeDSessionId);
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
            merchantTransaction.CardTransactionType = referenceMerchantTransaction.CardTransactionType;
            merchantTransaction.CardType = referenceMerchantTransaction.CardType;
            merchantTransaction.OrderId = bankTransaction.OrderId;
            merchantTransaction.LastModifiedBy = parseUserId;
            merchantTransaction.BankCommissionAmount = routeInfo.CommissionAmount;
            merchantTransaction.BankCommissionRate = routeInfo.CommissionRate;
            merchantTransaction.MerchantCustomerName = referenceMerchantTransaction.MerchantCustomerName;
            merchantTransaction.MerchantCustomerPhoneNumber = referenceMerchantTransaction.MerchantCustomerPhoneNumber;
            merchantTransaction.Description = referenceMerchantTransaction.Description;
            merchantTransaction.CardHolderName = referenceMerchantTransaction.CardHolderName;
            merchantTransaction.PfCommissionAmount = pricingProfile.PerTransactionFee + pricingProfileItem.CommissionRate / 100m * merchantTransaction.Amount;
            merchantTransaction.PfNetCommissionAmount = merchantTransaction.PfCommissionAmount - merchantTransaction.BankCommissionAmount;
            merchantTransaction.PfCommissionRate = pricingProfileItem.CommissionRate;
            merchantTransaction.PfPerTransactionFee = pricingProfile.PerTransactionFee;
            merchantTransaction.ParentMerchantCommissionRate = pricingProfileItem.ParentMerchantCommissionRate;
            merchantTransaction.ParentMerchantCommissionAmount = pricingProfileItem.ParentMerchantCommissionRate / 100m * merchantTransaction.Amount;
            merchantTransaction.AmountWithoutCommissions = merchantTransaction.Amount - merchantTransaction.PfCommissionAmount - merchantTransaction.ParentMerchantCommissionAmount;
            merchantTransaction.AmountWithoutBankCommission = merchantTransaction.Amount - merchantTransaction.BankCommissionAmount;
            merchantTransaction.AmountWithoutParentMerchantCommission = merchantTransaction.Amount - merchantTransaction.ParentMerchantCommissionAmount;
            merchantTransaction.PricingProfileItemId = pricingProfileItem.Id;
            merchantTransaction.BsmvAmount = await BsmvAmountCalculateHelper.CalculateBsmvAmount(merchantTransaction.PfNetCommissionAmount, _parameterService);
            merchantTransaction.PointCommissionRate = GetPointCommissionRate(merchantTransaction.CardTransactionType, routeInfo.Vpos);
            merchantTransaction.PointCommissionAmount = (merchantTransaction.Amount * merchantTransaction.PointCommissionRate) / 100m;
            merchantTransaction.ServiceCommissionRate = GetServiceCommissionRate(merchantTransaction.CardTransactionType, routeInfo.Vpos);
            merchantTransaction.ServiceCommissionAmount = (merchantTransaction.Amount * merchantTransaction.ServiceCommissionRate) / 100m;
            merchantTransaction.IsPerInstallment = routeInfo.ProfileSettlementMode == ProfileSettlementMode.PerInstallment ? true : false;

            int bankBlockedDayNumber = (int)(referenceMerchantTransaction.BankPaymentDate.Date - referenceMerchantTransaction.TransactionDate.Date).Days;
            merchantTransaction.BankPaymentDate = DateTime.Now.AddDays(bankBlockedDayNumber);
            merchantTransaction.PfPaymentDate = await _basePaymentService.CalculatePaymentDateAsync(DateTime.Now, pricingProfileItem.BlockedDayNumber);

            var transactionStatus = (referenceMerchantTransaction.Amount > merchantTransaction.Amount)
                                    ? TransactionStatus.PartiallyClosed
                                    : TransactionStatus.Closed;

            if (transactionStatus is TransactionStatus.PartiallyClosed)
            {
                merchantTransaction.BankCommissionAmount = (merchantTransaction.BankCommissionRate / 100m) * request.Amount;
                merchantTransaction.PointCommissionAmount = (merchantTransaction.PointCommissionRate / 100m) * request.Amount;
                merchantTransaction.ServiceCommissionAmount = (merchantTransaction.ServiceCommissionRate / 100m) * request.Amount;
            }

            var bankService = _vposServiceFactory.GetVposServices(vpos, merchant.Id, referenceMerchantTransaction.IsInsurancePayment);

            var subMerchantInfo = await GetSubMerchant(merchant.Id);

            try
            {
                posResponse = await bankService.PostAuth(new PosPostAuthRequest
                {
                    Amount = request.Amount,
                    PreAuthAmount = referenceMerchantTransaction.Amount,
                    Currency = referenceMerchantTransaction.Currency,
                    CurrencyCode = currency.Code,
                    OrderNumber = bankTransaction.OrderId,
                    OrgOrderNumber = referenceBankTransaction.OrderId,
                    CardBrand = bin!.CardBrand,
                    LanguageCode = request.LanguageCode,
                    SubMerchantCode = referenceBankTransaction.SubMerchantCode,
                    ClientIp = ipAddress,
                    SubMerchantId = subMerchantInfo.Number,
                    SubMerchantName = subMerchantInfo.Name,
                    SubMerchantCity = subMerchantInfo.Customer.CityName,
                    SubMerchantDistrict = subMerchantInfo.Customer.DistrictName,
                    SubMerchantAddress = subMerchantInfo.Customer.Address,
                    SubMerchantTaxNumber = subMerchantInfo.Customer.TaxNumber,
                    SubMerchantCountry = subMerchantInfo.Customer.Country.ToString(),
                    SubMerchantMcc = subMerchantInfo.MccCode,
                    SubMerchantUrl = subMerchantInfo.WebSiteUrl,
                    SubMerchantGlobalMerchantId = subMerchantInfo.GlobalMerchantId,
                    SubMerchantPostalCode = subMerchantInfo.Customer.PostalCode,
                    BankOrderId = referenceBankTransaction.BankOrderId,
                    ProvisionNumber = referenceBankTransaction.ApprovalCode,
                    RRN = referenceBankTransaction.RrnNumber,
                    Stan = referenceBankTransaction.Stan,
                    Installment = request.InstallmentCount,
                    OrderDate = referenceMerchantTransaction.TransactionDate,
                    IsBlockaged = vpos.VposType == VposType.Blockage ? true : false,
                    BlockageCode = vpos.BlockageCode,
                    ServiceProviderPspMerchantId = vpos.MerchantVposList.FirstOrDefault()?.ServiceProviderPspMerchantId
                });
            }
            catch (TaskCanceledException exception)
                when (exception.InnerException is TimeoutException)
            {
                await _basePaymentService.InsertTimeoutTransactionAsync(merchantTransaction, bankTransaction, ipAddress, referenceBankTransaction.OrderId);

                var timeoutExceptionCode = await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.BankHasTimedOut, request.LanguageCode);

                try
                {
                    await UpdateHealthCheckAsync(merchantTransaction.Id);
                }
                catch (Exception exceptions)
                {
                    _logger.LogError($"UpdateHealthCheckException: {exceptions}");
                }

                return new ProvisionResponse
                {
                    IsSucceed = false,
                    ErrorCode = timeoutExceptionCode.Code,
                    ErrorMessage = timeoutExceptionCode.Message,
                    OrderId = string.Empty,
                    ProvisionNumber = merchantTransaction.ProvisionNumber,
                    Description = merchantTransaction.Description,
                    ConversationId = request.ConversationId,
                    TransactionId = merchantTransaction.Id
                };
            }

            if (posResponse.IsSuccess && merchantTransaction.IsPerInstallment == true)
            {
                await CalculateInstallmentTransactions(merchantTransaction, pricingProfile, pricingProfileItem, vpos, routeInfo, posResponse.AuthCode, referenceMerchantTransaction.Id);

            }

            var provisionResponse = posResponse.IsSuccess
                ? await MarkAsCompletedAsync(merchantTransaction, bankTransaction, posResponse,
                    referenceMerchantTransaction, referenceBankTransaction)
                : await MarkAsFailedAsync(merchantTransaction, bankTransaction, posResponse, ipAddress, referenceBankTransaction);

            provisionResponse.TransactionId = merchantTransaction.Id;

            return provisionResponse;
        }
        catch (Exception exception)
        {
            _logger.LogError($"PostAuthAsyncException: {exception}");

            if (posResponse is not null)
            {
                return await RetryDbUpdateAsync(merchantTransaction, posResponse, bankTransaction,
                    request.ConversationId,
                    request.ClientIpAddress,
                    null,
                    referenceBankTransaction);
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

            try
            {
                await UpdateHealthCheckAsync(merchantTransaction.Id);
            }
            catch (Exception exceptions)
            {
                _logger.LogError($"UpdateHealthCheckException: {exceptions}");
            }

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                OrderId = string.Empty,
                ProvisionNumber = merchantTransaction.ProvisionNumber,
                Description = merchantTransaction.Description,
                ConversationId = request.ConversationId,
                TransactionId = merchantTransaction.Id
            };
        }
    }
    private async Task<ProvisionResponse> AuthOrPreAuthAsync(ProvisionCommand request,
        MerchantDto merchant,
        MerchantTransaction merchantTransaction,
        string parseUserId)
    {

        var bankTransaction = PopulateInitialBankTransaction(request, merchantTransaction);

        PosPaymentProvisionResponse posResponse = null;

        try
        {
            var cardToken = await _cardTokenService.GetByToken(request.CardToken);
            var card = await GetCardDetailsAsync(cardToken);
            var bin = await _binService.GetByNumberAsync(card.CardNumber);
            var loyaltyExceptions = bin is not null ? await _cardLoyaltyExceptionRepository.GetAll()
                .Where(s => s.CounterBankCode == bin.BankCode && s.RecordStatus == RecordStatus.Active)
                .ToListAsync() : [];

            RouteResponse routeInfo;
            var shortCircuitVposId = await _posRouterService.CheckRouteForShortCircuitAsync(
                merchant.Id, card.CardNumber[..8], request.Currency, request.InstallmentCount, request.Amount, request.IsInsurancePayment);

            if (shortCircuitVposId != Guid.Empty)
            {
                try
                {
                    routeInfo = await _posRouterService.RouteAsync(bin, merchant, request.Currency, request.InstallmentCount,
                        request.Amount, shortCircuitVposId, request.IsInsurancePayment ?? false, request.IsTopUpPayment ?? false);
                }
                catch (Exception exception)
                {
                    _logger.LogError($"Short circuit vpos route exception: {exception}");
                    routeInfo = await _posRouterService.RouteAsync(bin, merchant, request.Currency, request.InstallmentCount,
                        request.Amount, Guid.Empty, request.IsInsurancePayment ?? false, request.IsTopUpPayment ?? false);
                }
            }
            else
            {
                routeInfo = await _posRouterService.RouteAsync(bin, merchant, request.Currency, request.InstallmentCount,
                    request.Amount, Guid.Empty, request.IsInsurancePayment ?? false, request.IsTopUpPayment ?? false);
            }

            if (!routeInfo.Vpos.SecurityType.HasFlag(SecurityType.NonSecure))
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.NonSecureNotAllowed, request);
            }

            var merchantCode = await GetMerchantCode(routeInfo.AcquireBank.Id, routeInfo.Vpos.Id);
            var subMerchant = await GetSubMerchantCode(routeInfo.Vpos.Id, merchant.Id);
            var currency = await _currencyService.GetByCodeAsync(request.Currency);
            var ipAddress = !string.IsNullOrWhiteSpace(request.ClientIpAddress) && request.ClientIpAddress.ToLower() != "null"
                           ? request.ClientIpAddress
                           : _contextProvider.CurrentContext.ClientIpAddress;

            if (!await _fraudService.CheckFraudAsync(new FraudTransactionDetail
            {
                Amount = request.Amount,
                BeneficiaryNumber = merchant.Number,
                Beneficiary = merchant.Name,
                BeneficiaryBankID = routeInfo.AcquireBank.BankCode.ToString(),
                OriginatorNumber = CardHelper.GetMaskedCardNumber(card.CardNumber),
                Originator = request.CardHolderName,
                OriginatorBankID = bin?.BankCode.ToString() ?? string.Empty,
                FraudSource = FraudSource.Pos,
                Direction = Direction.Outbound,
                AmountCurrencyCode = currency.Number,
                BeneficiaryAccountCurrencyCode = currency.Number,
                OriginatorAccountCurrencyCode = currency.Number,
                Channel = _contextProvider.CurrentContext.Channel,
                TransactionType = request.PaymentType.ToString(),
                MccCode = Convert.ToInt32(merchant.MccCode)
            }, "PfProvision", ipAddress))
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.PotentialFraud, request);
            }

            bankTransaction.OrderId = await _orderNumberGeneratorService.GenerateForBankTransactionAsync(routeInfo.AcquireBank.BankCode, merchant.Number);
            bankTransaction.Currency = currency.Number;
            bankTransaction.CardNumber = CardHelper.GetMaskedCardNumber(card.CardNumber);
            bankTransaction.VposId = routeInfo.Vpos.Id;
            bankTransaction.MerchantCode = merchantCode;
            bankTransaction.SubMerchantCode = subMerchant.SubMerchantCode;
            bankTransaction.IssuerBankCode = bin?.BankCode ?? 0;
            bankTransaction.AcquireBankCode = routeInfo.AcquireBank.BankCode;

            merchantTransaction.Currency = currency.Number;
            merchantTransaction.InstallmentCount = request.InstallmentCount;
            merchantTransaction.ThreeDSessionId = request.ThreeDSessionId;
            merchantTransaction.Is3ds = !string.IsNullOrEmpty(request.ThreeDSessionId);
            merchantTransaction.CardNumber = CardHelper.GetMaskedCardNumber(card.CardNumber);
            merchantTransaction.BinNumber = string.IsNullOrEmpty(bin?.BinNumber) == true ? "0" : bin.BinNumber;
            merchantTransaction.HasCvv = !string.IsNullOrEmpty(card.Cvv);
            merchantTransaction.HasExpiryDate = !string.IsNullOrEmpty(card.ExpireMonth);
            merchantTransaction.IsAmex = bin is not null && bin.CardBrand == CardBrand.Amex;
            merchantTransaction.IsInternational = bin == null;
            merchantTransaction.VposId = routeInfo.Vpos.Id;
            merchantTransaction.VposName = routeInfo.Vpos.Name;
            merchantTransaction.IssuerBankCode = bin?.BankCode ?? 0;
            merchantTransaction.AcquireBankCode = routeInfo.AcquireBank.BankCode;
            merchantTransaction.BankPaymentDate = DateTime.Now.AddDays(routeInfo.BlockedDayNumber + 1);
            merchantTransaction.CardTransactionType = CardHelper.GetCardTransactionType(routeInfo.AcquireBank, bin, loyaltyExceptions);
            merchantTransaction.CardType = bin?.CardType ?? CardType.Unknown;
            merchantTransaction.OrderId = bankTransaction.OrderId;
            merchantTransaction.LastModifiedBy = parseUserId;
            merchantTransaction.Description = request.Description;
            merchantTransaction.IsPerInstallment = routeInfo.ProfileSettlementMode == ProfileSettlementMode.PerInstallment ? true : false;


            var pricingProfile = new PricingProfile();
            var pricingProfileItem = new PricingProfileItem();

            if (request.PaymentType != VposPaymentType.PreAuth)
            {
                pricingProfile = _basePaymentService.GetPricingProfileByTransaction(merchant, currency);

                pricingProfileItem = _basePaymentService.GetPricingProfileItemByTransaction(pricingProfile, merchantTransaction);

                if (pricingProfileItem is null)
                {
                    return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.PricingProfileItemNotFound, request);
                }
                merchantTransaction.BankCommissionAmount = routeInfo.CommissionAmount;
                merchantTransaction.BankCommissionRate = routeInfo.CommissionRate;
                merchantTransaction.PfCommissionAmount = pricingProfile.PerTransactionFee + pricingProfileItem.CommissionRate / 100m * merchantTransaction.Amount;
                merchantTransaction.ParentMerchantCommissionRate = pricingProfileItem.ParentMerchantCommissionRate;
                merchantTransaction.ParentMerchantCommissionAmount = pricingProfileItem.ParentMerchantCommissionRate / 100m * merchantTransaction.Amount;
                merchantTransaction.PfNetCommissionAmount = merchantTransaction.PfCommissionAmount - merchantTransaction.BankCommissionAmount;
                merchantTransaction.PfCommissionRate = pricingProfileItem.CommissionRate;
                merchantTransaction.PfPerTransactionFee = pricingProfile.PerTransactionFee;
                merchantTransaction.AmountWithoutCommissions = merchantTransaction.Amount - merchantTransaction.PfCommissionAmount - merchantTransaction.ParentMerchantCommissionAmount;
                merchantTransaction.AmountWithoutBankCommission = merchantTransaction.Amount - merchantTransaction.BankCommissionAmount;
                merchantTransaction.AmountWithoutParentMerchantCommission = merchantTransaction.Amount - merchantTransaction.ParentMerchantCommissionAmount;
                merchantTransaction.PricingProfileItemId = pricingProfileItem.Id;
                merchantTransaction.BsmvAmount = await BsmvAmountCalculateHelper.CalculateBsmvAmount(merchantTransaction.PfNetCommissionAmount, _parameterService);
                merchantTransaction.PfPaymentDate = await _basePaymentService.CalculatePaymentDateAsync(merchantTransaction.TransactionDate, pricingProfileItem.BlockedDayNumber);
                merchantTransaction.PointCommissionRate = GetPointCommissionRate(merchantTransaction.CardTransactionType, routeInfo.Vpos);
                merchantTransaction.PointCommissionAmount = (merchantTransaction.Amount * merchantTransaction.PointCommissionRate) / 100m;
                merchantTransaction.ServiceCommissionRate = GetServiceCommissionRate(merchantTransaction.CardTransactionType, routeInfo.Vpos);
                merchantTransaction.ServiceCommissionAmount = (merchantTransaction.Amount * merchantTransaction.ServiceCommissionRate) / 100m;
            }
            var subMerchantInfo = await GetSubMerchant(merchant.Id);

            var bankService = _vposServiceFactory.GetVposServices(routeInfo.Vpos, merchant.Id, request.IsInsurancePayment);

            try
            {
                posResponse = await bankService.PaymentNonSecure(new PosPaymentNonSecureRequest
                {
                    Amount = request.Amount,
                    BonusAmount = request.PointAmount,
                    AuthType = request.PaymentType == VposPaymentType.Auth
                        ? VposAuthType.Auth
                        : VposAuthType.PreAuth,
                    CardNumber = card.CardNumber,
                    CardBrand = bin?.CardBrand ?? CardBrand.Undefined,
                    Cvv2 = card.Cvv,
                    ExpireMonth = card.ExpireMonth,
                    ExpireYear = card.ExpireYear,
                    Installment = request.InstallmentCount,
                    OrderNumber = bankTransaction.OrderId,
                    Currency = currency.Number,
                    CurrencyCode = currency.Code,
                    IsBlockaged = routeInfo.Vpos.VposType == VposType.Blockage ? true : false,
                    BlockageCode = routeInfo.Vpos.BlockageCode,
                    LanguageCode = request.LanguageCode,
                    SubMerchantCode = subMerchant.SubMerchantCode,
                    SubMerchantId = subMerchantInfo.Number,
                    SubMerchantName = subMerchantInfo.Name,
                    SubMerchantCity = subMerchantInfo.Customer?.CityName,
                    SubMerchantAddress = subMerchantInfo.Customer?.Address,
                    SubMerchantDistrict = subMerchantInfo.Customer?.DistrictName,
                    SubMerchantTaxNumber = subMerchantInfo.Customer?.TaxNumber,
                    SubMerchantCountry = subMerchantInfo.Customer?.Country.ToString(),
                    SubMerchantMcc = subMerchantInfo.MccCode,
                    SubMerchantUrl = subMerchantInfo.WebSiteUrl,
                    SubMerchantGlobalMerchantId = subMerchantInfo.GlobalMerchantId,
                    SubMerchantPostalCode = subMerchantInfo.Customer?.PostalCode,
                    SubMerchantTerminalNo = subMerchant.TerminalNo,
                    ClientIp = ipAddress,
                    CardHolderName = merchantTransaction.CardHolderName,
                    ServiceProviderPspMerchantId = subMerchant.ServiceProviderPspMerchantId,
                    CardHolderIdentityNumber = request.CardHolderIdentityNumber
                });
            }
            catch (TaskCanceledException exception)
                when (exception.InnerException is TimeoutException)
            {
                await _basePaymentService.InsertTimeoutTransactionAsync(merchantTransaction, bankTransaction, ipAddress);

                var timeoutExceptionCode = await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.BankHasTimedOut, request.LanguageCode);

                try
                {
                    await UpdateHealthCheckAsync(merchantTransaction.Id);
                }
                catch (Exception exceptions)
                {
                    _logger.LogError($"UpdateHealthCheckException: {exceptions}");
                }

                return new ProvisionResponse
                {
                    IsSucceed = false,
                    ErrorCode = timeoutExceptionCode.Code,
                    ErrorMessage = timeoutExceptionCode.Message,
                    OrderId = string.Empty,
                    ProvisionNumber = merchantTransaction.ProvisionNumber,
                    Description = merchantTransaction.Description,
                    ConversationId = request.ConversationId,
                    TransactionId = merchantTransaction.Id
                };
            }

            if (posResponse.IsSuccess && merchantTransaction.IsPerInstallment == true)
            {
                await CalculateInstallmentTransactions(merchantTransaction, pricingProfile, pricingProfileItem, routeInfo.Vpos, routeInfo, posResponse.AuthCode, Guid.Empty);
            }

            return posResponse.IsSuccess
                ? await MarkAsCompletedAsync(merchantTransaction, bankTransaction, posResponse, token: cardToken)
                : await MarkAsFailedAsync(merchantTransaction, bankTransaction, posResponse, ipAddress);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Provision Error - {exception}");

            if (posResponse is not null)
            {
                return await RetryDbUpdateAsync(merchantTransaction, posResponse, bankTransaction,
                    request.ConversationId, request.ClientIpAddress);
            }

            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();

            merchantTransaction.ResponseCode =
                (exception is ApiException apiException)
                    ? apiException.Code
                    : GenericErrorCode;
            merchantTransaction.ResponseDescription = $"{exception.GetType().Name} - {exception.Message}";
            merchantTransaction.TransactionStatus = TransactionStatus.Fail;
            merchantTransaction.TransactionEndDate = DateTime.Now;
            dbContext.Update(merchantTransaction);

            bankTransaction.TransactionEndDate = DateTime.Now;
            bankTransaction.BankResponseCode = string.Empty;
            bankTransaction.BankResponseDescription = string.Empty;
            bankTransaction.TransactionStatus = TransactionStatus.Fail;
            await dbContext.AddAsync(bankTransaction);

            await dbContext.SaveChangesAsync();

            try
            {
                await UpdateHealthCheckAsync(merchantTransaction.Id);
            }
            catch (Exception exceptions)
            {
                _logger.LogError($"UpdateHealthCheckException: {exceptions}");
            }

            var apiResponse = await _errorCodeService.GetApiResponseCode(merchantTransaction.ResponseCode, request.LanguageCode);

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = merchantTransaction.ResponseCode,
                ErrorMessage = apiResponse.DisplayMessage,
                OrderId = string.Empty,
                ProvisionNumber = merchantTransaction.ProvisionNumber,
                Description = merchantTransaction.Description,
                ConversationId = request.ConversationId,
                TransactionId = merchantTransaction.Id
            };
        }
    }
    private async Task<ProvisionResponse> ThreeDPaymentAsync(ProvisionCommand request,
        MerchantDto merchant,
        MerchantTransaction merchantTransaction)
    {
        var bankTransaction = PopulateInitialBankTransaction(request, merchantTransaction);
        ThreeDVerification verification = null;
        PosPaymentProvisionResponse posResponse = null;

        try
        {
            verification = await _threeDVerificationRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == Guid.Parse(request.ThreeDSessionId) &&
                s.MerchantId == request.MerchantId &&
                s.RecordStatus == RecordStatus.Active &&
                s.CurrentStep == VerificationStep.VerificationFinished);

            var cardToken = await _cardTokenService.GetByToken(request.CardToken);
            var card = await GetCardDetailsAsync(cardToken);
            var bin = await _binService.GetByNumberAsync(card.CardNumber);
            var loyaltyExceptions = bin is not null ? await _cardLoyaltyExceptionRepository.GetAll()
                .Where(s => s.CounterBankCode == bin.BankCode && s.RecordStatus == RecordStatus.Active)
                .ToListAsync() : [];
            var vpos = await _vposRepository.GetAll()
                .Include(b => b.MerchantVposList.Where(a => a.MerchantId == merchant.Id && a.TerminalStatus == TerminalStatus.Active))
                .Include(s => s.AcquireBank)
                .Include(s => s.VposBankApiInfos)
                .ThenInclude(s => s.Key)
                .Include(s => s.CostProfiles)
                .FirstOrDefaultAsync(s => s.Id == verification.VposId);

            if (vpos is null)
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.VposNotFound, request);
            }

            var requiredSecurityType = GetSecurityType(verification);

            bool isAllowed =
             requiredSecurityType switch
             {
                 SecurityType.FullSecure =>
                     vpos.SecurityType.HasFlag(SecurityType.FullSecure),

                 SecurityType.HalfSecure =>
                     vpos.SecurityType.HasFlag(SecurityType.HalfSecure) ||
                     vpos.SecurityType.HasFlag(SecurityType.FullSecure),

                 _ => false
             };

            if (!isAllowed)
            {
                return new ProvisionResponse
                {
                    IsSucceed = false,
                    ErrorCode = "99",
                    ErrorMessage = "Vpos security type is invalid!",
                    OrderId = string.Empty,
                    ConversationId = request.ConversationId,
                    TransactionId = merchantTransaction.Id,
                    ProvisionNumber = merchantTransaction.ProvisionNumber,
                    Description = merchantTransaction.Description
                };
            }

            var ipAddress = !string.IsNullOrWhiteSpace(request.ClientIpAddress) && request.ClientIpAddress.ToLower() != "null"
                           ? request.ClientIpAddress
                           : _contextProvider.CurrentContext.ClientIpAddress;

            if (!await _fraudService.CheckFraudAsync(new FraudTransactionDetail
            {
                Amount = request.Amount,
                BeneficiaryNumber = merchant.Number,
                Beneficiary = merchant.Name,
                BeneficiaryBankID = vpos.AcquireBank.BankCode.ToString(),
                OriginatorNumber = CardHelper.GetMaskedCardNumber(card.CardNumber),
                Originator = request.CardHolderName,
                OriginatorBankID = bin?.BankCode.ToString() ?? string.Empty,
                FraudSource = FraudSource.Pos,
                Direction = Direction.Outbound,
                AmountCurrencyCode = verification.Currency,
                BeneficiaryAccountCurrencyCode = verification.Currency,
                OriginatorAccountCurrencyCode = verification.Currency,
                Channel = _contextProvider.CurrentContext.Channel,
                TransactionType = request.PaymentType.ToString(),
                MccCode = Convert.ToInt32(merchant.MccCode)
            }, "PfProvision", ipAddress))
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.PotentialFraud, request);
            }

            bankTransaction.OrderId = verification.OrderId;
            bankTransaction.Currency = verification.Currency;
            bankTransaction.CardNumber = CardHelper.GetMaskedCardNumber(card.CardNumber);
            bankTransaction.VposId = verification.VposId;
            bankTransaction.MerchantCode = verification.MerchantCode;
            bankTransaction.SubMerchantCode = verification.SubMerchantCode;
            bankTransaction.IssuerBankCode = verification.IssuerBankCode;
            bankTransaction.AcquireBankCode = vpos.AcquireBank.BankCode;

            merchantTransaction.Currency = verification.Currency;
            merchantTransaction.InstallmentCount = request.InstallmentCount;
            merchantTransaction.ThreeDSessionId = request.ThreeDSessionId;
            merchantTransaction.Is3ds = !string.IsNullOrEmpty(request.ThreeDSessionId);
            merchantTransaction.CardNumber = CardHelper.GetMaskedCardNumber(card.CardNumber);
            merchantTransaction.BinNumber = CardHelper.GetBinNumber(card.CardNumber);
            merchantTransaction.HasCvv = !string.IsNullOrEmpty(card.Cvv);
            merchantTransaction.HasExpiryDate = !string.IsNullOrEmpty(card.ExpireMonth);
            merchantTransaction.IsAmex = bin is not null && bin.CardBrand == CardBrand.Amex;
            merchantTransaction.IsInternational = bin == null;
            merchantTransaction.VposId = verification.VposId;
            merchantTransaction.VposName = vpos.Name;
            merchantTransaction.IssuerBankCode = verification.IssuerBankCode;
            merchantTransaction.AcquireBankCode = verification.AcquireBankCode;
            merchantTransaction.BankCommissionAmount = verification.BankCommissionAmount;
            merchantTransaction.BankCommissionRate = verification.BankCommissionRate;
            merchantTransaction.BankPaymentDate = DateTime.Now.AddDays(verification.BankBlockedDayNumber + 1);
            merchantTransaction.CardType = bin?.CardType ?? CardType.Unknown;
            merchantTransaction.CardTransactionType = CardHelper.GetCardTransactionType(vpos.AcquireBank, bin, loyaltyExceptions);
            merchantTransaction.OrderId = bankTransaction.OrderId;
            merchantTransaction.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();

            var currency = await _currencyService.GetByNumberAsync(verification.Currency);

            var pricingProfile = _basePaymentService.GetPricingProfileByTransaction(merchant, currency);

            var pricingProfileItem = _basePaymentService.GetPricingProfileItemByTransaction(pricingProfile, merchantTransaction);

            if (pricingProfileItem is null)
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.PricingProfileItemNotFound, request);
            }

            merchantTransaction.PfCommissionAmount = pricingProfile.PerTransactionFee + pricingProfileItem.CommissionRate / 100m * merchantTransaction.Amount;
            merchantTransaction.PfNetCommissionAmount = merchantTransaction.PfCommissionAmount - merchantTransaction.BankCommissionAmount;
            merchantTransaction.PfCommissionRate = pricingProfileItem.CommissionRate;
            merchantTransaction.PfPerTransactionFee = pricingProfile.PerTransactionFee;
            merchantTransaction.ParentMerchantCommissionAmount = pricingProfileItem.ParentMerchantCommissionRate / 100m * merchantTransaction.Amount;
            merchantTransaction.ParentMerchantCommissionRate = pricingProfileItem.ParentMerchantCommissionRate;
            merchantTransaction.AmountWithoutCommissions = merchantTransaction.Amount - merchantTransaction.PfCommissionAmount - merchantTransaction.ParentMerchantCommissionAmount;
            merchantTransaction.AmountWithoutBankCommission = merchantTransaction.Amount - merchantTransaction.BankCommissionAmount;
            merchantTransaction.AmountWithoutParentMerchantCommission = merchantTransaction.Amount - merchantTransaction.ParentMerchantCommissionAmount;
            merchantTransaction.PricingProfileItemId = pricingProfileItem.Id;
            merchantTransaction.BsmvAmount = await BsmvAmountCalculateHelper.CalculateBsmvAmount(merchantTransaction.PfNetCommissionAmount, _parameterService);
            merchantTransaction.PfPaymentDate = await _basePaymentService.CalculatePaymentDateAsync(merchantTransaction.TransactionDate, pricingProfileItem.BlockedDayNumber);
            merchantTransaction.PointCommissionRate = GetPointCommissionRate(merchantTransaction.CardTransactionType, vpos);
            merchantTransaction.PointCommissionAmount = (merchantTransaction.Amount * merchantTransaction.PointCommissionRate) / 100m;
            merchantTransaction.ServiceCommissionRate = GetServiceCommissionRate(merchantTransaction.CardTransactionType, vpos);
            merchantTransaction.ServiceCommissionAmount = (merchantTransaction.Amount * merchantTransaction.ServiceCommissionRate) / 100m;
            merchantTransaction.IsPerInstallment = verification.IsPerInstallment;

            var bankService = _vposServiceFactory.GetVposServices(vpos, merchant.Id, request.IsInsurancePayment);

            var subMerchantInfo = await GetSubMerchant(merchant.Id);

            if (request.IsTopUpPayment == true)
            {
                var vaultMerchantId = _vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "MerchantId");

                if (Guid.Parse(vaultMerchantId) != merchant.Id)
                {
                    var topUpExceptionCode = await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, request.LanguageCode);

                    return new ProvisionResponse
                    {
                        IsSucceed = false,
                        ErrorCode = topUpExceptionCode.Code,
                        ErrorMessage = topUpExceptionCode.Message,
                        OrderId = string.Empty,
                        ConversationId = request.ConversationId,
                        TransactionId = merchantTransaction.Id,
                        ProvisionNumber = merchantTransaction.ProvisionNumber,
                        Description = merchantTransaction.Description
                    };
                }
            }

            try
            {
                posResponse = await bankService.Payment3DModel(
                    new PosPayment3DModelRequest
                    {
                        Amount = request.Amount,
                        BonusAmount = request.PointAmount,
                        CardNumber = card.CardNumber,
                        ExpireMonth = card.ExpireMonth,
                        ExpireYear = card.ExpireYear,
                        AuthType = request.PaymentType == VposPaymentType.Auth
                            ? VposAuthType.Auth
                            : VposAuthType.PreAuth,
                        Installment = request.InstallmentCount,
                        OrderNumber = bankTransaction.OrderId,
                        Currency = verification.Currency,
                        CurrencyCode = currency.Code,
                        LanguageCode = request.LanguageCode,
                        SubMerchantCode = verification.SubMerchantCode,
                        SubMerchantTerminalNo = verification.SubMerchantTerminalNo,
                        IsBlockaged = vpos.VposType == VposType.Blockage ? true : false,
                        BlockageCode = vpos.BlockageCode,
                        Cavv = verification.Cavv,
                        Eci = verification.Eci,
                        MD = verification.Md,
                        OrgOrderNumber = verification.OrderId,
                        PayerTxnId = verification.PayerTxnId,
                        SubMerchantId = subMerchantInfo.Number,
                        SubMerchantName = subMerchantInfo.Name,
                        SubMerchantCity = subMerchantInfo.Customer.CityName,
                        SubMerchantTaxNumber = subMerchantInfo.Customer?.TaxNumber,
                        SubMerchantCountry = subMerchantInfo.Customer.Country.ToString(),
                        SubMerchantMcc = subMerchantInfo.MccCode,
                        SubMerchantUrl = subMerchantInfo.WebSiteUrl,
                        SubMerchantGlobalMerchantId = subMerchantInfo.GlobalMerchantId,
                        SubMerchantPostalCode = subMerchantInfo.Customer.PostalCode,
                        ClientIp = ipAddress,
                        CardHolderName = merchantTransaction.CardHolderName,
                        CardHolderIdentityNumber = merchantTransaction.CardHolderIdentityNumber,
                        BankPacket = verification.BankPacket,
                        IsTopUpPayment = request.IsTopUpPayment,
                        ServiceProviderPspMerchantId = vpos.MerchantVposList.FirstOrDefault()?.ServiceProviderPspMerchantId
                    });
            }
            catch (TaskCanceledException exception)
                when (exception.InnerException is TimeoutException)
            {
                await _basePaymentService.InsertTimeoutTransactionAsync(merchantTransaction, bankTransaction, ipAddress);

                var timeoutExceptionCode = await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.BankHasTimedOut, request.LanguageCode);

                try
                {
                    await UpdateHealthCheckAsync(merchantTransaction.Id);
                }
                catch (Exception exceptions)
                {
                    _logger.LogError($"UpdateHealthCheckException: {exceptions}");
                }

                return new ProvisionResponse
                {
                    IsSucceed = false,
                    ErrorCode = timeoutExceptionCode.Code,
                    ErrorMessage = timeoutExceptionCode.Message,
                    OrderId = string.Empty,
                    ProvisionNumber = merchantTransaction.ProvisionNumber,
                    Description = merchantTransaction.Description,
                    ConversationId = request.ConversationId,
                    TransactionId = merchantTransaction.Id
                };
            }

            if (posResponse.IsSuccess)
            {
                if (merchantTransaction.IsPerInstallment == true)
                {
                    var costProfile = await _costProfile.GetAll().Include(b => b.CostProfileItems).ThenInclude(b => b.CostProfileInstallments).FirstOrDefaultAsync(b => b.Id == verification.CostProfileItemId);

                    var newRouteRes = new RouteResponse
                    {
                        CostProfileItemId = costProfile.Id,
                        Installments = costProfile.CostProfileItems
                             .SelectMany(b => b.CostProfileInstallments)
                             .Select(b => new InstallmentItem
                             {
                                 InstallmentSequence = b.InstallmentSequence,
                                 BlockedDayNumber = b.BlockedDayNumber
                             }).ToList()
                    };

                    await CalculateInstallmentTransactions(merchantTransaction, pricingProfile, pricingProfileItem, vpos, newRouteRes, posResponse.AuthCode, Guid.Empty);
                }
            }


            var provisionResponse = posResponse.IsSuccess
                ? await MarkAsCompletedAsync(merchantTransaction, bankTransaction, posResponse,
                    verification: verification, token: cardToken)
                : await MarkAsFailedAsync(merchantTransaction, bankTransaction, posResponse, ipAddress);

            provisionResponse.TransactionId = merchantTransaction.Id;

            return provisionResponse;
        }
        catch (Exception exception)
        {
            _logger.LogError($"ThreeDPaymentAsyncException: {exception}");

            if (posResponse is not null)
            {
                return await RetryDbUpdateAsync(merchantTransaction, posResponse, bankTransaction,
                    request.ConversationId, request.ClientIpAddress, verification);
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

            try
            {
                await UpdateHealthCheckAsync(merchantTransaction.Id);
            }
            catch (Exception exceptions)
            {
                _logger.LogError($"UpdateHealthCheckException: {exceptions}");
            }

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = merchantTransaction.ResponseCode,
                ErrorMessage = "InternalError",
                OrderId = string.Empty,
                ConversationId = request.ConversationId
            };
        }
    }
    private static SecurityType GetSecurityType(ThreeDVerification verification)
    {
        SecurityType detectedSecurityType = SecurityType.Unknown;

        if (new[] { "2", "3", "4" }.Contains(verification.MdStatus))
        {
            detectedSecurityType = SecurityType.HalfSecure;
        }
        else if (verification.MdStatus == "1")
        {
            detectedSecurityType = SecurityType.FullSecure;
        }

        return detectedSecurityType;
    }

    private static BankTransaction PopulateInitialBankTransaction(ProvisionCommand request,
        MerchantTransaction merchantTransaction)
    {
        var bankTransaction = new BankTransaction
        {
            RecordStatus = RecordStatus.Active,
            CreateDate = DateTime.Now,
            CreatedBy = request.MerchantId.ToString(),
            TransactionStartDate = DateTime.Now,
            TransactionStatus = TransactionStatus.Pending,
            TransactionType = request.PaymentType switch
            {
                VposPaymentType.Auth => TransactionType.Auth,
                VposPaymentType.PreAuth => TransactionType.PreAuth,
                _ => TransactionType.PostAuth
            },
            Amount = request.Amount,
            PointAmount = request.PointAmount,
            InstallmentCount = request.InstallmentCount,
            Is3ds = !string.IsNullOrEmpty(request.ThreeDSessionId),
            IsReverse = false,
            MerchantTransactionId = merchantTransaction.Id,
            EndOfDayStatus = EndOfDayStatus.Pending
        };
        return bankTransaction;
    }

    private async Task<bool> PopulateInitialMerchantInstallmentTransaction(
       MerchantTransaction merchantTransaction, int installmentNumber, decimal amount, int pricingBlockedDayNumber,
    int costBlockedDayNumber, PricingProfile pricingProfile,
    PricingProfileItem pricingProfileItem,
    Vpos vpos,
    string provisionNumber,
    Guid referenceMerchantTransactionId)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();

        var merchantInstallmentTransaction = new MerchantInstallmentTransaction
        {
            RecordStatus = RecordStatus.Active,
            CreateDate = DateTime.Now,
            CreatedBy = merchantTransaction.CreatedBy,
            CreatedNameBy = merchantTransaction.CreatedNameBy,
            IpAddress = merchantTransaction.IpAddress,
            TransactionStartDate = merchantTransaction.TransactionStartDate,
            TransactionDate = merchantTransaction.TransactionDate,
            TransactionType = merchantTransaction.TransactionType,
            ConversationId = merchantTransaction.ConversationId,
            MerchantId = merchantTransaction.MerchantId,
            IntegrationMode = merchantTransaction.IntegrationMode,
            IsPreClose = merchantTransaction.IsPreClose,
            IsReverse = merchantTransaction.IsReverse,
            IsReturn = merchantTransaction.IsReturn,
            IsManualReturn = merchantTransaction.IsManualReturn,
            IsOnUsPayment = merchantTransaction.IsOnUsPayment,
            IsInsurancePayment = merchantTransaction.IsInsurancePayment,
            ReturnAmount = merchantTransaction.ReturnAmount,
            BankCommissionRate = merchantTransaction.BankCommissionRate,
            Currency = merchantTransaction.Currency,
            Amount = amount,
            PointAmount = merchantTransaction.PointAmount,
            InstallmentCount = installmentNumber,
            ThreeDSessionId = merchantTransaction.ThreeDSessionId,
            Is3ds = merchantTransaction.Is3ds,
            CardNumber = merchantTransaction.CardNumber,
            BinNumber = merchantTransaction.BinNumber,
            HasCvv = merchantTransaction.HasCvv,
            HasExpiryDate = merchantTransaction.HasExpiryDate,
            IsAmex = merchantTransaction.IsAmex,
            IsInternational = merchantTransaction.IsInternational,
            VposId = merchantTransaction.VposId,
            IssuerBankCode = merchantTransaction.IssuerBankCode,
            AcquireBankCode = merchantTransaction.AcquireBankCode,
            CardTransactionType = merchantTransaction.CardTransactionType,
            CardHolderName = merchantTransaction.CardHolderName,
            MerchantCustomerName = merchantTransaction.MerchantCustomerName,
            MerchantCustomerPhoneNumber = merchantTransaction.MerchantCustomerPhoneNumber,
            MerchantCustomerPhoneCode = merchantTransaction.MerchantCustomerPhoneCode,
            LanguageCode = merchantTransaction.LanguageCode,
            BatchStatus = merchantTransaction.BatchStatus,
            OrderId = merchantTransaction.OrderId,
            PostingItemId = merchantTransaction.PostingItemId,
            BlockageStatus = merchantTransaction.BlockageStatus,
            LastChargebackActivityDate = merchantTransaction.LastChargebackActivityDate,
            IsTopUpPayment = merchantTransaction.IsTopUpPayment,
            PfTransactionSource = merchantTransaction.PfTransactionSource,
            CardHolderIdentityNumber = merchantTransaction.CardHolderIdentityNumber,
            EndOfDayStatus = merchantTransaction.EndOfDayStatus,
            SubMerchantId = merchantTransaction.SubMerchantId,
            SubMerchantName = merchantTransaction.SubMerchantName,
            SubMerchantNumber = merchantTransaction.SubMerchantNumber,
            CardType = merchantTransaction.CardType,
            Description = merchantTransaction.Description,
            IsChargeback = merchantTransaction.IsChargeback,
            IsSuspecious = merchantTransaction.IsSuspecious,
            MerchantPhysicalPosId = merchantTransaction.MerchantPhysicalPosId,
            VposName = merchantTransaction.VposName,
            MerchantTransactionId = merchantTransaction.Id,
            PricingProfileItemId = pricingProfileItem.Id,
            PreCloseDate = merchantTransaction.PreCloseDate,
            PreCloseTransactionId = merchantTransaction.PreCloseTransactionId,
            ReturnDate = merchantTransaction.ReturnDate,
            ReturnedTransactionId = merchantTransaction.ReturnedTransactionId,
            ReturnStatus = merchantTransaction.ReturnStatus,
            ReverseDate = merchantTransaction.ReverseDate,
            SuspeciousDescription = merchantTransaction.SuspeciousDescription,
            TransactionEndDate = DateTime.Now,
            ResponseCode = GenericSuccessCode,
            ResponseDescription = GenericSuccessCode,
            TransactionStatus = TransactionStatus.Success,
            ProvisionNumber = provisionNumber,
            AmountWithoutBankCommission = 0,
            AmountWithoutCommissions = 0,
            AmountWithoutParentMerchantCommission = 0,
            BankCommissionAmount = 0,
            BsmvAmount = 0,
            ParentMerchantCommissionAmount = 0,
            ParentMerchantCommissionRate = 0,
            PfCommissionAmount = 0,
            PfCommissionRate = 0,
            PfNetCommissionAmount = 0,
            PfPerTransactionFee = 0,
            PointCommissionAmount = 0,
            PointCommissionRate = 0,
            ServiceCommissionAmount = 0,
            ServiceCommissionRate = 0,

        };

        merchantInstallmentTransaction.Amount = amount;

        if (merchantTransaction.TransactionType != TransactionType.PreAuth)
        {
            merchantInstallmentTransaction.BankCommissionAmount = (merchantTransaction.BankCommissionRate / 100m) * amount;

            merchantInstallmentTransaction.PfCommissionAmount = pricingProfile.PerTransactionFee
                + pricingProfileItem.CommissionRate / 100m * amount;

            merchantInstallmentTransaction.PfNetCommissionAmount = merchantInstallmentTransaction.PfCommissionAmount
                - merchantInstallmentTransaction.BankCommissionAmount;

            merchantInstallmentTransaction.PfCommissionRate = pricingProfileItem.CommissionRate;
            merchantInstallmentTransaction.PfPerTransactionFee = pricingProfile.PerTransactionFee;

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

            merchantInstallmentTransaction.PfPaymentDate = await _basePaymentService
                .CalculatePaymentDateAsync(merchantTransaction.TransactionDate, pricingBlockedDayNumber);

            merchantInstallmentTransaction.PointCommissionRate =
                GetPointCommissionRate(merchantTransaction.CardTransactionType, vpos);

            merchantInstallmentTransaction.PointCommissionAmount =
                (amount * merchantTransaction.PointCommissionRate) / 100m;

            merchantInstallmentTransaction.ServiceCommissionRate =
                GetServiceCommissionRate(merchantTransaction.CardTransactionType, vpos);

            merchantInstallmentTransaction.ServiceCommissionAmount =
                (amount * merchantTransaction.ServiceCommissionRate) / 100m;

            merchantInstallmentTransaction.BankPaymentDate = DateTime.Now.AddDays(costBlockedDayNumber + 1);
        }

        await dbContext.AddAsync(merchantInstallmentTransaction);

        if (merchantTransaction.TransactionType == TransactionType.PostAuth && referenceMerchantTransactionId != Guid.Empty)
        {
            var referenceInstallmentTransaction = await _merchantInstallmentTransactionRepository.GetAll()
                .Where(b => b.MerchantTransactionId == referenceMerchantTransactionId && b.InstallmentCount == installmentNumber)
                .FirstOrDefaultAsync();

            if (referenceInstallmentTransaction != null)
            {
                var transactionStatus = (referenceInstallmentTransaction.Amount > amount)
                                ? TransactionStatus.PartiallyClosed
                                : TransactionStatus.Closed;


                referenceInstallmentTransaction.TransactionStatus = transactionStatus;
                referenceInstallmentTransaction.IsPreClose = true;
                referenceInstallmentTransaction.PreCloseDate = DateTime.Now;
                referenceInstallmentTransaction.PreCloseTransactionId = merchantInstallmentTransaction.Id.ToString();
                referenceInstallmentTransaction.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();


                dbContext.Update(referenceInstallmentTransaction);
            }
        }

        await dbContext.SaveChangesAsync();

        return true;
    }

    private async Task<bool> CalculateInstallmentTransactions(
    MerchantTransaction merchantTransaction,
    PricingProfile pricingProfile,
    PricingProfileItem pricingProfileItem,
    Vpos vpos,
    RouteResponse routeResponse,
    string provisionNumber,
    Guid referenceMerchantTransactionId)
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

                var pricingBlockedDayNumber = pricingProfileItem.PricingProfileInstallments.Where(b => b.InstallmentSequence == i).FirstOrDefault().BlockedDayNumber;
                var costBlockedDayNumber = routeResponse.Installments.Where(b => b.InstallmentSequence == i).FirstOrDefault().BlockedDayNumber;

                var installmentTransaction = await PopulateInitialMerchantInstallmentTransaction(merchantTransaction, i, installmentAmount, pricingBlockedDayNumber, costBlockedDayNumber, pricingProfile, pricingProfileItem, vpos, provisionNumber, referenceMerchantTransactionId);

            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SaveMerchantInstallmentTransactionException: {exception}");
            return false;
        }


        return true;
    }
    private async Task<CardInfoDto> GetCardDetailsAsync(CardToken token)
    {
        return await _cardTokenService.GetCardDetailsAsync(token);
    }

    private async Task<string> GetMerchantCode(Guid acquirerBankId, Guid vposId)
    {
        var bankApiKey = await _bankApiKeyRepository.GetAll()
            .FirstOrDefaultAsync(s => s.AcquireBank.Id == acquirerBankId && s.Category == BankApiKeyCategory.MerchantId);

        if (bankApiKey is null)
        {
            return string.Empty;
        }

        var vposBankInfo = await _vposBankApiInfoRepository.GetAll()
            .FirstOrDefaultAsync(s => s.KeyId == bankApiKey.Id && s.VposId == vposId);

        return vposBankInfo == null
            ? string.Empty
            : vposBankInfo.Value;
    }

    private async Task<MerchantVpos> GetSubMerchantCode(Guid vposId, Guid merchantId)
    {
        var subMerchant = await _merchantVpos
            .GetAll()
            .FirstOrDefaultAsync(s => s.RecordStatus == RecordStatus.Active &&
                                      s.TerminalStatus == TerminalStatus.Active &&
                                      s.VposId == vposId &&
                                      s.MerchantId == merchantId);

        return subMerchant ?? new MerchantVpos();
    }
    private async Task<MerchantDto> GetSubMerchant(Guid merchantId)
    {
        var subMerchant = await _merchantService.GetByIdAsync(merchantId);

        return subMerchant ?? new MerchantDto();
    }
    private async Task<ProvisionResponse> MarkAsCompletedAsync(MerchantTransaction merchantTransaction,
        BankTransaction bankTransaction,
        PosPaymentProvisionResponse posResponse,
        MerchantTransaction referenceMerchantTransaction = null,
        BankTransaction referenceBankTransaction = null,
        ThreeDVerification verification = null,
        CardToken token = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            merchantTransaction.TransactionEndDate = DateTime.Now;
            merchantTransaction.ResponseCode = GenericSuccessCode;
            merchantTransaction.ResponseDescription = GenericSuccessCode;
            merchantTransaction.TransactionStatus = TransactionStatus.Success;
            merchantTransaction.ProvisionNumber = posResponse.AuthCode;

            if (merchantTransaction.TransactionType is TransactionType.Auth or TransactionType.PostAuth &&
                await _basePaymentService.GetIsPaymentDateWillBeShiftedAsync(
                    merchantTransaction.TransactionEndDate,
                    merchantTransaction.AcquireBankCode))
            {
                merchantTransaction.BankPaymentDate = merchantTransaction.BankPaymentDate.AddDays(1);
                merchantTransaction.PfPaymentDate = merchantTransaction.PfPaymentDate.AddDays(1);
            }

            bankTransaction.TransactionEndDate = DateTime.Now;
            bankTransaction.BankOrderId = posResponse.OrderNumber;
            bankTransaction.ApprovalCode = posResponse.AuthCode;
            bankTransaction.RrnNumber = posResponse.RrnNumber;
            bankTransaction.Stan = posResponse.Stan;
            bankTransaction.BankResponseCode = posResponse.ResponseCode;
            bankTransaction.BankResponseDescription = posResponse.ResponseMessage;
            bankTransaction.TransactionStatus = TransactionStatus.Success;
            bankTransaction.BankTransactionDate = posResponse.TrxDate;

            dbContext.MerchantTransaction.Update(merchantTransaction);

            await dbContext.AddAsync(bankTransaction);

            if (merchantTransaction.TransactionType == TransactionType.PostAuth && referenceMerchantTransaction != null)
            {
                var transactionStatus = (referenceMerchantTransaction.Amount > merchantTransaction.Amount)
                                    ? TransactionStatus.PartiallyClosed
                                    : TransactionStatus.Closed;

                referenceMerchantTransaction.TransactionStatus = transactionStatus;
                referenceMerchantTransaction.IsPreClose = true;
                referenceMerchantTransaction.PreCloseDate = DateTime.Now;
                referenceMerchantTransaction.PreCloseTransactionId = merchantTransaction.Id.ToString();
                referenceMerchantTransaction.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                dbContext.Update(referenceMerchantTransaction);

                if (referenceBankTransaction != null)
                {
                    referenceBankTransaction.TransactionStatus = transactionStatus;
                    referenceBankTransaction.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                    dbContext.Update(referenceBankTransaction);
                }
            }

            if (verification is not null)
            {
                verification.CurrentStep = VerificationStep.ProvisionCompleted;
                verification.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                dbContext.Update(verification);
            }

            await dbContext.SaveChangesAsync();

            transactionScope.Complete();
        });

        try
        {
            await _basePaymentService.PublishIncrementLimitAsync(merchantTransaction.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError($"PublishIncrementLimitException: {exception}");
        }

        if (token is not null)
        {
            await _cardTokenService.DeleteTokenAsync(token);
        }

        return new ProvisionResponse
        {
            IsSucceed = true,
            ConversationId = merchantTransaction.ConversationId,
            OrderId = bankTransaction.OrderId,
            ProvisionNumber = posResponse.AuthCode,
            TransactionId = merchantTransaction.Id
        };
    }
    private async Task UpdateHealthCheckAsync(Guid merchantTransactionId)
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.UpdateHealthCheck"));
        await endpoint.Send(new IncrementLimits
        {
            MerchantTransactionId = merchantTransactionId
        }, tokenSource.Token);
    }
    private async Task<ProvisionResponse> MarkAsFailedAsync(MerchantTransaction merchantTransaction,
        BankTransaction bankTransaction,
        PosPaymentProvisionResponse posResponse,
        string clientIpAddress,
        BankTransaction referenceBankTransaction = null)
    {
        var merchantError =
            await _errorCodeService.GetMerchantResponseCodeByBankCodeAsync(bankTransaction.AcquireBankCode,
                posResponse.ResponseCode,
                posResponse.ResponseMessage,
                merchantTransaction.LanguageCode);

        if (merchantError.ProcessTimeoutManagement)
        {
            if (referenceBankTransaction is not null)
            {
                await _basePaymentService.InsertTimeoutTransactionAsync(merchantTransaction, bankTransaction, clientIpAddress, referenceBankTransaction.OrderId);
            }
            else
            {
                await _basePaymentService.InsertTimeoutTransactionAsync(merchantTransaction, bankTransaction, clientIpAddress);
            }

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = merchantError.ResponseCode,
                ErrorMessage = merchantError.DisplayMessage,
                OrderId = merchantTransaction.OrderId,
                ProvisionNumber = merchantTransaction.ProvisionNumber,
                Description = merchantTransaction.Description,
                ConversationId = merchantTransaction.ConversationId,
                TransactionId = merchantTransaction.Id
            };
        }

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

            bankTransaction.BankResponseCode = posResponse.ResponseCode;
            bankTransaction.BankResponseDescription = posResponse.ResponseMessage;
            bankTransaction.TransactionEndDate = DateTime.Now;
            bankTransaction.TransactionStatus = TransactionStatus.Fail;
            bankTransaction.BankTransactionDate = posResponse.TrxDate;

            dbContext.MerchantTransaction.Update(merchantTransaction);

            await dbContext.AddAsync(bankTransaction);

            await dbContext.SaveChangesAsync();

            transactionScope.Complete();
        });

        try
        {
            await UpdateHealthCheckAsync(merchantTransaction.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateHealthCheckException: {exception}");
        }

        return new ProvisionResponse
        {
            IsSucceed = false,
            ErrorMessage = merchantError.DisplayMessage,
            ErrorCode = merchantError.ResponseCode,
            OrderId = string.Empty,
            ProvisionNumber = merchantTransaction.ProvisionNumber,
            Description = merchantTransaction.Description,
            ConversationId = merchantTransaction.ConversationId,
            TransactionId = merchantTransaction.Id
        };
    }

    private async Task<ProvisionResponse> RetryDbUpdateAsync(MerchantTransaction merchantTransaction,
        PosPaymentProvisionResponse posResponse,
        BankTransaction bankTransaction,
        string conversationId,
        string clientIpAddress,
        ThreeDVerification verification = null,
        BankTransaction referenceBankTransaction = null)
    {
        try
        {
            return posResponse.IsSuccess
                ? await MarkAsCompletedAsync(merchantTransaction, bankTransaction, posResponse,
                    verification: verification)
                : await MarkAsFailedAsync(merchantTransaction, bankTransaction, posResponse, clientIpAddress, referenceBankTransaction);
        }
        catch (Exception exception)
        {
            if (posResponse.IsSuccess)
            {
                _logger.LogError($"MarkAsCompleteError: MerchantTransactionId : {merchantTransaction.Id}" +
                                 $" Error : {exception}");

                await _basePaymentService.InsertTimeoutTransactionAsync(merchantTransaction, bankTransaction, clientIpAddress, referenceBankTransaction?.OrderId);
            }
            else
            {
                _logger.LogError($"MarkAsFailedError: MerchantTransactionId : {merchantTransaction.Id} " +
                                 $"Error : {exception}");
            }

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                OrderId = string.Empty,
                ConversationId = conversationId
            };
        }
    }

    private async Task<ProvisionResponse> CheckDuplicateRecordAsync(ProvisionCommand request)
    {
        var activeMerchantTransaction = await _merchantTransactionRepository.GetAll()
           .FirstOrDefaultAsync(b => b.MerchantId == request.MerchantId
           && b.ConversationId == request.ConversationId
           && b.RecordStatus == RecordStatus.Active);

        if (activeMerchantTransaction is not null)
        {
            return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.DuplicateMerchantTransaction, request);
        }

        return new ProvisionResponse { IsSucceed = true };
    }
    private decimal GetPointCommissionRate(CardTransactionType? cardTransactionType, Vpos vpos)
    {
        if (cardTransactionType == CardTransactionType.OnUs)
        {
            var costProfile = vpos.CostProfiles.FirstOrDefault(c => c.RecordStatus == RecordStatus.Active
                                                                    && c.ProfileStatus == ProfileStatus.InUse);
            return costProfile?.PointCommission ?? 0;
        }
        else
            return 0;
    }

    private decimal GetServiceCommissionRate(CardTransactionType? cardTransactionType, Vpos vpos)
    {
        var costProfile = vpos.CostProfiles.FirstOrDefault(c => c.RecordStatus == RecordStatus.Active
                                                                && c.ProfileStatus == ProfileStatus.InUse);
        return costProfile?.ServiceCommission ?? 0;
    }
}