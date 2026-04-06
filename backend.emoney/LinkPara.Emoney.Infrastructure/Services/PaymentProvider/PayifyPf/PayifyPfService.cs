using LinkPara.Backend.Emoney.PaymentProvider.Commons;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Application.Features.Topups.Commands.TopupProcess;
using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.BusModels.Commands.Scheduler.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

namespace LinkPara.Emoney.Infrastructure.Services.PaymentProvider.PayifyPf;

public class PayifyPfService : HttpClientBase, IPaymentProviderService
{
    private readonly HttpClient _client;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISignatureGenerator _signatureGenerator;
    private readonly IPaymentApiLog _paymentRequestResponseLog;
    private readonly IGenericRepository<CardTopupRequest> _cardTopupRequestRepository;
    private readonly ILogger<PayifyPfService> _logger;
    private readonly IGenericRepository<PricingProfile> _pricingProfileRepository;
    private readonly IParameterService _parameterService;
    private readonly IContextProvider _contextProvider;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IStringLocalizer _localizer;
    private readonly IMasterpassService _masterpassService;

    private readonly string _errorMessage;
    private readonly string _merchantNumber;
    private readonly string _publicKey;
    private readonly string _callBackUrl;

    public PayifyPfService(HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        ISignatureGenerator signatureGenerator,
        IVaultClient vaultClient,
        IPaymentApiLog paymentRequestResponseLog,
        IGenericRepository<CardTopupRequest> cardTopupRequestRepository,
        IContextProvider contextProvider,
        ILogger<PayifyPfService> logger,
        IGenericRepository<PricingProfile> pricingProfileRepository,
        IParameterService parameterService,
        IApplicationUserService applicationUserService,
        IStringLocalizerFactory factory)
        : base(client, httpContextAccessor)
    {
        _client = client;
        _httpContextAccessor = httpContextAccessor;
        _signatureGenerator = signatureGenerator;
        _paymentRequestResponseLog = paymentRequestResponseLog;
        var pfServiceUrl = vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "PFApiGateway");
        client.BaseAddress = new Uri(pfServiceUrl);
        _merchantNumber = vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "MerchantNumber");
        _publicKey = vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "PublicKey");
        _cardTopupRequestRepository = cardTopupRequestRepository;
        _contextProvider = contextProvider;
        _callBackUrl = vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "ApiGateway");
        _logger = logger;
        _pricingProfileRepository = pricingProfileRepository;
        _parameterService = parameterService;
        _applicationUserService = applicationUserService;
        _localizer = factory.Create("ErrorMessages", "LinkPara.Emoney.API");
        _errorMessage = _localizer["UnexpectedErrorMessage"];
    }

    public async Task<MerchantApiKeyDto> GetApiKeysAsync(string publicKey, Uri clientBaseAddress)
    {
        var publicKeyBytes = System.Text.Encoding.UTF8.GetBytes(publicKey);
        var publicKeyEncoded = Convert.ToBase64String(publicKeyBytes);

        if (string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId))
        {
            var token = _applicationUserService.Token;

            if (!string.IsNullOrEmpty(token) && (_client.DefaultRequestHeaders.Authorization == null))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        var response = await GetAsync($"{clientBaseAddress}v1/Merchants/apiKeys?PublicKeyEncoded={publicKeyEncoded}");
        var responseString = await response.Content.ReadAsStringAsync();
        var apiKeys = System.Text.Json.JsonSerializer.Deserialize<MerchantApiKeyDto>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return apiKeys;
    }

    public async Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionRequest request)
    {
        var cardTopupRequest = await GetCardTopupRequestAsync(request.CardTopupRequestId);

        if (cardTopupRequest == null)
        {
            return new GetThreeDSessionResponse { IsSucceed = false, ErrorMessage = _errorMessage };
        }

        var header = await _signatureGenerator.GenerateSignatureAsync(new SignatureDataRequest
        {
            PublicKey = _publicKey,
            MerchantNumber = _merchantNumber,
            ConversationId = cardTopupRequest.ConversationId
        });

        request.Signature = header.Signature;
        request.MerchantNumber = _merchantNumber;
        request.PublicKey = header.PublicKey;
        request.Nonce = header.Nonce;
        request.ConversationId = header.ConversationId;

        var cardType = cardTopupRequest.CardType == CardType.Unknown ? CardType.Credit : cardTopupRequest.CardType;

        var pricingProfile = await CalculatePricingAsync(new CalculatePricingRequest
        {
            Amount = request.Amount,
            CurrencyCode = request.Currency,
            SenderWalletType = WalletType.Individual,
            TransferType = TransferType.CreditCardTopup
        }, cardType);

        request.Amount = Math.Round(pricingProfile.TotalAmount, 2);

        AddHeaders<GetThreeDSessionRequest>(request);

        var response = await PostAsJsonAsync("v1/ThreeDS/GetThreedSession", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var threeDSessionResponse = JsonSerializer.Deserialize<GetThreeDSessionResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        cardTopupRequest.ThreedSessionId = threeDSessionResponse.ThreeDSessionId;
        cardTopupRequest.Currency = request.Currency;
        cardTopupRequest.ErrorCode = threeDSessionResponse.ErrorCode;
        cardTopupRequest.ErrorMessage = threeDSessionResponse.ErrorMessage;
        cardTopupRequest.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();
        cardTopupRequest.UpdateDate = DateTime.Now;

        if (pricingProfile?.Amount != 0)
        {
            cardTopupRequest.Amount = pricingProfile.Amount;
            cardTopupRequest.Fee = pricingProfile.Fee;
            cardTopupRequest.CommissionTotal = pricingProfile.CommissionAmount;
            cardTopupRequest.BsmvTotal = pricingProfile.BsmvTotal;
            cardTopupRequest.CommissionRate = pricingProfile.CommissionRate;
        }

        await _cardTopupRequestRepository.UpdateAsync(cardTopupRequest);
        threeDSessionResponse.CardTopupRequestId = cardTopupRequest.Id;

        return threeDSessionResponse;
    }

    public async Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync(
        GetThreeDSessionResultRequest request)
    {
        var cardTopupRequest = await GetCardTopupRequestAsync(request.CardTopupRequestId);

        if (cardTopupRequest == null)
        {
            return new GetThreeDSessionResultResponse { IsSucceed = false, ErrorMessage = _errorMessage };
        }

        var header = await _signatureGenerator.GenerateSignatureAsync(new SignatureDataRequest
        {
            PublicKey = _publicKey,
            MerchantNumber = _merchantNumber,
            ConversationId = cardTopupRequest.ConversationId
        });

        request.Signature = header.Signature;
        request.MerchantNumber = _merchantNumber;
        request.PublicKey = header.PublicKey;
        request.Nonce = header.Nonce;
        request.ConversationId = header.ConversationId;

        AddHeaders<GetThreeDSessionResultRequest>(request);
        var response = await PostAsJsonAsync("v1/ThreeDS/GetThreedSessionResult", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var threeDSessionResponse = JsonSerializer.Deserialize<GetThreeDSessionResultResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        threeDSessionResponse.CardTopupRequestId = cardTopupRequest.Id;
        cardTopupRequest.ErrorCode = threeDSessionResponse.ErrorCode;
        cardTopupRequest.ErrorMessage = threeDSessionResponse.ErrorMessage;
        cardTopupRequest.UpdateDate = DateTime.Now;
        cardTopupRequest.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();

        await _cardTopupRequestRepository.UpdateAsync(cardTopupRequest);

        return threeDSessionResponse;
    }

    public async Task<Init3dsResponse> Init3dsAsync(Init3dsRequest request)
    {
        try
        {
            var cardTopupRequest = await GetCardTopupRequestAsync(request.CardTopupRequestId);

            if (cardTopupRequest == null)
            {
                return new Init3dsResponse { IsSucceed = false, ErrorMessage = _errorMessage };
            }

            var header = await _signatureGenerator.GenerateSignatureAsync(new SignatureDataRequest
            {
                PublicKey = _publicKey,
                MerchantNumber = _merchantNumber,
                ConversationId = cardTopupRequest.ConversationId
            });

            var clientIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "ThreeDSessionId", request.ThreeDSessionId },
                { "CallbackUrl", _callBackUrl },
                { "LanguageCode", request.LanguageCode },
                { "PublicKey", header.PublicKey },
                { "Nonce", header.Nonce },
                { "Signature", header.Signature },
                { "ConversationId", header.ConversationId },
                { "MerchantNumber", _merchantNumber },
                { "ClientIpAddress", clientIpAddress },
                { "CardHolderName", request.CardHolderName },
                { "IsTopUpPayment", "true" }
            });

            var response = await _client.PostAsync("v1/ThreeDS/init3ds", content);

            var responseString = await response.Content.ReadAsStringAsync();
            var init3dResponse = System.Text.Json.JsonSerializer.Deserialize<Init3dsResponse>(responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            cardTopupRequest.Status = CardTopupRequestStatus.Pending;
            cardTopupRequest.ConversationId = header.ConversationId;
            cardTopupRequest.UpdateDate = DateTime.Now;
            cardTopupRequest.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();

            if (init3dResponse.IsSucceed)
            {
                cardTopupRequest.ErrorMessage = init3dResponse.ErrorMessage;
                cardTopupRequest.ErrorCode = init3dResponse.ErrorCode;
            }

            await _cardTopupRequestRepository.UpdateAsync(cardTopupRequest);
            init3dResponse.CallBackUrl = _callBackUrl;
            init3dResponse.CardTopupRequestId = cardTopupRequest.Id;
            init3dResponse.ThreedSessionId = cardTopupRequest.ThreedSessionId;
            init3dResponse.HtmlContent = init3dResponse.HtmlContent ?? string.Empty;

            return init3dResponse;
        }
        catch (Exception exception)
        {
            _logger.LogError("EMoney Init3d exception {exception}", exception);
        }
        return default;
    }

    public async Task<PaymentProviderProvisionResponse> ProvisionAsync(PaymentProviderProvisionRequest request)
    {
        var cardTopupRequest = await GetCardTopupRequestAsync(request.CardTopupRequestId);

        if (cardTopupRequest == null)
        {
            return new PaymentProviderProvisionResponse { IsSucceed = false, ErrorMessage = _errorMessage };
        }

        var header = await _signatureGenerator.GenerateSignatureAsync(new SignatureDataRequest
        {
            PublicKey = _publicKey,
            MerchantNumber = _merchantNumber,
            ConversationId = cardTopupRequest.ConversationId
        });

        request.Signature = header.Signature;
        request.MerchantNumber = _merchantNumber;
        request.PublicKey = header.PublicKey;
        request.Nonce = header.Nonce;
        request.ConversationId = header.ConversationId;

        AddHeaders<PaymentProviderProvisionRequest>(request);

        var response = await PostAsJsonAsync("v1/Payments/provision", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var provisionResponse = System.Text.Json.JsonSerializer.Deserialize<PaymentProviderProvisionResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        await PaymentApiLogAsync(header.MerchantId, request, response,
          provisionResponse.ErrorCode, provisionResponse.ErrorMessage, PaymentLogType.Provision);

        cardTopupRequest.ProvisionNumber = provisionResponse.ProvisionNumber;
        cardTopupRequest.OrderId = provisionResponse.OrderId;
        cardTopupRequest.Status = CardTopupRequestStatus.Pending;
        cardTopupRequest.ErrorCode = provisionResponse.ErrorCode;
        cardTopupRequest.ErrorMessage = provisionResponse.ErrorMessage;
        cardTopupRequest.UpdateDate = DateTime.Now;
        cardTopupRequest.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();

        await _cardTopupRequestRepository.UpdateAsync(cardTopupRequest);
        provisionResponse.CardTopupRequestId = cardTopupRequest.Id;

        provisionResponse.SignatureDataResponse = header;

        return provisionResponse;
    }

    public async Task<ReturnResponse> ReturnAsync(ReturnRequest request)
    {
        request.Signature = request.SignatureDataResponse?.Signature;
        request.MerchantNumber = _merchantNumber;
        request.PublicKey = request.SignatureDataResponse?.PublicKey;
        request.Nonce = request.SignatureDataResponse?.Nonce;
        request.ConversationId = request.SignatureDataResponse?.ConversationId;

        AddHeaders<ReturnRequest>(request);

        var response = await PostAsJsonAsync("v1/Payments/return", request);

        var responseString = await response.Content.ReadAsStringAsync();

        var returnResponse = JsonSerializer.Deserialize<ReturnResponse>(responseString,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        await PaymentApiLogAsync(request.SignatureDataResponse.MerchantId, request, response,
          returnResponse.ErrorCode, returnResponse.ErrorMessage, PaymentLogType.Return);

        return returnResponse;
    }

    public async Task<ReverseResponse> ReverseAsync(ReverseRequest request)
    {
        var header = new SignatureDataResponse();

        if (request.SignatureDataResponse != null)
        {
            header = request.SignatureDataResponse;
        }
        else
        {
            header = await _signatureGenerator.GenerateSignatureAsync(new SignatureDataRequest
            {
                PublicKey = _publicKey,
                MerchantNumber = _merchantNumber,
                ConversationId = request.ConversationId
            });
        }

        request.Signature = header.Signature;
        request.MerchantNumber = _merchantNumber;
        request.PublicKey = header.PublicKey;
        request.Nonce = header.Nonce;
        request.ConversationId = header.ConversationId;

        AddHeaders<ReverseRequest>(request);

        var response = await PostAsJsonAsync("v1/Payments/reverse", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var reverseResponse = JsonSerializer.Deserialize<ReverseResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        reverseResponse.SignatureDataResponse = header;

        return reverseResponse;
    }

    public async Task<CardTokenDto> GenerateCardTokenAsync(GenerateCardTokenRequest request)
    {
        var header = await _signatureGenerator.GenerateSignatureAsync(new SignatureDataRequest
        {
            PublicKey = _publicKey,
            MerchantNumber = _merchantNumber
        });

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "CardNumber", request.CardNumber },
                { "ExpireMonth", request.ExpireMonth },
                { "ExpireYear", request.ExpireYear },
                { "Cvv", request.Cvv },
                { "PublicKey", header.PublicKey },
                { "Nonce", header.Nonce },
                { "Signature", header.Signature },
                { "ConversationId", header.ConversationId },
                { "MerchantNumber", _merchantNumber }
            });

            var response = await _client.PostAsync("v1/Tokens", content);

            if (!response.IsSuccessStatusCode)
            {
                await HandleExceptionAsync(response);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var cardToken = JsonSerializer.Deserialize<CardTokenDto>(responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var informationRequest = new GetBinInformationRequest { CardToken = cardToken.CardToken };

            var binInformation = await GetBinInformationAsync(informationRequest, header);

            var cardTopupRequest = await _cardTopupRequestRepository.AddAsync(new CardTopupRequest
            {
                CardNumber = MaskedCardNumber.GetMaskedCardNumber(request.CardNumber),
                Status = CardTopupRequestStatus.Pending,
                CardBrand = binInformation.CardBrand,
                CardType = binInformation.CardType,
                ErrorCode = cardToken.ErrorCode,
                ErrorMessage = cardToken.ErrorMessage,
                CreateDate = DateTime.Now,
                CreatedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString(),
                ConversationId = header.ConversationId,
                PaymentProviderType = request.PaymentProviderType,
                BankCode = binInformation.BankCode,
                BankName = binInformation.BankName,
            });

            if (cardTopupRequest.Id != Guid.Empty)
            {
                cardToken.CardTopupRequestId = cardTopupRequest.Id;
            }

            return cardToken;
        }
        catch (NotFoundException)
        {
            return new CardTokenDto()
            {
                ErrorCode = ((int)HttpStatusCode.BadRequest).ToString(),
                ConversationId = header.ConversationId,
                ErrorMessage = "InvalidPublicKey",
                IsSucceed = false
            };
        }
    }

    public async Task<InquireResponseModel> InquireAsync(InquireRequestModel request)
    {
        var cardTopupRequest = await GetCardTopupRequestAsync(request.CardTopupRequestId);

        if (cardTopupRequest == null)
        {
            return new InquireResponseModel { IsSucceed = false, ErrorMessage = _errorMessage };
        }

        var header = await _signatureGenerator.GenerateSignatureAsync(new SignatureDataRequest
        {
            PublicKey = _publicKey,
            MerchantNumber = _merchantNumber,
            ConversationId = cardTopupRequest.ConversationId
        });

        request.Signature = header.Signature;
        request.MerchantNumber = _merchantNumber;
        request.PublicKey = header.PublicKey;
        request.Nonce = header.Nonce;
        request.ConversationId = header.ConversationId;

        AddHeaders<InquireRequestModel>(request);

        var response = await PostAsJsonAsync("v1/Payments/inquire", request);
        var responseString = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        var inquireResponse = JsonSerializer.Deserialize<InquireResponseModel>(responseString, options);

        return inquireResponse;
    }

    public async Task<TopupProcessResponse> TopupProcessAsync(TopupProcessRequest request, Wallet wallet, CardTopupRequest cardTopupRequest, ITopupService topupService)
    {
        var provisionResponse = new PaymentProviderProvisionResponse();

        var amount = Math.Round(cardTopupRequest.Amount + cardTopupRequest.Fee + cardTopupRequest.CommissionTotal + cardTopupRequest.BsmvTotal, 2);

        try
        {
            provisionResponse = await ProvisionAsync(new PaymentProviderProvisionRequest
            {
                Amount = amount,
                CardToken = request.CardToken,
                ThreeDSessionId = cardTopupRequest.ThreedSessionId,
                CardTopupRequestId = request.CardTopupRequestId,
                CardHolderName = request.CardHolderName,
                PaymentType = PaymentType.Auth,
                Currency = request.Currency,
            });
        }
        catch (Exception exception) when (exception is TaskCanceledException || exception is TimeoutException)
        {
            LogErrorMessage(nameof(TopupProcessAsync), cardTopupRequest.OrderId, exception);
            await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.ProvisionTimeout);
            return new TopupProcessResponse { CardTopupRequestId = request.CardTopupRequestId, IsSuccess = false, ErrorMessage = _errorMessage };
        }
        catch (Exception exception)
        {
            LogErrorMessage(nameof(TopupProcessAsync), cardTopupRequest.OrderId, exception);
            await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.Failed);
            return new TopupProcessResponse { CardTopupRequestId = request.CardTopupRequestId, IsSuccess = false, ErrorMessage = _errorMessage };
        }

        if (!provisionResponse.IsSucceed)
        {
            await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.Failed);
            return new TopupProcessResponse
            {
                CardTopupRequestId = request.CardTopupRequestId,
                IsSuccess = false,
                ErrorCode = provisionResponse.ErrorCode,
                ErrorMessage = provisionResponse.ErrorMessage
            };
        }
        try
        {
            cardTopupRequest.TransactionDate = DateTime.Now;
            var topupResult = await topupService.TopupProcessAsync(new TopupProcessCommand { BaseRequest = request }, wallet, cardTopupRequest, request.CardHolderName, amount);
            return new TopupProcessResponse { CardTopupRequestId = request.CardTopupRequestId, IsSuccess = true, TransactionId = topupResult.TransactionId };
        }
        catch (Exception exception)
        {
            LogErrorMessage(nameof(TopupProcessAsync), cardTopupRequest.OrderId, exception);
            await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.Failed);
            await ReturnOrReverseProcessAsync(provisionResponse, amount, cardTopupRequest);
            return new TopupProcessResponse { CardTopupRequestId = request.CardTopupRequestId, IsSuccess = false, ErrorMessage = _errorMessage };
        }
    }

    private async Task ReturnOrReverseProcessAsync(PaymentProviderProvisionResponse provisionResponse, decimal amount, CardTopupRequest cardTopupRequest)
    {
        var reverseResponse = new ReverseResponse();
        try
        {
            reverseResponse = await ReverseAsync(new ReverseRequest
            {
                OrderId = provisionResponse?.OrderId,
                SignatureDataResponse = provisionResponse?.SignatureDataResponse,
            });
        }
        catch (Exception exception)
        {
            await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.BankActionRequired);
            _logger.LogError("Topup process command error: {exception}", exception);
        }

        if (!reverseResponse.IsSucceed)
        {
            var returnResponse = new ReturnResponse();
            try
            {
                returnResponse = await ReturnAsync(new ReturnRequest
                {
                    OrderId = provisionResponse?.OrderId,
                    Amount = amount,
                    SignatureDataResponse = reverseResponse.SignatureDataResponse,
                });
            }
            catch (Exception exception)
            {
                await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.BankActionRequired);
                _logger.LogError("Topup process command error: {exception}", exception);
            }

            if (!returnResponse.IsSucceed)
            {
                await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.BankActionRequired);
            }
            else if (returnResponse.ReturnApprovalStatus != ReturnApprovalStatus.PendingApproval && returnResponse.IsSucceed)
            {
                await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.Returned);
            }
        }
        else
        {
            await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.Reversed);
        }
    }

    private async Task UpdateCardTopupStatusAsync(CardTopupRequest request, CardTopupRequestStatus status)
    {
        if (request != null)
        {
            request.Status = status;
            request.UpdateDate = DateTime.Now;
            request.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();

            await _cardTopupRequestRepository.UpdateAsync(request);
        }
    }

    private async Task<GetBinInformationResponse> GetBinInformationAsync(GetBinInformationRequest request, SignatureDataResponse signatureDataResponse)
    {
        request.PublicKey = signatureDataResponse.PublicKey;
        request.Nonce = signatureDataResponse.Nonce;
        request.Signature = signatureDataResponse.Signature;
        request.ConversationId = signatureDataResponse.ConversationId;
        request.MerchantNumber = _merchantNumber;

        AddHeaders<GetBinInformationRequest>(request);

        var response = await _client.PostAsJsonAsync("v1/Payments/bin-information", request);

        var responseString = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        var binInfo = JsonSerializer.Deserialize<GetBinInformationResponse>(responseString, options);

        await PaymentApiLogAsync(signatureDataResponse.MerchantId, request, response,
            binInfo.ErrorCode, binInfo.ErrorMessage, PaymentLogType.BinInformation);

        return binInfo;
    }

    private void AddHeaders<T>(T request) where T : RequestHeaderInfo
    {
        _client.DefaultRequestHeaders.Clear();

        var ipAddress = !string.IsNullOrEmpty(_httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString())
            ? _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString()
            : GetLocalIpAddress();

        _client.DefaultRequestHeaders.Add("PublicKey", request.PublicKey);
        _client.DefaultRequestHeaders.Add("Nonce", request.Nonce);
        _client.DefaultRequestHeaders.Add("Signature", request.Signature);
        _client.DefaultRequestHeaders.Add("ConversationId", request.ConversationId);
        _client.DefaultRequestHeaders.Add("ClientIpAddress", ipAddress);
        _client.DefaultRequestHeaders.Add("MerchantNumber", request.MerchantNumber);
        _client.DefaultRequestHeaders.Add("IsTopUpPayment", "true");
    }

    private static string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var ipAddress = Array.Find(host.AddressList, ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

        return ipAddress?.ToString() ?? string.Empty;
    }

    private async Task PaymentApiLogAsync(Guid merchantId, object request, object response, string errorCode,
    string errorMessage, PaymentLogType logType)
    {
        await _paymentRequestResponseLog.SaveApiLogAsync(new PaymentApiLog
        {
            MerchantId = merchantId,
            Request = JsonConvert.SerializeObject(request),
            Response = JsonConvert.SerializeObject(response),
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            PaymentType = logType
        });
    }

    private async Task<CardTopupRequest> GetCardTopupRequestAsync(Guid cardTopupRequestId)
    {
        return await _cardTopupRequestRepository.GetByIdAsync(cardTopupRequestId);
    }

    private async Task<CalculatePricingResponse> CalculatePricingAsync(CalculatePricingRequest request, CardType cardType)
    {
        var profile = await _pricingProfileRepository
                .GetAll()
                .Include(s => s.Bank)
                .Include(s => s.Currency)
                .Include(s => s.PricingProfileItems)
            .FirstOrDefaultAsync(s =>
                s.Status == Domain.Enums.PricingProfileStatus.InUse &&
                s.TransferType == request.TransferType &&
                s.BankCode == request.BankCode &&
                s.CurrencyCode == request.CurrencyCode.ToUpper() &&
                s.CardType == cardType);

        if (profile is null)
        {
            return new CalculatePricingResponse { Amount = request.Amount.ToDecimal2() };
        }

        var item = profile.PricingProfileItems.Find(s => s.MinAmount <= request.Amount &&
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

    public async Task<TopupCancelResponse> TopupCancelAsync(CardTopupRequest cardTopupRequest, Wallet wallet, string description, decimal amount, IPaymentProviderService paymentProvider, IMasterpassService masterpassService, ITopupService topupService)
    {
        var reverseResponse = new ReverseResponse();
        try
        {
            reverseResponse = await paymentProvider.ReverseAsync(new ReverseRequest
            {
                OrderId = cardTopupRequest.OrderId,
                ConversationId = cardTopupRequest.ConversationId,
            });
        }
        catch (Exception exception)
        {
            LogErrorMessage(nameof(TopupCancelAsync), cardTopupRequest.OrderId, exception);
            await UpdateCardTopupRequest(cardTopupRequest, wallet, CardTopupRequestStatus.WalletActionRequired, description);
            return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = false };
        }

        if (reverseResponse.IsSucceed)
        {
            try
            {
                await topupService.TopupReverseAsync(cardTopupRequest, CardTopupRequestStatus.Reversed, wallet);
                return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = true };
            }
            catch (Exception exception)
            {
                LogErrorMessage(nameof(TopupCancelAsync), cardTopupRequest.OrderId, exception);
                await UpdateCardTopupRequest(cardTopupRequest, wallet, CardTopupRequestStatus.WalletActionRequired, description);
                return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = false };
            }
        }
        else
        {
            var returnResponse = new ReturnResponse();
            try
            {
                returnResponse = await paymentProvider.ReturnAsync(new ReturnRequest
                {
                    Amount = amount,
                    OrderId = cardTopupRequest.OrderId,
                    SignatureDataResponse = reverseResponse.SignatureDataResponse,
                });
            }
            catch (Exception exception)
            {
                LogErrorMessage(nameof(TopupCancelAsync), cardTopupRequest.OrderId, exception);
                await UpdateCardTopupRequest(cardTopupRequest, wallet, CardTopupRequestStatus.WalletActionRequired, description);
                return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = false };
            }

            if (!returnResponse.IsSucceed)
            {
                await UpdateCardTopupRequest(cardTopupRequest, wallet, CardTopupRequestStatus.WalletActionRequired, description);
                return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = false };
            }
            else if (returnResponse.ReturnApprovalStatus != ReturnApprovalStatus.PendingApproval && returnResponse.IsSucceed)
            {
                try
                {
                    await topupService.TopupReverseAsync(cardTopupRequest, CardTopupRequestStatus.Returned, wallet);
                    return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = true };
                }
                catch (Exception exception)
                {
                    LogErrorMessage(nameof(TopupCancelAsync), cardTopupRequest.OrderId, exception);
                    await UpdateCardTopupRequest(cardTopupRequest, wallet, CardTopupRequestStatus.WalletActionRequired, description);
                    return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = false };
                }
            }
            return new TopupCancelResponse { CardTopupRequestId = cardTopupRequest.Id, IsSuccess = false };
        }
    }

    private void LogErrorMessage(string methodName, string orderId, Exception exception)
    {
        _logger.LogError("Topup {methodName} failed for OrderId: {orderId}, Error: {exceptionMessage}",
            methodName, orderId, exception.Message);
    }

    private async Task UpdateCardTopupRequest(CardTopupRequest cardTopupRequest, Wallet wallet,
        CardTopupRequestStatus status, string description)
    {
        cardTopupRequest.Status = status;
        cardTopupRequest.WalletId = wallet.Id;
        cardTopupRequest.WalletNumber = wallet.WalletNumber;
        cardTopupRequest.Name = wallet.Account.Name;
        cardTopupRequest.CancelDescription = description;
        cardTopupRequest.UpdateDate = DateTime.Now;
        cardTopupRequest.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();

        await _cardTopupRequestRepository.UpdateAsync(cardTopupRequest);
    }
}
