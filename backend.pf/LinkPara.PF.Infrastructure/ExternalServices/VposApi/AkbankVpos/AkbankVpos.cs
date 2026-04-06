using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Request;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Response;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos;

public class AkbankVpos : VposBase, IVposApi
{
    private AkbankPosInfo _akbankPos;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;
    private readonly ILogger<AkbankVpos> _logger;
    private const string SuccessCode = "VPS-0000";
    private readonly string ThreeDFullSecureStatus = "Y";
    private readonly string ThreeDHalfSecureStatus = "A";

    public AkbankVpos(
        IParameterService parameterService,
        IBus bus,
        ILogger<AkbankVpos> logger)
    {
        _parameterService = parameterService;
        _bus = bus;
        _logger = logger;
    }

    public async Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request)
    {
        var expiryDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear);
        string PaymentModel = "3D";

        var initRequest = new AkbankInit3dModelRequest
        {
            PostUrl = _akbankPos.ThreeDSecureUrl,
            Version = "1.00",
            PaymentModel = PaymentModel,
            TxnCode = request.AuthType == VposAuthType.Auth ? "3000" : "3004",
            MerchantSafeId = _akbankPos.MerchantSafeId,
            TerminalSafeId = _akbankPos.TerminalSafeId,
            SecretKey = _akbankPos.SecretKey,
            OrderId = request.OrderNumber,
            Lang = request.LanguageCode.ToUpper(),
            Amount = FormatAmount(request.Amount),
            BonusAmount =  FormatAmount(request.BonusAmount ?? 0),
            CurrencyCode = request.Currency,
            InstallCount = request.Installment > 0 ? request.Installment : 1,
            OkUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            FailUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            SubMerchantId = request.SubMerchantCode,
            CardNumber = request.CardNumber,
            Cvv2 = request.Cvv2,
            ExpireDate = expiryDate,
            RandomNumber = GetRandomNumberBase16(128),
            RequestDateTime = FormatDate(DateTime.UtcNow)
        };

        var correlationId = Guid.NewGuid();

        var json = JsonConvert.SerializeObject(initRequest, Formatting.Indented);
        await SendIntegrationRequest(json, correlationId, IntegrationLogDataType.Json);

        var html = await initRequest.BuildRequest();
        await SendIntegrationRequest(html, correlationId, IntegrationLogDataType.Html);

        return new PosInit3DModelResponse
        {
            IsSuccess = true,
            HtmlContent = Base64Encode(html),
            ResponseCode = string.Empty,
            ResponseMessage = string.Empty,
        };
    }

    public async Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request)
    {
        var installCount = 1;
        if (request.Installment is not null)
        {
            installCount = request.Installment > 0 ? (int)request.Installment : 1;
        }

        var bonusAmount = decimal.Round(request.BonusAmount, 2, MidpointRounding.AwayFromZero);
        bonusAmount = decimal.Parse(bonusAmount.ToString("0.00"));

        var authRequest = new AkbankPayment3DModelRequest
        {
            MerchantSafeId = _akbankPos.MerchantSafeId,
            TerminalSafeId = _akbankPos.TerminalSafeId,
            TxnCode = request.AuthType == VposAuthType.Auth ? "1000" : "1004",
            OrderId = request.OrderNumber,
            Amount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            MotoInd = "0",
            InstallCount = installCount,
            PointAmount = bonusAmount,
            SecureId = request.PayerTxnId,
            SecureEcomInd = request.Eci,
            SecureData = request.Cavv,
            SecureMd = request.MD,
            RandomNumber = GetRandomNumberBase16(128),
            RequestDateTime = FormatDate(DateTime.UtcNow),
            SubMerchantId = request.SubMerchantCode
        }.BuildRequest();

        var content = await SendAkbankRequestAsync(_akbankPos.NonSecureUrl, authRequest);

        var parseResponse = new AkbankPayment3DModelResponse()
            .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResponseCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderId;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
        }
        return response;
    }

    public async Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request)
    {
        var paymentDetailRequest = new AkbankPaymentDetailRequest
        {
            MerchantSafeId = _akbankPos.MerchantSafeId,
            TerminalSafeId = _akbankPos.TerminalSafeId,
            Lang = request.LanguageCode.ToUpper(),
            TxnCode = "1010",
            RandomNumber = GetRandomNumberBase16(128),
            RequestDateTime = FormatDate(DateTime.UtcNow),
            OrderId = request.OrderNumber,
            SubMerchantId = request.SubMerchantCode,
        }.BuildRequest();

        var content = await SendAkbankRequestAsync(_akbankPos.NonSecureUrl, paymentDetailRequest);

        var parseResponse = new AkbankPaymentDetailResponse()
            .Parse(content);

        var response = new PosPaymentDetailResponse();

        if (parseResponse.ResponseCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderStatus = GetOrderStatus(parseResponse);
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.OrderStatus = GetOrderStatus(parseResponse);
        }
        return response;
    }

    public async Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request)
    {
        var expiryDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear);

        var installCount = 1;
        if (request.Installment is not null)
        {
            installCount = request.Installment > 0 ? (int)request.Installment : 1;
        }

        var bonusAmount = 0.00m;
        if (request.BonusAmount is not null)
        {
            bonusAmount = decimal.Round(request.BonusAmount ?? 0, 2, MidpointRounding.AwayFromZero);
            bonusAmount = decimal.Parse(bonusAmount.ToString("0.00"));
        }

        var authRequest = new AkbankPaymentNonSecureRequest
        {
            MerchantSafeId = _akbankPos.MerchantSafeId,
            TerminalSafeId = _akbankPos.TerminalSafeId,
            Lang = request.LanguageCode.ToUpper(),
            TxnCode = request.AuthType == VposAuthType.Auth ? "1000" : "1004",
            CardNumber = request.CardNumber,
            ExpireDate = expiryDate,
            MotoInd = "0",
            Amount = FormatAmount(request.Amount),
            NonSecureBonusAmount = bonusAmount,
            CurrencyCode = request.Currency,
            OrderId = request.OrderNumber,
            InstallCount = installCount,
            Cvv2 = request.Cvv2,
            SubMerchantId = request.SubMerchantCode,
            RandomNumber = GetRandomNumberBase16(128),
            RequestDateTime = FormatDate(DateTime.UtcNow),
        }.BuildRequest();

        var content = await SendAkbankRequestAsync(_akbankPos.NonSecureUrl, authRequest);

        var parseResponse = new AkbankPaymentNonSecureResponse()
            .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResponseCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderId;
            response.RrnNumber = parseResponse.RrnNumber;
            response.TrxDate = parseResponse.TxnDateTime;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
        }
        return response;
    }

    public async Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request)
    {
        var expiryDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear);
        var pointInquiryRequest = new AkbankPointInquiryRequest
        {
            MerchantSafeId = _akbankPos.MerchantSafeId,
            TerminalSafeId = _akbankPos.TerminalSafeId,
            TxnCode = "1006",
            CardNumber = request.CardNumber,
            ExpireDate = expiryDate,
            Cvv2 = request.Cvv2,
            RandomNumber = GetRandomNumberBase16(128),
            RequestDateTime = FormatDate(DateTime.UtcNow),
        }.BuildRequest();

        var content = await SendAkbankRequestAsync(_akbankPos.NonSecureUrl, pointInquiryRequest);

        var parseResponse = new AkbankPointInquiryResponse()
           .Parse(content);

        var response = new PosPointInquiryResponse();

        if (parseResponse.ResponseCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.AvailablePoint = parseResponse.AvailablePoint;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
        }

        return response;
    }

    public async Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request)
    {
        var postAuthRequest = new AkbankPostAuthRequest
        {
            MerchantSafeId = _akbankPos.MerchantSafeId,
            TerminalSafeId = _akbankPos.TerminalSafeId,
            TxnCode = "1005",
            Amount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            OrderId = request.OrgOrderNumber,
            SubMerchantId = request.SubMerchantCode,
            RandomNumber = GetRandomNumberBase16(128),
            RequestDateTime = FormatDate(DateTime.UtcNow),
        }.BuildRequest();

        var content = await SendAkbankRequestAsync(_akbankPos.NonSecureUrl, postAuthRequest);

        var parseResponse = new AkbankPostAuthResponse()
            .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResponseCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderId;
            response.RrnNumber = parseResponse.RrnNumber;
            response.TrxDate = parseResponse.TxnDateTime;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
        }
        return response;
    }

    public async Task<PosRefundResponse> Refund(PosRefundRequest request)
    {
        var refundRequest = new AkbankRefundRequest
        {
            TxnCode = "1002",
            RequestDateTime = FormatDate(DateTime.UtcNow),
            RandomNumber = GetRandomNumberBase16(128),
            MerchantSafeId = _akbankPos.MerchantSafeId,
            TerminalSafeId = _akbankPos.TerminalSafeId,
            OrderId = request.OrgAuthProcessOrderNo,
            SubMerchantId = request.SubMerchantCode,
            Amount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
        }.BuildRequest();

        var content = await SendAkbankRequestAsync(_akbankPos.NonSecureUrl, refundRequest);

        var parseResponse = new AkbankRefundResponse()
            .Parse(content);

        var response = new PosRefundResponse();

        if (parseResponse.ResponseCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderId;
            response.RrnNumber = parseResponse.RrnNumber;
            response.TrxDate = parseResponse.TxnDateTime;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
        }

        return response;
    }

    public void SetServiceParameters(object serviceParameters, Guid? merchantId = null)
    {
        _akbankPos = (AkbankPosInfo)serviceParameters;
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

        form.TryGetValue("mdStatus", out string status);
        form.TryGetValue("txnCode", out string txnCode);
        form.TryGetValue("responseCode", out string responseCode);
        form.TryGetValue("responseMessage", out string responseMessage);
        form.TryGetValue("orderId", out string orderId);
        form.TryGetValue("secureEcomInd", out string secureEcomInd);
        form.TryGetValue("secureId", out string secureId);
        form.TryGetValue("secureData", out string secureData);
        form.TryGetValue("secureMd", out string secureMd);

        response.MdStatus = status;
        response.MdErrorMessage = responseMessage;

        if (string.IsNullOrEmpty(status))
        {
            response.ResponseCode = responseCode;
            response.ResponseMessage = responseMessage;
            return await Task.FromResult(response);
        }

        if (status != "1" && status != "4")
        {
            response.ResponseCode = "";
            if (!string.IsNullOrEmpty(responseCode))
            {
                response.ResponseMessage = responseCode;
            }
            response.ResponseMessage = responseMessage;
            return await Task.FromResult(response);
        }

        if (responseCode != SuccessCode)
        {
            response.ResponseCode = responseCode;
            response.ResponseMessage = responseMessage;
            return await Task.FromResult(response);
        }

        string[] parameters = form["hashParams"].Split("+");
        StringBuilder builder = new StringBuilder();
        foreach (string param in parameters)
        {
            builder.Append(form[param]);
        }
        string hash = VposHelper.HashToString(builder.ToString(), _akbankPos.SecretKey);

        if (form["hash"] != hash)
        {
            response.Hash = form["hash"];
            response.ResponseCode = "";
            response.ResponseMessage = "Invalid Hash";
            return await Task.FromResult(response);
        }

        response.Cavv = secureData;
        response.Eci = secureEcomInd;
        response.OrderNumber = orderId;
        response.MD = secureMd;
        response.PayerTxnId = secureId;
        response.IsSuccess = true;
        response.TxnStat = status == "1" ? ThreeDFullSecureStatus : status == "4" ? ThreeDHalfSecureStatus : "N";

        return await Task.FromResult(response);
    }

    public async Task<PosVoidResponse> Void(PosVoidRequest request)
    {
        var voidRequest = new AkbankVoidRequest
        {
            TxnCode = "1003",
            RequestDateTime = FormatDate(DateTime.UtcNow),
            RandomNumber = GetRandomNumberBase16(128),
            MerchantSafeId = _akbankPos.MerchantSafeId,
            TerminalSafeId = _akbankPos.TerminalSafeId,
            OrderId = request.OrgAuthProcessOrderNo,
            SubMerchantId = request.SubMerchantCode
        }.BuildRequest();

        var content = await SendAkbankRequestAsync(_akbankPos.NonSecureUrl, voidRequest);

        var parseResponse = new AkbankVoidResponse()
            .Parse(content);

        var response = new PosVoidResponse();

        if (parseResponse.ResponseCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderId;
            response.RrnNumber = parseResponse.RrnNumber;
            response.TrxDate = parseResponse.TxnDateTime;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
        }

        return response;
    }

    private async Task<string> SendAkbankRequestAsync(string url, string data)
    {
        var correlationId = Guid.NewGuid();

        string hash = VposHelper.HashToString(data, _akbankPos.SecretKey);

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("auth-hash", hash);

            HttpContent request = new StringContent(data, Encoding.UTF8, "application/json");

            await SendIntegrationRequest(data, correlationId, IntegrationLogDataType.Json);
            var content = "";
            try
            {
                HttpResponseMessage response = await client.PostAsync(url, request);
                await SendIntegrationResponse(response, correlationId);

                content = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {

                _logger.LogError($"AcquireBankPatchError : {e}");
                throw;
            }


            return content;
        }
    }
    private async Task SendIntegrationRequest(string data, Guid correlationId, IntegrationLogDataType integrationLogDataType)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync(VposConsts.ParameterGroupCode, VposConsts.AkbankVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.AkbankVpos,
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
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.AkbankVpos} - Exception {exception}");
        }

    }
    private async Task SendIntegrationResponse(HttpResponseMessage data, Guid correlationId)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.AkbankVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var bytes = await data.Content.ReadAsByteArrayAsync();
                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.AkbankVpos,
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
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.AkbankVpos} - Exception {exception}");
        }

    }

    protected override string FormatAmount(decimal amount)
    {
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero)
            .ToString("0.00", new CultureInfo("en-US"));
    }

    protected override string FormatExpiryDate(string month, string year)
    {
        var date = $"{month}{year}";
        return date;
    }

    private static string FormatDate(DateTime date)
    {
        return DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff");
    }

    private static string GetRandomNumberBase16(int length)
    {
        var byteArray = new byte[length / 2];

        RandomNumberGenerator.Fill(byteArray);

        StringBuilder result = new StringBuilder(length);
        foreach (byte b in byteArray)
        {
            result.Append(b.ToString("x2"));
        }

        return result.ToString();
    }

    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
    private static OrderStatus GetOrderStatus(AkbankPaymentDetailResponse paymentDetail)
    {
        if (paymentDetail.ResponseCode != SuccessCode)
        {
            return OrderStatus.Rejected;
        }

        if (paymentDetail.TxnStatus == "S")
        {
            return OrderStatus.Rejected;
        }

        if (paymentDetail.TxnStatus == "V")
        {
            return OrderStatus.Cancelled;
        }

        if (paymentDetail.TxnStatus == "R")
        {
            return OrderStatus.Refunded;
        }

        if (paymentDetail.ResponseCode == SuccessCode)
        {
            return paymentDetail.TxnDateTime.Date == DateTime.Now.Date
                                             ? OrderStatus.WaitingEndOfDay
                                             : OrderStatus.EndOfDayCompleted;
        }

        return OrderStatus.Unknown;
    }
}
