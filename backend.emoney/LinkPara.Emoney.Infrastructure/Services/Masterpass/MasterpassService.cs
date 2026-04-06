using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.Masterpass.Requests;
using LinkPara.Emoney.Application.Commons.Models.Masterpass.Responses;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Application.Features.Topups.Commands.TopupProcess;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LinkPara.Emoney.Infrastructure.Services.Masterpass;

public class MasterpassService : HttpClientBase, IMasterpassService
{
    private readonly HttpClient _client;

    private readonly IGenericRepository<CardTopupRequest> _cardTopupRequestRepository;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<MasterpassService> _logger;
    private readonly ICacheService _cacheService;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<PricingProfile> _pricingProfileRepository;
    private readonly IParameterService _parameterService;
    private readonly IApplicationUserService _applicationUserService;

    private readonly int _merchantId;
    private readonly string _grantType;
    private readonly string _terminalGroupId;
    private readonly string _secret;
    private readonly string _errorMessage;
    private readonly Uri _pfServiceUrl;

    public MasterpassService(HttpClient client,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IVaultClient vaultClient,
        IGenericRepository<CardTopupRequest> cardTopupRequestRepository,
        ILogger<MasterpassService> logger,
        IStringLocalizerFactory factory,
        ICacheService cacheService,
        IContextProvider contextProvider,
        IGenericRepository<PricingProfile> pricingProfileRepository,
        IParameterService parameterService,
        IApplicationUserService applicationUserService)
        : base(client, httpContextAccessor)
    {
        _client = httpClientFactory.CreateClient("MasterpassClient");
        var masterpassServiceUrl = vaultClient.GetSecretValue<string>("EmoneySecrets", "MasterpassConfigs", "BaseUrl");
        _merchantId = int.Parse(vaultClient.GetSecretValue<string>("EmoneySecrets", "MasterpassConfigs", "MerchantId"));
        _terminalGroupId = vaultClient.GetSecretValue<string>("EmoneySecrets", "MasterpassConfigs", "TerminalGroupId");
        _secret = vaultClient.GetSecretValue<string>("EmoneySecrets", "MasterpassConfigs", "Secret");
        _grantType = vaultClient.GetSecretValue<string>("EmoneySecrets", "MasterpassConfigs", "GrantType");
        _client.BaseAddress = new Uri(masterpassServiceUrl);
        _cardTopupRequestRepository = cardTopupRequestRepository;
        _localizer = factory.Create("ErrorMessages", "LinkPara.Emoney.API");
        _errorMessage = _localizer["UnexpectedErrorMessage"];
        _logger = logger;
        _cacheService = cacheService;
        _contextProvider = contextProvider;
        _pricingProfileRepository = pricingProfileRepository;
        _parameterService = parameterService;
        _applicationUserService = applicationUserService;
        var pfServiceUrl = vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Pf");
        _pfServiceUrl = new Uri(pfServiceUrl);
    }

    public async Task<BaseResponse<GenerateAccessTokenResponse>> GenerateAccessTokenAsync(GenerateAccessTokenRequest request)
    {
        var cacheKey = $"Token_{request.UserId}_{request.OrderNo}";
        var cacheMinute = TimeSpan.FromMinutes(10);

        var cachedResponse = await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            request.MerchantId = _merchantId;

            _client.DefaultRequestHeaders.Add("Secret", _merchantId.ToString());
            _client.DefaultRequestHeaders.Add("ClientId", _merchantId.ToString());
            _client.DefaultRequestHeaders.Add("GrantType", _grantType);

            var apiResponse = await SendRequestAsync<BaseResponse<GenerateAccessTokenResponse>>("merchant/api/token/generate", request);

            if (apiResponse.Result != null)
            {
                apiResponse.Result.OrderId = request.OrderNo;
                apiResponse.Result.AuthenticationMethod = request.AuthenticationMethod;
                apiResponse.Result.Secure3dType = request.Secure3dType;
            }
            return apiResponse;

        }, (int)cacheMinute.TotalMinutes);

        cachedResponse.Result.AuthenticationMethod = request.AuthenticationMethod;
        cachedResponse.Result.Secure3dType = request.Secure3dType;

        return cachedResponse;
    }

    public async Task ThreedSecureAsync(ThreedSecureRequest request)
    {
        if (!string.IsNullOrEmpty(request.Oid))
        {
            await _cardTopupRequestRepository.AddAsync(new CardTopupRequest
            {
                MdStatus = !string.IsNullOrEmpty(request.MdStatus) ? int.Parse(request.MdStatus) : null,
                OrderId = request.Oid,
                Status = CardTopupRequestStatus.Pending,
                TransactionDate = request.GetTransactionDate(),
                PaymentProviderType = PaymentProviderType.Masterpass,
                CreateDate = DateTime.Now,
                CreatedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString(),
            });
        }
    }

    public async Task<ValidateThreedSecureResponse> ValidateThreedSecureAsync(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentNullException(nameof(orderId));

        var cardTopupRequest = await GetCardTopupRequestByOrderId(orderId);

        if (cardTopupRequest == null)
        {
            return new ValidateThreedSecureResponse
            {
                CardTopupRequestId = null,
                IsValid = null
            };
        }

        return new ValidateThreedSecureResponse
        {
            CardTopupRequestId = cardTopupRequest.Id,
            IsValid = cardTopupRequest.MdStatus == 1
        };
    }
    public async Task<BaseResponse<AccountUnlinkResponse>> AccountUnlinkAccountAsync(AccountUnlinkRequest request)
    {
        var cacheKey = $"Token_{request.UserId}_{request.OrderNo}";
        var cachedResponse = _cacheService.Get<BaseResponse<GenerateAccessTokenResponse>>(cacheKey);

        if (cachedResponse == null || string.IsNullOrEmpty(cachedResponse.Result.AccessToken))
        {
            throw new MasterpassUnauthorizedAccessException();
        }

        AddAuthorizationHeader(cachedResponse.Result.AccessToken);
        request.MerchantId = _merchantId;

        var response = await SendRequestAsync<BaseResponse<AccountUnlinkResponse>>("account/api/Account/Unlink", request);

        if (response.Exception != null && !string.IsNullOrEmpty(request.OrderNo))
        {
            var cardTopupRequest = await GetCardTopupRequestByOrderId(request.OrderNo);

            if (cardTopupRequest != null)
            {
                cardTopupRequest.ErrorMessage = response.Exception.Message;
                cardTopupRequest.ErrorCode = response.Exception.Code;
                cardTopupRequest.UpdateDate = DateTime.Now;

                await UpdateCardTopupRequest(cardTopupRequest);
            }
        }

        return response;
    }

    public async Task<TopupProcessResponse> TopupProcessAsync(MasterpassTopupProcessRequest request, Wallet wallet, CardTopupRequest cardTopupRequest, ITopupService topupService)
    {
        _ = new BaseResponse<PaymentResponse>();
        BaseResponse<PaymentResponse> paymentResponse;
        var cardBin = await GetCardBinByNumberAsync(request.CardNumber, _pfServiceUrl);
        var cardType = cardBin.CardType == CardType.Unknown ? CardType.Credit : cardBin.CardType;
        var pricingProfile = await CalculatePricingAsync(new CalculatePricingRequest
        {
            Amount = request.Amount,
            CurrencyCode = request.Currency,
            SenderWalletType = WalletType.Individual,
            TransferType = TransferType.CreditCardTopup
        }, cardType);

        if (pricingProfile == null)
        {
            throw new NotFoundException(nameof(PricingProfileItem), pricingProfile);
        }

        cardTopupRequest.AccountKey = request.AccountKey;
        cardTopupRequest.Amount = pricingProfile.Amount;
        cardTopupRequest.Fee = pricingProfile.Fee;
        cardTopupRequest.CommissionTotal = pricingProfile.CommissionAmount;
        cardTopupRequest.BsmvTotal = pricingProfile.BsmvTotal;
        cardTopupRequest.CommissionRate = pricingProfile.CommissionRate;
        cardTopupRequest.CreatedBy = _contextProvider.CurrentContext.UserId ?? string.Empty;
        cardTopupRequest.Currency = request.Currency;

        cardTopupRequest.CardType = cardBin.CardType;
        cardTopupRequest.CardBrand = cardBin.CardBrand;
        cardTopupRequest.CardNumber = request.CardNumber;

        await UpdateCardTopupRequest(cardTopupRequest);

        var amount = Math.Round(cardTopupRequest.Amount + cardTopupRequest.Fee + cardTopupRequest.CommissionTotal + cardTopupRequest.BsmvTotal, 2);
        var formattedAmount = FormatAmount(amount);

        try
        {
            paymentResponse = await PaymentProcessAsync(new PaymentRequest
            {
                Amount = formattedAmount,
                AccountKey = request.AccountKey,
                AcquirerIcaNumber = request.AcquirerIcaNumber,
                InstallmentCount = request.InstallmentCount,
                MerchantId = _merchantId,
                OrderId = request.OrderId,
                RequestReferenceNo = request.RequestReferenceNo,
                TerminalGroupId = _terminalGroupId,
                Token = request.Token,
            }, cardTopupRequest, request.TransactionType);
        }
        catch (Exception exception) when (exception is TaskCanceledException || exception is TimeoutException)
        {
            LogErrorMessage(nameof(TopupProcessAsync), cardTopupRequest.OrderId, exception);
            await UpdateCardTopupRequest(cardTopupRequest, CardTopupRequestStatus.ProvisionTimeout);
            return new TopupProcessResponse { CardTopupRequestId = request.CardTopupRequestId, IsSuccess = false, ErrorMessage = _errorMessage };
        }
        catch (Exception exception)
        {
            LogErrorMessage(nameof(TopupProcessAsync), cardTopupRequest.OrderId, exception);
            await UpdateCardTopupRequest(cardTopupRequest, CardTopupRequestStatus.Failed);
            return new TopupProcessResponse { CardTopupRequestId = request.CardTopupRequestId, IsSuccess = false, ErrorMessage = _errorMessage };
        }

        if (paymentResponse?.Exception != null)
        {
            var validationErrors = paymentResponse.Exception.ValidationErrors?.Any() == true
                ? string.Join("\n", paymentResponse.Exception.ValidationErrors.Select(e => $"- Field: {e.Field}, Message: {e.Message}, Code: {e.Code}"))
                : null;

            var errorMessage = string.IsNullOrEmpty(validationErrors)
                ? paymentResponse.Exception.Message
                : $"{paymentResponse.Exception.Message}\n{validationErrors}";

            cardTopupRequest.ErrorCode = paymentResponse.Exception.Code;
            cardTopupRequest.ErrorMessage = errorMessage;

            await UpdateCardTopupRequest(cardTopupRequest, CardTopupRequestStatus.Failed);
            return new TopupProcessResponse
            {
                CardTopupRequestId = cardTopupRequest.Id,
                IsSuccess = false,
                ErrorCode = paymentResponse.Exception.Code,
                ErrorMessage = errorMessage
            };
        }

        try
        {
            var topupResult = await topupService.TopupProcessAsync(new TopupProcessCommand { BaseRequest = request }, wallet, cardTopupRequest, request.CardHolderName, amount);
            return new TopupProcessResponse { CardTopupRequestId = request.CardTopupRequestId, IsSuccess = true, TransactionId = topupResult.TransactionId };
        }
        catch (Exception exception)
        {
            LogErrorMessage(nameof(TopupProcessAsync), cardTopupRequest.OrderId, exception);
            await UpdateCardTopupRequest(cardTopupRequest, CardTopupRequestStatus.Failed);
            await RefundOrVoidProcessAsync(cardTopupRequest, CardTopupRequestStatus.Reversed, CardTopupRequestStatus.Returned, CardTopupRequestStatus.BankActionRequired, formattedAmount);
            return new TopupProcessResponse { CardTopupRequestId = request.CardTopupRequestId, IsSuccess = false, ErrorMessage = _errorMessage };
        }
    }

    public async Task RefundOrVoidProcessAsync(CardTopupRequest cardTopupRequest, CardTopupRequestStatus voidStatus, CardTopupRequestStatus refundStatus, CardTopupRequestStatus failedStatus, string formattedAmount)
    {
        var voidResponse = new BaseResponse<VoidResponse>();
        try
        {
            voidResponse = await VoidAsync(new VoidRequest
            {
                MerchantId = _merchantId,
                OrderId = cardTopupRequest.OrderId,
                TransactionDate = cardTopupRequest.TransactionDate.Value.ToString("yyyyMMdd"),
                TerminalGroupId = _terminalGroupId
            });
        }
        catch (Exception exception)
        {
            LogErrorMessage(nameof(RefundOrVoidProcessAsync), cardTopupRequest.OrderId, exception);
            await UpdateCardTopupRequest(cardTopupRequest, failedStatus);
        }

        if (!(voidResponse.StatusCode == 200))
        {
            var refundResponse = new BaseResponse<RefundResponse>();
            try
            {
                refundResponse = await RefundAsync(new RefundRequest
                {
                    MerchantId = _merchantId,
                    OrderId = cardTopupRequest.OrderId,
                    OrderDate = cardTopupRequest.TransactionDate.Value.ToString("yyyyMMdd"),
                    TerminalGroupId = _terminalGroupId,
                    Amount = formattedAmount
                });
            }
            catch (Exception exception)
            {
                LogErrorMessage(nameof(RefundOrVoidProcessAsync), cardTopupRequest.OrderId, exception);
                await UpdateCardTopupRequest(cardTopupRequest, failedStatus);
            }

            if (!(refundResponse.StatusCode == 200))
            {
                await UpdateCardTopupRequest(cardTopupRequest, failedStatus);
            }
            else
            {
                await UpdateCardTopupRequest(cardTopupRequest, refundStatus);
            }
        }
        else
        {
            await UpdateCardTopupRequest(cardTopupRequest, voidStatus);
        }
    }

    private async Task<CardBinDto> GetCardBinByNumberAsync(string cardNumber, Uri clientBaseAddress)
    {
        if (string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId))
        {
            var token = _applicationUserService.Token;

            if (!string.IsNullOrEmpty(token))
            {
                if (_client.DefaultRequestHeaders.Authorization == null)
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        var response = await GetAsync($"{clientBaseAddress}v1/CardBins/{cardNumber[..6]}");
        var responseString = await response.Content.ReadAsStringAsync();
        var cardBinResponse = System.Text.Json.JsonSerializer.Deserialize<CardBinDto>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return cardBinResponse;
    }

    public async Task<CardBinDto> GetCardBinByNumberAsync(string cardNumber)
    {
        return await GetCardBinByNumberAsync(cardNumber, _pfServiceUrl);
    }

    public async Task<TopupCancelResponse> TopupCancelAsync(CardTopupRequest cardTopupRequest, Wallet wallet, string description, decimal amount, IPaymentProviderService paymentProvider, IMasterpassService masterpassService, ITopupService topupService)
    {
        var tokenResponse = await GenerateAccessTokenAsync(new GenerateAccessTokenRequest
        {
            AccountKey = cardTopupRequest.AccountKey,
            OrderNo = cardTopupRequest.OrderId,
            UserId = wallet.Account.AccountUsers.First().UserId.ToString(),
            MerchantId = _merchantId,
            AuthenticationMethod = cardTopupRequest.AuthenticationMethod,
            Secure3dType = cardTopupRequest.Secure3dType
        });

        if (string.IsNullOrEmpty(tokenResponse.Result.AccessToken))
        {
            throw new MasterpassUnauthorizedAccessException();
        }

        AddAuthorizationHeader(tokenResponse.Result.AccessToken);

        if (!cardTopupRequest.TransactionDate.HasValue)
        {
            throw new ArgumentNullException(nameof(cardTopupRequest.TransactionDate), "Transaction date cannot be null.");
        }

        var voidResponse = new BaseResponse<VoidResponse>();
        try
        {
            voidResponse = await VoidAsync(new VoidRequest
            {
                MerchantId = _merchantId,
                OrderId = cardTopupRequest.OrderId,
                TransactionDate = cardTopupRequest.TransactionDate.Value.ToString("yyyyMMdd")
            });
        }
        catch (Exception exception)
        {
            LogErrorMessage(nameof(TopupCancelAsync), cardTopupRequest.OrderId, exception);
            await UpdateCardTopupRequest(cardTopupRequest, CardTopupRequestStatus.WalletActionRequired, wallet, description);
            return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = false };
        }

        if (voidResponse.StatusCode == 200)
        {
            try
            {
                await topupService.TopupReverseAsync(cardTopupRequest, CardTopupRequestStatus.Reversed, wallet);
                return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = true };
            }
            catch (Exception exception)
            {
                LogErrorMessage(nameof(TopupCancelAsync), cardTopupRequest.OrderId, exception);
                await UpdateCardTopupRequest(cardTopupRequest, CardTopupRequestStatus.WalletActionRequired, wallet, description);
                return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = false };
            }
        }
        else
        {
            var refundResponse = new BaseResponse<RefundResponse>();
            try
            {
                refundResponse = await RefundAsync(new RefundRequest
                {
                    MerchantId = _merchantId,
                    OrderId = cardTopupRequest.OrderId,
                    OrderDate = cardTopupRequest.TransactionDate.Value.ToString("yyyyMMdd"),
                    Amount = FormatAmount(amount),
                    TerminalGroupId = _terminalGroupId
                });
            }
            catch (Exception exception)
            {
                LogErrorMessage(nameof(TopupCancelAsync), cardTopupRequest.OrderId, exception);
                await UpdateCardTopupRequest(cardTopupRequest, CardTopupRequestStatus.WalletActionRequired, wallet, description);
                return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = false };
            }

            if (!(refundResponse.StatusCode == 200))
            {
                if (refundResponse?.Exception != null)
                {
                    var validationErrors = refundResponse.Exception.ValidationErrors?.Any() == true
                        ? string.Join("\n", refundResponse.Exception.ValidationErrors.Select(e => $"- Field: {e.Field}, Message: {e.Message}, Code: {e.Code}"))
                        : null;

                    var errorMessage = string.IsNullOrEmpty(validationErrors)
                        ? refundResponse.Exception.Message
                        : $"{refundResponse.Exception.Message}\n{validationErrors}";

                    cardTopupRequest.ErrorCode = refundResponse.Exception.Code;
                    cardTopupRequest.ErrorMessage = errorMessage;
                }

                await UpdateCardTopupRequest(cardTopupRequest, CardTopupRequestStatus.WalletActionRequired, wallet, description);
                return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = false, ErrorCode = cardTopupRequest.ErrorCode, ErrorMessage = cardTopupRequest.ErrorMessage };
            }
            else
            {
                try
                {
                    await topupService.TopupReverseAsync(cardTopupRequest, CardTopupRequestStatus.Returned, wallet);
                    return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = true };
                }
                catch (Exception exception)
                {
                    LogErrorMessage(nameof(TopupCancelAsync), cardTopupRequest.OrderId, exception);
                    await UpdateCardTopupRequest(cardTopupRequest, CardTopupRequestStatus.WalletActionRequired, wallet, description);
                    return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = false };
                }
            }
        }
    }

    private void LogErrorMessage(string methodName, string orderId, Exception exception)
    {
        _logger.LogError($"Topup {methodName} failed for OrderId: {orderId}, Error: {exception.Message}");
    }

    private async Task<CalculatePricingResponse> CalculatePricingAsync(CalculatePricingRequest request, CardType cardType)
    {
        var profile = await _pricingProfileRepository
                .GetAll()
                .Include(s => s.Bank)
                .Include(s => s.Currency)
                .Include(s => s.PricingProfileItems)
            .FirstOrDefaultAsync(s =>
                s.Status == PricingProfileStatus.InUse &&
                s.TransferType == request.TransferType &&
                s.BankCode == request.BankCode &&
                s.CurrencyCode == request.CurrencyCode.ToUpper() &&
                s.CardType == cardType);

        if (profile is null)
        {
            return new CalculatePricingResponse { Amount = request.Amount.ToDecimal2() };
        }

        var item = profile.PricingProfileItems.FirstOrDefault(s =>
            s.MinAmount <= request.Amount &&
            s.MaxAmount >= request.Amount &&
            s.WalletType == request.SenderWalletType);

        if (item is null)
        {
            return new CalculatePricingResponse { Amount = request.Amount.ToDecimal2() };
        }

        var amount = request.Amount.ToDecimal2();
        var fee = item.Fee.ToDecimal2();
        var commissionRate = item.CommissionRate.ToDecimal2();
        var commissionAmount = (amount * (commissionRate / 100m)).ToDecimal2();
        var bsmvRateParameter = await _parameterService.GetParameterAsync("Comission", "BsmvRate");
        var bsmvRate = Convert.ToDecimal(bsmvRateParameter?.ParameterValue);

        var response = new CalculatePricingResponse
        {
            Amount = amount,
            CommissionRate = commissionRate,
            CommissionAmount = commissionAmount,
            Fee = fee,
            BsmvRate = bsmvRate,
            BsmvTotal = ((fee + commissionAmount) * (bsmvRate / 100m)).ToDecimal2(),
        };

        if (response.BsmvTotal == 0 && response.CommissionAmount > 0)
        {
            response.BsmvTotal = 0.01m;
        }

        return response;
    }

    private async Task<BaseResponse<PaymentResponse>> PaymentProcessAsync(PaymentRequest request, CardTopupRequest cardTopupRequest, MasterpassTransactionType transactionType)
    {
        var cacheKey = $"Token_{_contextProvider.CurrentContext.UserId}_{request.OrderId}";
        var cachedResponse = _cacheService.Get<BaseResponse<GenerateAccessTokenResponse>>(cacheKey);

        if (cachedResponse == null || string.IsNullOrEmpty(cachedResponse.Result.AccessToken))
        {
            throw new MasterpassUnauthorizedAccessException();
        }

        cardTopupRequest.AuthenticationMethod = cachedResponse.Result.AuthenticationMethod;
        cardTopupRequest.Secure3dType = cachedResponse.Result.Secure3dType;

        await UpdateCardTopupRequest(cardTopupRequest);

        AddAuthorizationHeader(cachedResponse.Result.AccessToken);

        var paymentResponse = await PaymentAsync(new PaymentRequest
        {
            AccountKey = request.AccountKey,
            AcquirerIcaNumber = request.AcquirerIcaNumber,
            Amount = request.Amount,
            InstallmentCount = request.InstallmentCount,
            MerchantId = _merchantId,
            OrderId = request.OrderId,
            RequestReferenceNo = request.RequestReferenceNo,
            TerminalGroupId = request.TerminalGroupId,
            Token = request.Token
        }, transactionType);

        return paymentResponse;
    }

    private async Task<BaseResponse<PaymentResponse>> PaymentAsync(PaymentRequest request, MasterpassTransactionType transactionType)
    {
        if (transactionType == MasterpassTransactionType.REGISTER_AND_PURCHASE_3D)
        {
            return await SendRequestAsync<BaseResponse<PaymentResponse>>("payment/api/RegisterAndPurchase/complete", request);
        }

        return await SendRequestAsync<BaseResponse<PaymentResponse>>("payment/api/Payment/complete", request);
    }

    public async Task<BaseResponse<VoidResponse>> VoidAsync(VoidRequest request)
    {
        return await SendRequestAsync<BaseResponse<VoidResponse>>("payment/api/Payment/void", request);
    }

    public async Task<BaseResponse<RefundResponse>> RefundAsync(RefundRequest request)
    {
        return await SendRequestAsync<BaseResponse<RefundResponse>>("payment/api/Payment/refund", request);
    }

    private async Task<TResponse> SendRequestAsync<TResponse>(string endpoint, object request)
    {
        var jsonRequest = JsonConvert.SerializeObject(request);
        var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(endpoint, httpContent);

        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<TResponse>(jsonResponse);
    }

    private void AddAuthorizationHeader(string token)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
    }

    private async Task<CardTopupRequest> GetCardTopupRequestAsync(Guid cardTopupRequestId)
        => await _cardTopupRequestRepository.GetByIdAsync(cardTopupRequestId);

    private async Task<CardTopupRequest> GetCardTopupRequestByOrderId(string orderId)
        => await _cardTopupRequestRepository
                    .GetAll()
                    .FirstOrDefaultAsync(c => c.OrderId == orderId);

    private async Task UpdateCardTopupRequest(CardTopupRequest cardTopupRequest,
    CardTopupRequestStatus? status = null, Wallet wallet = null, string description = null)
    {
        if (status.HasValue)
        {
            cardTopupRequest.Status = status.Value;
        }

        if (wallet != null)
        {
            cardTopupRequest.WalletId = wallet.Id;
            cardTopupRequest.WalletNumber = wallet.WalletNumber;
            cardTopupRequest.Name = wallet.Account.Name;
        }

        if (!string.IsNullOrEmpty(description))
        {
            cardTopupRequest.CancelDescription = description;
        }

        cardTopupRequest.UpdateDate = DateTime.Now;
        cardTopupRequest.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();

        await _cardTopupRequestRepository.UpdateAsync(cardTopupRequest);
    }

    public string FormatAmount(decimal amount)
    {
        var formattedAmount = (int)(amount * 100);
        return formattedAmount.ToString();
    }
}