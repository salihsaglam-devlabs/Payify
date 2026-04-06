using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Request;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Response;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IsbankInit3dModelRequest = LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Request.IsbankInit3dModelRequest;
using PosVerify3dModelResponse = LinkPara.PF.Application.Commons.Models.VposModels.Response.PosVerify3dModelResponse;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos;

public class IsbankVpos : VposBase, IVposApi
{
    private IsbankPosInfo _isbankPos;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;
    private readonly ILogger<IsbankVpos> _logger;
    private readonly IVaultClient _vaultClient;
    private readonly IGenericRepository<ThreeDVerification> _threeDVerificationRepository;
    private readonly string NonSecureUrl = "/api/isbank/virtual-pos/v1/payment";
    private readonly string NonSecurePreAuthUrl = "/api/isbank/virtual-pos/v1/preAuthorization";
    private readonly string SecureUrl = "/api/isbank/virtual-pos/v1/3dPayment";
    private readonly string SecurePreAuthUrl = "/api/isbank/virtual-pos/v1/3dPreauth";
    private readonly string RefundUrl = "/api/isbank/virtual-pos/v1/refund";
    private readonly string VoidUrl = "/api/isbank/virtual-pos/v1/cancellation";
    private readonly string ReversalUrl = "/api/isbank/virtual-pos/v1/reversal";
    private readonly string PointInquiryUrl = "/api/isbank/virtual-pos/v1/inquiry";
    private readonly string PostAuthUrl = "/api/isbank/virtual-pos/v1/preAuthorizationComplete";
    private readonly string PaymentDetailUrl = "/api/isbank/virtual-pos/v1/transactions";
    
    private readonly string ThreeDFullSecureStatus = "Y";
    private readonly string ThreeDHalfSecureStatus = "A";
    private static readonly List<string> SuccessCodes = ["00", "08", "11", "85"];

    public IsbankVpos(
        IParameterService parameterService, 
        IBus bus, 
        ILogger<IsbankVpos> logger, 
        IVaultClient vaultClient,
        IGenericRepository<ThreeDVerification> threeDVerificationRepository)
    {
        _parameterService = parameterService;
        _bus = bus;
        _logger = logger;
        _vaultClient = vaultClient;
        _threeDVerificationRepository = threeDVerificationRepository;
    } 
    
    protected override string FormatAmount(decimal amount)
    {
        return string.Empty;
    }

    protected override string FormatExpiryDate(string month, string year)
    {
        return string.Empty;
    }

    public async Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request)
    {
        var installCount = 1;
        if (request.Installment is not null)
        {
            installCount = request.Installment > 0 ? (int)request.Installment : 1;
        }

        var type = string.Empty;
        if (request.BonusAmount != null && request.BonusAmount != 0m) 
            type = "Sale";
        
        var isbankNonSecureRequest = new IsbankPaymentNonSecureRequest
        {
            MerchantNumber = _isbankPos.MerchantNumber,
            OrderNumber = request.OrderNumber,
            CardNumber = request.CardNumber,
            CardExpireMonth = Int32.Parse(request.ExpireMonth),
            CardExpireYear = Int32.Parse(request.ExpireYear),
            Cvv = request.Cvv2,
            Currency = request.Currency,
            Amount = request.Amount,
            Installment = installCount,
            Type = string.IsNullOrEmpty(type) ? null : type,
            CardHolderName = request.CardHolderName,
            PointAmount = request.BonusAmount ?? 0,
            SubMerchantId = request.SubMerchantId,
            SubMerchantMcc = request.SubMerchantMcc,
            SubMerchantCitizenId = request.SubMerchantTaxNumber,
            SubMerchantCity = request.SubMerchantCity,
            SubMerchantPostalCode = request.SubMerchantPostalCode,
            SubMerchantUrl = request.SubMerchantUrl
        }.BuildRequest();

        var sendUrl = request.AuthType == VposAuthType.Auth ?
            _isbankPos.BaseUrl + NonSecureUrl : _isbankPos.BaseUrl + NonSecurePreAuthUrl;
        
        var content = await SendIsbankRequestAsync(sendUrl,isbankNonSecureRequest);
        
        var parseResponse = new IsbankPaymentNonSecureResponse().Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (SuccessCodes.Contains(parseResponse.ResponseCode))
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderNumber;
            response.TrxDate = parseResponse.AuthorizationDateTime;
            response.RrnNumber = parseResponse.RetrievalReferenceNumber.ToString();
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
        }
        return response;
    }

    public async Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request)
    {
        if (request.AuthType == VposAuthType.PreAuth)
        {
            request.BonusAmount = 0m;
        }
        var isbankInitRequest = new IsbankInit3dModelRequest
        {
            MerchantNumber = _isbankPos.MerchantNumber,
            MerchantPassword = _isbankPos.MerchantPassword,
            OrderNumber = request.OrderNumber,
            CardNumber = request.CardNumber,
            CardExpireMonth = Int32.Parse(request.ExpireMonth),
            CardExpireYear = Int32.Parse(request.ExpireYear),
            CardHolderName = request.CardHolderName,
            OkUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            FailUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            Cvv = request.Cvv2,
            Currency = request.Currency,
            Amount = request.Amount,
            Installment = request.Installment > 0 ? request.Installment : 1,
            TransactionType = GetTransactionType(request.AuthType, request.Amount, request.BonusAmount ?? 0m),
            PointAmount = request.BonusAmount ?? 0m,
            ClientIp = request.ClientIp,
            InitUrl = _isbankPos.ThreeDSecureUrl,
            SubMerchantId = request.SubMerchantId,
            SubMerchantMcc = request.SubMerchantMcc,
            SubMerchantName = request.SubMerchantName,
            SubMerchantCitizenId = request.SubMerchantTaxNumber,
            SubMerchantCity = request.SubMerchantCity,
            SubMerchantPostalCode = request.SubMerchantPostalCode,
            SubMerchantUrl = request.SubMerchantUrl
        }.BuildRequest();
        
        await SendIntegrationRequest(isbankInitRequest, Guid.NewGuid(), IntegrationLogDataType.Html);

        return new PosInit3DModelResponse
        {
            IsSuccess = true,
            HtmlContent = Base64Encode(isbankInitRequest),
            ResponseCode = string.Empty,
            ResponseMessage = string.Empty,
        };
    }

    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
    
    public async Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request)
    {
        var installCount = 1;
        if (request.Installment is not null)
        {
            installCount = request.Installment > 0 ? (int)request.Installment : 1;
        }

        if (request.AuthType == VposAuthType.PreAuth)
        {
            request.BonusAmount = 0m;
        }
        
        var isbankSecureRequest = new IsbankPayment3DModelRequest
        {
            MerchantNumber = _isbankPos.MerchantNumber,
            OrderNumber = request.OrderNumber,
            Currency = request.Currency,
            Amount = request.Amount,
            Installment = installCount,
            PointAmount = request.BonusAmount,
            ThreeDSTransactionId = request.PayerTxnId,
            SubMerchantId = request.SubMerchantId,
            SubMerchantMcc = request.SubMerchantMcc,
            SubMerchantCitizenId = request.SubMerchantTaxNumber,
            SubMerchantCity = request.SubMerchantCity,
            SubMerchantPostalCode = request.SubMerchantPostalCode,
            SubMerchantUrl = request.SubMerchantUrl
        }.BuildRequest();
        
        var sendUrl = request.AuthType == VposAuthType.Auth ?
            _isbankPos.BaseUrl + SecureUrl : _isbankPos.BaseUrl + SecurePreAuthUrl;

        var content = await SendIsbankRequestAsync(sendUrl,isbankSecureRequest);
        
        var parseResponse = new IsbankPayment3DModelResponse().Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (SuccessCodes.Contains(parseResponse.ResponseCode))
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderNumber;
            response.TrxDate = parseResponse.AuthorizationDateTime;
            response.RrnNumber = parseResponse.RetrievalReferenceNumber.ToString();
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
        }
        return response;
    }

    public async Task<PosVerify3dModelResponse> Verify3DModel(Dictionary<string, string> form)
    {
        var response = new PosVerify3dModelResponse();
        response.IsSuccess = false;

        if (form == null)
        {
            response.ResponseCode = "";
            response.ResponseMessage = "Invalid Form";
            return await Task.FromResult(response);
        }
        
        form.TryGetValue("MdStatus", out string mdStatus);
        form.TryGetValue("mdErrorMsg", out string mdErrorMessage);
        form.TryGetValue("OrderNumber", out string orderNumber);
        form.TryGetValue("FlowType", out string flowType);
        form.TryGetValue("TransactionId", out string transactionId);
        form.TryGetValue("Eci", out string eci);
        form.TryGetValue("MerchantNumber", out string merchantNumber);
        form.TryGetValue("ErrorCode", out string errorCode);
        form.TryGetValue("ErrorMessage", out string errorMessage);
        form.TryGetValue("Rnd", out string rnd);
        form.TryGetValue("UsedPointAmount", out string usedPointAmount);
        form.TryGetValue("SaleAmount", out string saleAmount);
        form.TryGetValue("Hash", out string hash);

        var verification = await _threeDVerificationRepository.GetAll()
            .Where(v => v.OrderId == orderNumber)
            .FirstOrDefaultAsync();

        try
        {
            var decimalAmount = Math.Round(verification.Amount, 2);
            var hashAmount = (int)(decimalAmount * 100); 

            var hashString = _isbankPos.MerchantNumber + "-" + orderNumber + "-" + hashAmount.ToString("0") + "-" + rnd + "-" + _isbankPos.MerchantPassword;
            var calcHash = VposHelper.GetSHA512String(hashString);
            
            if (!hash.Equals(calcHash))
            {
                response.IsSuccess = false;
                response.ResponseCode = "";
                response.ResponseMessage = "Invalid Hash";
                return response;
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Error while calculating hash: {Message}", e.Message);
            throw;
        }

        if (mdStatus.Trim() == "1")
            response.TxnStat = ThreeDFullSecureStatus;
        else if (mdStatus.Trim() == "2" || mdStatus.Trim() == "3" || mdStatus.Trim() == "4")
            response.TxnStat = ThreeDHalfSecureStatus;

        response.Eci = eci;
        response.Hash = hash;
        response.PayerTxnId = transactionId;
        response.OrderNumber = orderNumber;
        response.MdStatus = mdStatus;
        response.MdErrorMessage = mdErrorMessage;
        response.TransId = transactionId;
        response.IsSuccess = true;

        
        return response;
    }

    public async Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request)
    {
        var isbankPostAuthRequest = new IsbankPostAuthRequest
        {
            MerchantNumber = _isbankPos.MerchantNumber,
            OrderNumber = request.OrderNumber,
            Currency = request.Currency,
            Amount = request.Amount,
            OriginalRetrievalReferenceNumber = request.RRN
        }.BuildRequest();

        var content = await SendIsbankRequestAsync(_isbankPos.BaseUrl + PostAuthUrl,isbankPostAuthRequest);
        
        var parseResponse = new IsbankPostAuthResponse().Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (SuccessCodes.Contains(parseResponse.ResponseCode))
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderNumber;
            response.TrxDate = parseResponse.AuthorizationDateTime;
            response.RrnNumber = parseResponse.RetrievalReferenceNumber.ToString();
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
        }
        return response;
    }

    public async Task<PosVoidResponse> Void(PosVoidRequest request)
    {
        var isbankVoidRequest = new IsbankVoidRequest
        {
            MerchantNumber = _isbankPos.MerchantNumber,
            OrderNumber = request.OrderNumber,
            Currency = request.Currency,
            Amount = request.Amount,
            OriginalRetrievalReferenceNumber = request.RRN
        }.BuildRequest();
        
        var content = await SendIsbankRequestAsync(_isbankPos.BaseUrl + VoidUrl,isbankVoidRequest);
        
        var parseResponse = new IsbankVoidResponse().Parse(content);

        var response = new PosVoidResponse();

        if (SuccessCodes.Contains(parseResponse.ResponseCode))
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderNumber;
            response.TrxDate = parseResponse.AuthorizationDateTime;
            response.RrnNumber = parseResponse.RetrievalReferenceNumber.ToString();
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
        }
        return response;
    }

    public async Task<PosRefundResponse> Refund(PosRefundRequest request)
    {
        var isbankRefundRequest = new IsbankRefundRequest
        {
            MerchantNumber = _isbankPos.MerchantNumber,
            OrderNumber = request.OrderNumber,
            Currency = request.Currency,
            Amount = request.Amount,
            OriginalRetrievalReferenceNumber = request.RRN
        }.BuildRequest();
        
        var content = await SendIsbankRequestAsync(_isbankPos.BaseUrl + RefundUrl,isbankRefundRequest);
        
        var parseResponse = new IsbankRefundResponse().Parse(content);

        var response = new PosRefundResponse();

        if (SuccessCodes.Contains(parseResponse.ResponseCode))
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderNumber;
            response.TrxDate = parseResponse.AuthorizationDateTime;
            response.RrnNumber = parseResponse.RetrievalReferenceNumber.ToString();
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
        }
        return response;
    }

    public async Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request)
    {
        var paymentDetailRequest = new IsbankPaymentDetailRequest
        {
            MerchantNumber = _isbankPos.MerchantNumber,
            RetrievalReferenceNumber = request.RRN,
        };
        var paymentDetailResponse = await paymentDetailRequest.SendQueryRequestAsync(_isbankPos.BaseUrl);
        var parseResponse = new IsbankPaymentDetailResponse().Parse(paymentDetailResponse);
        
        var response = new PosPaymentDetailResponse
        {
            IsSuccess = SuccessCodes.Contains(parseResponse.TrnxResponseCode),
            ResponseCode = parseResponse.TrnxResponseCode,
            ResponseMessage = parseResponse.TrnxResponseStatus,
            OrderStatus = GetOrderStatus(parseResponse),
            TransactionDate = DateTime.Parse(parseResponse.TrnxDate),
            Amount = parseResponse.Amount,
            RrnNumber = parseResponse.OriginalRetrievalReferenceNumber
        };

        return response;
    }

    public async Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request)
    {
        var isbankNonSecureRequest = new IsbankPointInquiryRequest
        {
            MerchantNumber = _isbankPos.MerchantNumber,
            OrderNumber = request.OrderNumber,
            CardNumber = request.CardNumber,
            CardExpireMonth = Int32.Parse(request.ExpireMonth),
            CardExpireYear = Int32.Parse(request.ExpireYear),
            Cvv = request.Cvv2,
        }.BuildRequest();
        
        var content = await SendIsbankRequestAsync(_isbankPos.BaseUrl + PointInquiryUrl,isbankNonSecureRequest);
        
        var parseResponse = new IsbankPointInquiryResponse().Parse(content);

        var response = new PosPointInquiryResponse();

        if (SuccessCodes.Contains(response.ResponseCode))
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderNumber;
            response.TrxDate = parseResponse.AuthorizationDateTime;
            response.AvailablePoint = parseResponse.MaxipointTotalForMile;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.Message;
        }
        return response;
    }

    private async Task<string> SendIsbankRequestAsync(string url,string data)
    {
        var correlationId = Guid.NewGuid();

        var handler = await ChainVerification();
        
        var accessToken = await GetAccessToken(handler);
        
        var certificate = await _vaultClient.GetSecretValueAsync<string>("PFSecrets", "VposSettings", "ClientCertificate");

        var apiHandler = await ChainVerification();
        using (HttpClient client = new HttpClient(apiHandler))
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Add("X-Isbank-Client-Id", _isbankPos.ClientId);
            client.DefaultRequestHeaders.Add("X-Isbank-Client-Secret", _isbankPos.ClientSecret);
            client.DefaultRequestHeaders.Add("X-Client-Certificate", certificate);

            HttpContent request = new StringContent(data, Encoding.UTF8, "application/json");

            await SendIntegrationRequest(data, correlationId, IntegrationLogDataType.Json);
            var content = "";
            HttpResponseMessage response = await client.PostAsync(url, request);
            await SendIntegrationResponse(response, correlationId);
            content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            return content;
        }
    }
    
    private async Task SendIntegrationRequest(string data, Guid correlationId, IntegrationLogDataType integrationLogDataType)
    {
        try
        {
            var isLogEnable =
                await _parameterService.GetParameterAsync(VposConsts.ParameterGroupCode, VposConsts.IsBankVpos);
            if(isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.IsBankVpos,
                    Type = nameof(IntegrationLogType.Vpos),
                    Date = DateTime.Now,
                    Request = data,
                    DataType = integrationLogDataType
                };

                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.IsBankVpos} - Exception {exception}");
        }

    }
    
    private async Task SendIntegrationResponse(HttpResponseMessage data, Guid correlationId)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
                (VposConsts.ParameterGroupCode, VposConsts.IsBankVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var bytes = await data.Content.ReadAsByteArrayAsync();
                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.IsBankVpos,
                    Type = nameof(IntegrationLogType.Vpos),
                    Date = DateTime.Now,
                    Response = Encoding.UTF8.GetString(bytes),
                    DataType = IntegrationLogDataType.Text
                };

                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.IsBankVpos} - Exception {exception}");
        }

    }

    public void SetServiceParameters(object serviceParameters, Guid? merchantId = null)
    {
        _isbankPos = (IsbankPosInfo)serviceParameters;
    }
    private static OrderStatus GetOrderStatus(IsbankPaymentDetailResponse paymentDetail)
    {
        if (!SuccessCodes.Contains(paymentDetail.TrnxResponseCode))
        {
            return OrderStatus.Rejected;
        }

        if (paymentDetail.TrnxResponseStatus == "Declined")
        {
            return OrderStatus.Rejected;
        }

        if (paymentDetail.TrnxResponseStatus == "Voided")
        {
            return OrderStatus.Cancelled;
        }

        if (paymentDetail.TrnxResponseStatus == "Reversed")
        {
            return OrderStatus.Refunded;
        }

        var txnDate = DateTime.Parse(paymentDetail.TrnxDate);
        if (SuccessCodes.Contains(paymentDetail.TrnxResponseCode))
        {
            return txnDate.Date == DateTime.Now.Date
                ? OrderStatus.WaitingEndOfDay
                : OrderStatus.EndOfDayCompleted;
        }

        return OrderStatus.Unknown;
    }

    private async Task<string> GetAccessToken(HttpClientHandler handler)
    {
        using HttpClient client = new HttpClient(handler);
        
        var tokenUrl = $"{_isbankPos.TokenUrl}/api/isbank/v1/app-identity-provider/oauth2/token";

        var clientId = _isbankPos.ClientId;
        var clientSecret = _isbankPos.ClientSecret;
        var scopes = "manage:virtual-pos";

        var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
        var split = _isbankPos.TokenUrl.Split("://");
        client.DefaultRequestHeaders.Host = split[1];
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", scopes),
        });

        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        try
        {
            var response = await client.PostAsync(tokenUrl, content);
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = JsonDocument.Parse(json);
                return tokenResponse.RootElement.GetProperty("access_token").GetString();
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"GetAccessToken Request failed for Isbank Vpos: {e.InnerException}");
        }
        return null;
    }
    
    private async Task<HttpClientHandler> ChainVerification()
    {
        var p12 = await _vaultClient.GetSecretValueAsync<string>("PFSecrets", "VposSettings", "IsbankMtlsCertificate");
        var p12Password = await _vaultClient.GetSecretValueAsync<string>("PFSecrets", "VposSettings", "IsbankMtlsCertificatePassword");
        var p12Bytes = Convert.FromBase64String(p12);
        
        var error = false;
        var clientCert = new X509Certificate2(
            p12Bytes,
            p12Password,
        X509KeyStorageFlags.MachineKeySet);
        var handler = new HttpClientHandler
        {
            SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
        };
        handler.ClientCertificates.Add(clientCert);
        
        handler.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => errors == SslPolicyErrors.None;

        return handler;
    }

    private int GetTransactionType(VposAuthType authType, decimal amount, decimal bonusAmount)
    {
        if (authType == VposAuthType.PreAuth)
        {
            return 2;
        }
        if (bonusAmount > 0m && amount <= 0)
        {
            return 5;
        }

        if (bonusAmount > 0m && amount > 0)
        {
            return 3;
        }

        if (authType == VposAuthType.Auth)
        {
            return 1;
        }
        
        return 1;
    }
}