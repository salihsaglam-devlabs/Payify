using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.Payments.Commands.GetThreeDSession;
using LinkPara.PF.Application.Features.Payments.Commands.GetThreeDSessionResult;
using LinkPara.PF.Application.Features.Payments.Commands.Init3ds;
using LinkPara.PF.Application.Features.Payments.Commands.Verify3ds;
using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.PF.Application.Features.Tokens;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Services.PaymentServices;
using LinkPara.PF.Infrastructure.Services.VposServices;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;

namespace LinkPara.PF.Infrastructure.Services;

public class ThreeDService : IThreeDService
{
    private readonly string ThreeDHalfSecure = "A";
    private const string GenericErrorCode = "99";

    private readonly ILogger<ThreeDService> _logger;
    private readonly ICardBinService _binService;
    private readonly ICurrencyService _currencyService;
    private readonly ISubMerchantService _subMerchantService;
    private readonly IPosRouterService _posRouterService;
    private readonly ICardTokenService _cardTokenService;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IGenericRepository<VposBankApiInfo> _vposBankApiInfoRepository;
    private readonly IGenericRepository<MerchantVpos> _merchantVpos;
    private readonly IGenericRepository<BankApiKey> _bankApiKeyRepository;
    private readonly IResponseCodeService _errorCodeService;
    private readonly IGenericRepository<MerchantApiValidationLog> _apiRequestValidationLogRepository;
    private readonly IGenericRepository<ThreeDVerification> _threeDVerificationRepository;
    private readonly IOrderNumberGeneratorService _orderNumberGeneratorService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly IVaultClient _vaultClient;
    private readonly VposServiceFactory _vposServiceFactory;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IMapper _mapper;
    public ThreeDService(
        ILogger<ThreeDService> logger,
        ICardBinService binService,
        ICurrencyService currencyService,
        ISubMerchantService subMerchantService,
        IPosRouterService posRouterService,
        ICardTokenService cardTokenService,
        IGenericRepository<Vpos> vposRepository,
        IGenericRepository<VposBankApiInfo> vposBankApiInfoRepository,
        IGenericRepository<MerchantVpos> merchantVpos,
        IGenericRepository<BankApiKey> bankApiKeyRepository,
        IResponseCodeService errorCodeService,
        IGenericRepository<MerchantApiValidationLog> apiRequestValidationLogRepository,
        IGenericRepository<ThreeDVerification> threeDVerificationRepository,
        IOrderNumberGeneratorService orderNumberGeneratorService,
        IApplicationUserService applicationUserService,
        IAuditLogService auditLogService,
        IVaultClient vaultClient,
        VposServiceFactory vposServiceFactory,
        IContextProvider contextProvider,
        IGenericRepository<Merchant> merchantRepository, IMapper mapper)
    {
        _logger = logger;
        _binService = binService;
        _currencyService = currencyService;
        _vposRepository = vposRepository;
        _subMerchantService = subMerchantService;
        _posRouterService = posRouterService;
        _cardTokenService = cardTokenService;
        _vposBankApiInfoRepository = vposBankApiInfoRepository;
        _merchantVpos = merchantVpos;
        _bankApiKeyRepository = bankApiKeyRepository;
        _errorCodeService = errorCodeService;
        _apiRequestValidationLogRepository = apiRequestValidationLogRepository;
        _threeDVerificationRepository = threeDVerificationRepository;
        _orderNumberGeneratorService = orderNumberGeneratorService;
        _applicationUserService = applicationUserService;
        _auditLogService = auditLogService;
        _vaultClient = vaultClient;
        _vposServiceFactory = vposServiceFactory;
        _contextProvider = contextProvider;
        _merchantRepository = merchantRepository;
        _mapper = mapper;
    }

    public async Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionCommand request)
    {
        var merchant = await GetMerchantByIdAsync(request.MerchantId);

        var subMerchant = new SubMerchantDto();
        if (request.SubMerchantId.HasValue && request.SubMerchantId.Value != Guid.Empty)
        {
            subMerchant = await _subMerchantService.GetByIdAsync(request.SubMerchantId.Value);
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

        var validationResponse = await PreValidate(request, merchant, parentMerchantFinancialTransaction, subMerchant);

        if (!validationResponse.IsValid)
        {
            _logger.LogError($"GetThreeDSession PreValidation failed with code : {validationResponse.Code}, " +
                             $"Message: {validationResponse.Message}");

            await InsertValidationLogAsync(request, validationResponse);

            return new GetThreeDSessionResponse
            {
                IsSucceed = false,
                ErrorCode = validationResponse.Code,
                ConversationId = request.ConversationId,
                ErrorMessage = validationResponse.Message,
                ThreeDSessionId = string.Empty
            };
        }

        if (request.IsTopUpPayment == true)
        {
            var vaultMerchantId = _vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "MerchantId");

            if (Guid.Parse(vaultMerchantId) != merchant.Id)
            {
                return new GetThreeDSessionResponse
                {
                    IsSucceed = false,
                    ErrorCode = "99",
                    ConversationId = request.ConversationId,
                    ErrorMessage = "Invalid Merchant",
                    ThreeDSessionId = string.Empty
                };
            }
        }

        SetInstallmentNumber(request);

        try
        {
            var token = await _cardTokenService.GetByToken(request.CardToken);
            var card = await GetCardDetailsAsync(token);
            var bin = await _binService.GetByNumberAsync(card.CardNumber);

            RouteResponse routeInfo;
            var shortCircuitVposId = await _posRouterService.CheckRouteForShortCircuitAsync(
                merchant.Id, card.CardNumber[..8], request.Currency, request.InstallmentCount, request.Amount);

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

            var merchantCode = await GetMerchantCode(routeInfo.AcquireBank.Id, routeInfo.Vpos.Id);
            var subMerchantCode = await GetSubMerchantCode(routeInfo.Vpos.Id, merchant.Id);
            var currency = await _currencyService.GetByCodeAsync(request.Currency);
            var orderId = await _orderNumberGeneratorService.GenerateForBankTransactionAsync(routeInfo.AcquireBank.BankCode, merchant.Number);

            var verification = new ThreeDVerification
            {
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                Amount = request.Amount,
                CardToken = request.CardToken,
                CreateDate = DateTime.Now,
                InstallmentCount = request.InstallmentCount,
                TransactionType = request.PaymentType == VposPaymentType.Auth
                    ? TransactionType.Auth
                    : TransactionType.PreAuth,
                MerchantCode = merchantCode,
                SubMerchantCode = subMerchantCode.SubMerchantCode ?? string.Empty,
                SubMerchantTerminalNo = subMerchantCode.TerminalNo ?? string.Empty,
                AcquireBankCode = routeInfo.AcquireBank.BankCode,
                VposId = routeInfo.Vpos.Id,
                IssuerBankCode = bin?.BankCode ?? 0,
                CurrentStep = VerificationStep.SessionIdRequested,
                PointAmount = request.PointAmount,
                MerchantId = merchant.Id,
                BinNumber = bin?.BinNumber,
                Currency = currency.Number,
                OrderId = orderId,
                SessionExpiryDate = token.ExpiryDate,
                BankCommissionAmount = routeInfo.CommissionAmount,
                BankCommissionRate = routeInfo.CommissionRate,
                BankBlockedDayNumber = routeInfo.BlockedDayNumber,
                ConversationId = request.ConversationId,
                CostProfileItemId = routeInfo.CostProfileItemId,
                IsPerInstallment = routeInfo.ProfileSettlementMode == ProfileSettlementMode.PerInstallment ? true : false
            };

            await _threeDVerificationRepository.AddAsync(verification);

            return new GetThreeDSessionResponse
            {
                ConversationId = request.ConversationId,
                ErrorCode = string.Empty,
                ErrorMessage = string.Empty,
                IsSucceed = true,
                ThreeDSessionId = verification.Id.ToString()
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetThreeDSession Error : {exception}");

            return new GetThreeDSessionResponse
            {
                ConversationId = request.ConversationId,
                ErrorCode = (exception is ApiException apiException)
                    ? apiException.Code
                    : GenericErrorCode,
                ErrorMessage = (exception is ApiException ex)
                    ? ex.Message
                    : "InternalError",
                IsSucceed = false,
                ThreeDSessionId = string.Empty
            };
        }
    }

    public async Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync(GetThreeDSessionResultCommand request)
    {
        var merchant = await GetMerchantByIdAsync(request.MerchantId);

        var parentMerchantFinancialTransaction = true;
        if (merchant.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
        {
            var parentMerchant = await _merchantRepository.GetByIdAsync(merchant.ParentMerchantId);
            if (parentMerchant is not null)
            {
                parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
            }
        }

        var validationResponse =
            await PreValidateThreeDSession(request.ThreeDSessionId, request.LanguageCode, merchant, parentMerchantFinancialTransaction);

        if (!validationResponse.IsValid)
        {
            _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                             $"Message: {validationResponse.Message}");

            return new GetThreeDSessionResultResponse
            {
                IsSucceed = false,
                ErrorCode = validationResponse.Code,
                ConversationId = request.ConversationId,
                ErrorMessage = validationResponse.Message,
                CurrentStep = string.Empty,
                MdErrorMessage = string.Empty,
                MdStatus = string.Empty,
                HalfSecure = false
            };
        }

        _ = Guid.TryParse(request.ThreeDSessionId, out var sessionId);

        var verification = await _threeDVerificationRepository.GetAll()
            .FirstOrDefaultAsync(s =>
                s.Id == sessionId && s.MerchantId == request.MerchantId);

        if (verification is not null)
        {
            return new GetThreeDSessionResultResponse
            {
                ConversationId = request.ConversationId,
                CurrentStep = verification.CurrentStep.ToString(),
                IsSucceed = true,
                MdErrorMessage = verification.MdErrorMessage,
                MdStatus = verification.MdStatus,
                HalfSecure = verification.TxnStat == ThreeDHalfSecure
            };
        }

        var error = await GetValidationResponseAsync(ApiErrorCode.InvalidSessionId, request.LanguageCode);

        return new GetThreeDSessionResultResponse
        {
            ConversationId = request.ConversationId,
            IsSucceed = false,
            ErrorCode = error.Code,
            ErrorMessage = error.Message,
            CurrentStep = string.Empty,
            MdErrorMessage = string.Empty,
            MdStatus = string.Empty,
            HalfSecure = false
        };
    }

    public async Task<Init3dsResponse> Init3ds(Init3dsCommand request)
    {
        var merchant = await GetMerchantByIdAsync(request.MerchantId);

        var parentMerchantFinancialTransaction = true;
        if (merchant.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
        {
            var parentMerchant = await _merchantRepository.GetByIdAsync(merchant.ParentMerchantId);
            if (parentMerchant is not null)
            {
                parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
            }
        }

        var validationResponse =
            await PreValidateThreeDSession(request.ThreeDSessionId, request.LanguageCode, merchant, parentMerchantFinancialTransaction);

        var response = new Init3dsResponse
        {
            IsSucceed = false,
            HtmlContent = string.Empty,
            ConversationId = request.ConversationId
        };

        if (!validationResponse.IsValid)
        {
            _logger.LogError($"Init3ds PreValidation failed with code : {validationResponse.Code}, " +
                             $"Message: {validationResponse.Message}");

            response.ErrorCode = validationResponse.Code;
            response.ErrorMessage = validationResponse.Message;
            return response;
        }

        _ = Guid.TryParse(request.ThreeDSessionId, out var sessionId);

        var verification = await _threeDVerificationRepository.GetAll()
            .FirstOrDefaultAsync(s =>
                s.Id == sessionId &&
                s.MerchantId == request.MerchantId &&
                s.CurrentStep == VerificationStep.SessionIdRequested &&
                s.SessionExpiryDate >= DateTime.Now);

        if (verification is null)
        {
            var apiResponse = await _errorCodeService.GetApiResponseCode(ApiErrorCode.ThreeDVerificationNotFound, string.Empty);
            response.ErrorCode = apiResponse.ResponseCode;
            response.ErrorMessage = apiResponse.DisplayMessage;
            return response;
        }

        try
        {
            var card = await GetCardDetailsAsync(verification.CardToken);
            var bin = await _binService.GetByNumberAsync(card.CardNumber);
            var vpos = await _vposRepository.GetAll()
                .Include(b => b.MerchantVposList.Where(a => a.MerchantId == merchant.Id && a.TerminalStatus == TerminalStatus.Active))
                .Include(s => s.AcquireBank)
                .Include(s => s.VposBankApiInfos)
                .ThenInclude(s => s.Key)
                .FirstOrDefaultAsync(s => s.Id == verification.VposId);

            var subMerchantInfo = await GetSubMerchant(merchant.Id);

            var bankService = _vposServiceFactory.GetVposServices(vpos, merchant.Id, request.IsInsurancePayment);

            var currency = await _currencyService.GetByNumberAsync(verification.Currency);

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var clientIp = request.ClientIpAddress;

            if (environment?.ToLowerInvariant() == "development")
            {
                clientIp = "192.168.1.1";
            }

            if (request.IsTopUpPayment == true)
            {
                var vaultMerchantId = _vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "MerchantId");

                if (Guid.Parse(vaultMerchantId) != merchant.Id)
                {

                    response.ErrorMessage = "Invalid Merchant";
                    response.ErrorCode = "99";
                    return response;
                }
            }

            var posResponse = await bankService.Init3DModel(new PosInit3DModelRequest
            {
                Amount = verification.Amount,
                BonusAmount = verification.PointAmount,
                AuthType = verification.TransactionType == TransactionType.Auth
                    ? VposAuthType.Auth
                    : VposAuthType.PreAuth,
                CardNumber = card.CardNumber,
                CardBrand = bin?.CardBrand ?? CardBrand.Undefined,
                ExpireMonth = card.ExpireMonth,
                ExpireYear = card.ExpireYear,
                Cvv2 = card.Cvv,
                IsBlockaged = vpos.VposType == VposType.Blockage ? true : false,
                BlockageCode = vpos.BlockageCode,
                LanguageCode = request.LanguageCode,
                OrderNumber = verification.OrderId,
                SubMerchantCode = verification.SubMerchantCode,
                Installment = verification.InstallmentCount,
                Currency = verification.Currency,
                CurrencyCode = currency.Code,
                ThreedSessionId = verification.Id,
                SubMerchantId = subMerchantInfo.Number,
                CardHolderName = request.CardHolderName,
                SubMerchantName = subMerchantInfo.Name,
                SubMerchantCity = subMerchantInfo.Customer?.CityName,
                SubmerchantDistrict = subMerchantInfo.Customer?.DistrictName,
                SubmerchantAddress = subMerchantInfo.Customer?.Address,
                SubMerchantTaxNumber = subMerchantInfo.Customer?.TaxNumber,
                SubMerchantCountry = subMerchantInfo.Customer?.Country.ToString(),
                SubMerchantMcc = subMerchantInfo.MccCode,
                SubMerchantUrl = subMerchantInfo.WebSiteUrl,
                SubMerchantGlobalMerchantId = subMerchantInfo.GlobalMerchantId,
                SubMerchantPostalCode = subMerchantInfo.Customer?.PostalCode,
                SubmerchantEmail = subMerchantInfo.Customer?.AuthorizedPerson?.Email,
                SubmerchantPhoneNumber = subMerchantInfo.Customer?.AuthorizedPerson?.MobilePhoneNumber,
                SubmerchantPhoneCode = subMerchantInfo.PhoneCode,
                ClientIp = !string.IsNullOrWhiteSpace(clientIp) && clientIp.ToLower() != "null"
                           ? clientIp
                           : _contextProvider.CurrentContext.ClientIpAddress,
                VposCallbackUrl = _vaultClient.GetSecretValue<string>("PFSecrets", "VposSettings", "CallbackUrl"),
                IsTopUpPayment = request.IsTopUpPayment,
                ServiceProviderPspMerchantId = vpos.MerchantVposList.FirstOrDefault()?.ServiceProviderPspMerchantId
            });


            if (!posResponse.IsSuccess)
            {
                verification.HashKey = posResponse.Hash;
                verification.BankResponseCode = posResponse.ResponseCode;
                verification.BankResponseDescription = posResponse.ResponseMessage;

                await _threeDVerificationRepository.UpdateAsync(verification);

                response.IsSucceed = posResponse.IsSuccess;
                response.HtmlContent = posResponse.HtmlContent;
                response.ErrorCode = posResponse.ResponseCode;
                response.ErrorMessage = posResponse.ResponseMessage;
                return response;
            }

            verification.CallbackUrl = request.CallbackUrl;
            verification.CurrentStep = VerificationStep.VerificationStarted;
            verification.BankTransactionDate = posResponse.TrxDate;
            verification.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
            verification.BankResponseCode = posResponse.ResponseCode;
            verification.BankResponseDescription = posResponse.ResponseMessage;

            await _threeDVerificationRepository.UpdateAsync(verification);

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateThreeDVerification",
                SourceApplication = "PF",
                Resource = "ThreeDVerification",
                Details = new Dictionary<string, string>
                {
                       {"Id", verification.Id.ToString() },
                       {"OrderId", verification.OrderId},
                       {"Amount", verification.Amount.ToString() },
                       {"BinNumber", verification.BinNumber }
                }
            });

            response.IsSucceed = true;
            response.HtmlContent = posResponse.HtmlContent;
            response.ErrorCode = posResponse.ResponseCode;
            response.ErrorMessage = posResponse.ResponseMessage;
            return response;

        }
        catch (Exception exception)
        {
            _logger.LogError($"Start Init3ds failed with code : {exception}");
        }
        response.ErrorMessage = "System Error";
        response.ErrorCode = "99";
        return response;
    }

    public async Task<Verify3dsResponse> Verify3ds(Verify3dsCommand request)
    {
        var verification = await _threeDVerificationRepository
            .GetAll()
            .FirstOrDefaultAsync(s =>
                s.Id == request.ThreedSessionId &&
                s.OrderId == request.OrderId);

        if (verification is null || verification.SessionExpiryDate <= DateTime.Now || verification.CurrentStep != VerificationStep.VerificationStarted)
        {
            var apiResponse = await _errorCodeService.GetApiResponseCode(ApiErrorCode.InvalidSessionId, string.Empty);

            var response = new Verify3dsResponse
            {
                IsSucceed = false,
                HalfSecure = false,
                ErrorCode = apiResponse.ResponseCode,
                ErrorMessage = apiResponse.DisplayMessage,
                ThreeDSessionId = request.ThreedSessionId.ToString()
            };

            if (verification is not null)
            {
                response.CallbackUrl = verification.CallbackUrl;
                response.ConversationId = verification.ConversationId;
            }

            return response;
        }

        PosVerify3dModelResponse posResponse = null;

        try
        {
            var vpos = await _vposRepository.GetAll()
                .Include(s => s.AcquireBank)
                .Include(s => s.VposBankApiInfos)
                .ThenInclude(s => s.Key)
                .FirstOrDefaultAsync(s => s.Id == verification.VposId);

            var bankService = _vposServiceFactory.GetVposServices(vpos, verification.MerchantId);

            if (verification.AcquireBankCode == (int)BankCode.YapiKredi)
            {
                var currency = await _currencyService.GetByNumberAsync(verification.Currency);
                request.FormCollection.Add("CurrencyCode", currency.Code);
            }

            posResponse = await bankService.Verify3DModel(request.FormCollection);

            verification.CurrentStep = VerificationStep.VerificationFinished;
            verification.Cavv = posResponse.Cavv;
            verification.Eci = posResponse.Eci;
            verification.Md = posResponse.MD;
            verification.Xid = posResponse.OrderNumber;
            verification.HashKey = posResponse.Hash;
            verification.MdStatus = posResponse.MdStatus;
            verification.PayerTxnId = posResponse.PayerTxnId;
            verification.BankResponseCode = posResponse.ResponseCode;
            verification.BankResponseDescription = posResponse.ResponseMessage;
            verification.MdErrorMessage = posResponse.MdErrorMessage;
            verification.TxnStat = posResponse.TxnStat;
            verification.ThreeDStatus = posResponse.ThreeDStatus;
            verification.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
            verification.BankPacket = posResponse.BankPacket;

            await _threeDVerificationRepository.UpdateAsync(verification);

            if (posResponse.IsSuccess)
            {
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
                    return new Verify3dsResponse
                    {
                        CallbackUrl = verification.CallbackUrl,
                        ErrorCode = "99",
                        ErrorMessage = "Vpos security type is invalid!",
                        IsSucceed = false,
                        MdStatus = verification.MdStatus,
                        ThreeDSessionId = verification.Id.ToString(),
                        HalfSecure = false,
                        ConversationId = verification.ConversationId,
                    };
                }

                var merchant = await _merchantRepository.GetByIdAsync(verification.MerchantId);
                if (merchant is not null && requiredSecurityType == SecurityType.HalfSecure && !merchant.HalfSecureAllowed)
                {
                    return new Verify3dsResponse
                    {
                        CallbackUrl = verification.CallbackUrl,
                        ErrorCode = "99",
                        ErrorMessage = "Merchant does not have Half Secure payment authorization!",
                        IsSucceed = false,
                        MdStatus = verification.MdStatus,
                        ThreeDSessionId = verification.Id.ToString(),
                        HalfSecure = false,
                        ConversationId = verification.ConversationId,
                    };
                }
            }

            return posResponse.IsSuccess
                ? new Verify3dsResponse
                {
                    IsSucceed = true,
                    ThreeDSessionId = verification.Id.ToString(),
                    MdStatus = verification.MdStatus,
                    CallbackUrl = verification.CallbackUrl,
                    HalfSecure = verification.TxnStat == ThreeDHalfSecure,
                    ConversationId = verification.ConversationId,
                }
                : new Verify3dsResponse
                {
                    CallbackUrl = verification.CallbackUrl,
                    ErrorCode = posResponse.ResponseCode,
                    ErrorMessage = posResponse.ResponseMessage,
                    IsSucceed = false,
                    MdStatus = verification.MdStatus,
                    ThreeDSessionId = verification.Id.ToString(),
                    HalfSecure = false,
                    ConversationId = verification.ConversationId,
                };

        }
        catch (Exception exception)
        {
            _logger.LogError($"Verify 3ds failed : {exception}");

            return new Verify3dsResponse
            {
                CallbackUrl = verification.CallbackUrl,
                IsSucceed = false,
                MdStatus = verification.MdStatus,
                ThreeDSessionId = verification.Id.ToString(),
                HalfSecure = false,
                ErrorCode = (exception is ApiException apiException)
                    ? apiException.Code
                    : GenericErrorCode,
                ErrorMessage = (exception is ApiException ex)
                    ? ex.Message
                    : "InternalError",
                ConversationId = verification.ConversationId
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
    private async Task InsertValidationLogAsync(GetThreeDSessionCommand request, ValidationResponse validationResponse)
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
            TransactionType = request.PaymentType == VposPaymentType.Auth
                ? TransactionType.Auth
                : TransactionType.PreAuth,
            ClientIpAddress = request.ClientIpAddress,
            OriginalReferenceNumber = string.Empty,
            ThreeDSessionId = string.Empty,
            ErrorCode = validationResponse.Code == null ? "Error Code Not Found!" : validationResponse.Code,
            ErrorMessage = validationResponse.Message == null ? "Error Message Not Found!" : validationResponse.Message
        });
    }

    private async Task<ValidationResponse> PreValidate(GetThreeDSessionCommand request, MerchantDto merchant, bool parentMerchantFinancialTransaction, SubMerchantDto subMerchant)
    {
        if (request.PaymentType == VposPaymentType.PostAuth)
        {
            return await GetValidationResponseAsync(ApiErrorCode.InvalidPaymentType, request.LanguageCode);
        }

        if (merchant is null)
        {
            return await GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, request.LanguageCode);
        }

        if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
        {
            return await GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, request.LanguageCode);
        }

        if (request.PaymentType == VposPaymentType.PreAuth && !merchant.PreAuthorizationAllowed)
        {
            return await GetValidationResponseAsync(ApiErrorCode.PreAuthorizationNotAllowed, request.LanguageCode);
        }

        if (request.InstallmentCount > 1 && !merchant.InstallmentAllowed)
        {
            return await GetValidationResponseAsync(ApiErrorCode.InstallmentNotAllowed, request.LanguageCode);
        }

        if (!string.IsNullOrEmpty(subMerchant.Name))
        {

            if (request.PaymentType == VposPaymentType.PreAuth && !subMerchant.PreAuthorizationAllowed)
            {
                return await GetValidationResponseAsync(ApiErrorCode.PreAuthorizationNotAllowed, request.LanguageCode);
            }

            if (request.InstallmentCount > 1 && !subMerchant.InstallmentAllowed)
            {
                return await GetValidationResponseAsync(ApiErrorCode.SubMerchantInstallmentNotAllowed, request.LanguageCode);
            }

            if (subMerchant.RecordStatus != RecordStatus.Active)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidSubMerchantStatus, request.LanguageCode);
            }
        }

        var cardToken = await _cardTokenService.GetByToken(request.CardToken);

        if (cardToken is null)
        {
            return await GetValidationResponseAsync(ApiErrorCode.InvalidToken, request.LanguageCode);
        }

        if (DateTime.Now.CompareTo(cardToken.ExpiryDate) >= 0)
        {
            return await GetValidationResponseAsync(ApiErrorCode.CardTokenExpired, request.LanguageCode);
        }

        try
        {
            var cardInfo = await _cardTokenService.GetCardDetailsAsync(cardToken);

            var bin = await _binService.GetByNumberAsync(cardInfo.CardNumber);

            if (bin is null && (!merchant.InternationalCardAllowed || (!string.IsNullOrEmpty(subMerchant.Name) && !subMerchant.InternationalCardAllowed)))
            {
                return await GetValidationResponseAsync(ApiErrorCode.InternationalCardNotAllowed, request.LanguageCode);
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"InvalidCardInfoException: {e}");
            return await GetValidationResponseAsync(ApiErrorCode.InvalidCardInfo, request.LanguageCode);
        }

        return new ValidationResponse { IsValid = true };
    }

    private async Task<ValidationResponse> PreValidateThreeDSession(string threeDSessionId, string language, MerchantDto merchant, bool parentMerchantFinancialTransaction, bool? isInsurancePayment = false)
    {
        if (merchant is null)
        {
            return await GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, language);
        }

        if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
        {
            return await GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, language);
        }

        if (string.IsNullOrEmpty(merchant.GlobalMerchantId))
        {
            return await GetValidationResponseAsync(ApiErrorCode.GlobalMerchantIdNotFound, language);
        }

        if (!Guid.TryParse(threeDSessionId, out _))
        {
            return await GetValidationResponseAsync(ApiErrorCode.InvalidSessionId, language);
        }

        if (isInsurancePayment == true && !merchant.InsurancePaymentAllowed)
        {
            return await GetValidationResponseAsync(ApiErrorCode.InsurancePaymentNotAllowed, language);
        }

        return new ValidationResponse { IsValid = true };
    }

    private async Task<ValidationResponse> GetValidationResponseAsync(string errorCode, string languageCode)
    {
        var merchantResponse = await _errorCodeService.GetApiResponseCode(errorCode, languageCode);

        return new ValidationResponse
        {
            Code = errorCode,
            IsValid = false,
            Message = merchantResponse.DisplayMessage
        };
    }

    private static void SetInstallmentNumber(GetThreeDSessionCommand request)
    {
        request.InstallmentCount = request.InstallmentCount == 1
           ? 0
           : request.InstallmentCount;
    }

    private async Task<CardInfoDto> GetCardDetailsAsync(string token)
    {
        var cardToken = await _cardTokenService.GetByToken(token);

        var cardInfo = await _cardTokenService.GetCardDetailsAsync(cardToken);

        return cardInfo;
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

        if (vposBankInfo is null)
        {
            return string.Empty;
        }

        return vposBankInfo.Value ?? string.Empty;
    }

    private async Task<MerchantVpos> GetSubMerchantCode(Guid vposId, Guid merchantId)
    {
        var subMerchant = await _merchantVpos
           .GetAll()
           .FirstOrDefaultAsync(s => s.RecordStatus == RecordStatus.Active &&
                                     s.VposId == vposId &&
                                     s.MerchantId == merchantId);

        if (subMerchant is null)
        {
            return new MerchantVpos();
        }

        return subMerchant;

    }

    private async Task<MerchantDto> GetSubMerchant(Guid merchantId)
    {
        var subMerchant = await GetMerchantByIdAsync(merchantId);

        return subMerchant ?? new MerchantDto();
    }
    private async Task<MerchantDto> GetMerchantByIdAsync(Guid id)
    {
        var merchant = await _merchantRepository.GetAll()
            .Include(b => b.Customer)
            .ThenInclude(b => b.AuthorizedPerson)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), id);
        }

        return _mapper.Map<MerchantDto>(merchant);
    }
}