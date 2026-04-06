using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Request;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Response;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos;

public class InterVpos : VposBase, IVposApi
{
    private const string SuccessCode = "00";
    private const string OrderNotFoundCode = "05";
    private const string FullSecureStatusCode = "Y";
    private const string HalfSecureStatusCode = "A";

    private IvpPosInfo _posInfo;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;
    private readonly ILogger<InterVpos> _logger;

    public InterVpos(
        IParameterService parameterService,
        IBus bus,
        ILogger<InterVpos> logger)
    {
        _parameterService = parameterService;
        _bus = bus;
        _logger = logger;
    }

    public async Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request)
    {
        var bonusInquiry = new IvpBonusInquiryRequest
        {
            ShopCode = _posInfo.ShopCode,
            UserCode = _posInfo.UserCode,
            UserPass = _posInfo.UserPass,
            OrderId = request.OrderNumber,
            Lang = request.LanguageCode,
            Currency = request.Currency,
            Cvv2 = request.Cvv2,
            Expiry = FormatExpiryDate(request.ExpireMonth, request.ExpireYear),
            Pan = request.CardNumber,
            SecureType = nameof(VposSecureType.NonSecure),
            TxnType = "BonusInquiry"
        }.BuildRequest();

        var content = await SendRequestAsync(_posInfo.NonSecureUrl, bonusInquiry);

        var parseResponse = new IvpBonusInquiryResponse()
            .Parse(content);

        var response = new PosPointInquiryResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = string.Empty;
            response.ResponseMessage = string.Empty;
            response.AuthCode = parseResponse.AuthCode;
            response.RrnNumber = parseResponse.TransId;
            response.OrderNumber = parseResponse.OrderId;
            response.AvailablePoint = parseResponse.AvailableBonus;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.TrxDate = parseResponse.TrxDate;
        }

        return response;
    }

    public async Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request)
    {
        var authRequest = new IvpPaymentNonSecureRequest
        {
            ShopCode = _posInfo.ShopCode,
            UserCode = _posInfo.UserCode,
            UserPass = _posInfo.UserPass,
            PurcAmount = FormatAmount(request.Amount),
            OrderId = request.OrderNumber,
            Lang = request.LanguageCode,
            Currency = request.Currency,
            BonusAmount = request.BonusAmount ?? 0,
            Cvv2 = request.Cvv2,
            Expiry = FormatExpiryDate(request.ExpireMonth, request.ExpireYear),
            InstallmentCount = request.Installment ?? 0,
            Pan = request.CardNumber,
            SubMerchantCode = request.SubMerchantId,
            SecureType = nameof(VposSecureType.NonSecure),
            TxnType = request.AuthType == VposAuthType.PreAuth ? "PreAuth" : "Auth"
        }.BuildRequest();

        var content = await SendRequestAsync(_posInfo.NonSecureUrl, authRequest);

        var parseResponse = new IvpPaymentNonSecureResponse()
            .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = string.Empty;
            response.ResponseMessage = string.Empty;
            response.AuthCode = parseResponse.AuthCode;
            response.RrnNumber = parseResponse.TransId;
            response.OrderNumber = parseResponse.OrderId;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.TrxDate = parseResponse.TrxDate;
        }

        return response;
    }
    private string GetHash(PosInit3DModelRequest request, string rnd, string amount)
    {
        var hashBuilder = new StringBuilder();
        hashBuilder.Append(_posInfo.ShopCode);
        hashBuilder.Append(request.OrderNumber);
        hashBuilder.Append(amount);
        hashBuilder.Append($"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}");
        hashBuilder.Append($"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}");
        hashBuilder.Append(request.AuthType == VposAuthType.PreAuth ? "PreAuth" : "Auth");
        hashBuilder.Append(request.Installment);
        hashBuilder.Append(rnd);
        hashBuilder.Append(request.CardNumber);
        hashBuilder.Append(_posInfo.MerchantPass);

        return VposHelper.GetEncoding1254(hashBuilder.ToString());
    }
    private string GetRnd()
    {
        return DateTime.Now.Ticks.ToString();
    }

    public async Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request)
    {
        var purchAmount = FormatAmount(request.Amount);
        var rnd = GetRnd();
        var hash = GetHash(request, rnd, purchAmount);

        var threedRequest = new IvpInit3dModelRequest
        {
            ShopCode = _posInfo.ShopCode,
            UserCode = _posInfo.UserCode,
            UserPass = _posInfo.UserPass,
            PurcAmount = purchAmount,
            OrderId = request.OrderNumber,
            Lang = request.LanguageCode,
            Currency = request.Currency,
            BonusAmount = request.BonusAmount ?? 0,
            Cvv2 = request.Cvv2,
            Hash = hash,
            Rnd = rnd,
            Expiry = FormatExpiryDate(request.ExpireMonth, request.ExpireYear),
            InstallmentCount = request.Installment,
            Pan = request.CardNumber,
            SubMerchantCode = request.SubMerchantId,
            SecureType = "3DModel",
            TxnType = request.AuthType == VposAuthType.PreAuth ? "PreAuth" : "Auth",
            OkUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            FailUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            MerchantPass = _posInfo.MerchantPass
        }.BuildRequest();

        var content = await SendRequestAsync(_posInfo.ThreeDSecureUrl, threedRequest);

        if (string.IsNullOrEmpty(content))
        {
            return new PosInit3DModelResponse { IsSuccess = false, HtmlContent = string.Empty };
        }

        var dic = GetFormParams(content);

        if (dic?.ContainsKey("ErrorMessage") == true || dic?.ContainsKey("ErrorCode") == true)
        {
            var errorCode = dic?.ContainsKey("ErrorCode") == true ? dic["ErrorCode"].ToString() : string.Empty;
            var errorMessage = dic?.ContainsKey("ErrorMessage") == true ? dic["ErrorMessage"].ToString() : string.Empty;

            return new PosInit3DModelResponse
            {
                IsSuccess = false,
                ResponseCode = errorCode,
                ResponseMessage = errorMessage,
                HtmlContent = Base64Encode(content),
                Hash = hash
            };
        }
        else
        {
            return new PosInit3DModelResponse
            {
                IsSuccess = true,
                HtmlContent = Base64Encode(content),
                ResponseCode = string.Empty,
                ResponseMessage = string.Empty,
                Hash = hash
            };
        }

    }

    public async Task<PosVerify3dModelResponse> Verify3DModel(Dictionary<string, string> form)
    {
        var response = new PosVerify3dModelResponse();
        response.IsSuccess = false;
        form.TryGetValue("HASHPARAMS", out string hashParams);
        form.TryGetValue("HASHPARAMSVAL", out string hashParamsVal);
        var paramsVal = "";
        var index1 = 0;
        do
        {
            var index2 = hashParams.IndexOf(":", index1, StringComparison.Ordinal);
            var val = string.IsNullOrEmpty(form[hashParams[index1..index2]]) ? "" : form[hashParams[index1..index2]].ToString();
            paramsVal += val;
            index1 = index2 + 1;
        }
        while (index1 < hashParams.Length);

        var hashVal = paramsVal + _posInfo.MerchantPass;
        form.TryGetValue("HASH", out string hashParam);

        // todo : library
        var hash = VposHelper.GetSha1(hashVal);
        var encodeHashParam = HttpUtility.UrlEncode(hashParam);

        // todo : hata mesajlarına bakılmalı
        if (!paramsVal.Equals(hashParamsVal) || !hash.Equals(encodeHashParam))
        {
            response.ResponseCode = "";
            response.ResponseMessage = "Invalid Hash";
           // return await Task.FromResult(response);
        }

        form.TryGetValue("OrderId", out string orderId);
        form.TryGetValue("MD", out string md);
        form.TryGetValue("PayerTxnId", out string payerTxnId);
        form.TryGetValue("PayerAuthenticationCode", out string payerAuthenticationCode);
        form.TryGetValue("Eci", out string eci);
        form.TryGetValue("mdStatus", out string mdStatus);
        form.TryGetValue("mdErrorMsg", out string mdErrorMessage);
        form.TryGetValue("3DStatus", out string threeDStatus);
        form.TryGetValue("TxnStat", out string txnStat);

        response.Hash = hash;
        response.HashParams = hashParams;
        response.HashParamsVal = hashParamsVal;
        response.Cavv = payerAuthenticationCode;
        response.Eci = eci;
        response.PayerTxnId = payerTxnId;
        response.OrderNumber = orderId;
        response.MD = md;
        response.MdStatus = txnStat == FullSecureStatusCode ? "1" : txnStat == HalfSecureStatusCode ? "2" : "0";
        response.MdErrorMessage = mdErrorMessage;
        response.ThreeDStatus = threeDStatus;
        response.TxnStat = txnStat;
        response.IsSuccess = true;

        return await Task.FromResult(response);
    }

    public async Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request)
    {
        var payment3DRequest = new IvpPayment3DModelRequest
        {
            ShopCode = _posInfo.ShopCode,
            UserCode = _posInfo.UserCode,
            UserPass = _posInfo.UserPass,
            PurcAmount = FormatAmount(request.Amount),
            OrderId = request.OrderNumber,
            Lang = request.LanguageCode,
            Currency = request.Currency,
            InstallmentCount = request.Installment ?? 0,
            SubMerchantCode = request.SubMerchantId,
            SecureType = nameof(VposSecureType.NonSecure),
            TxnType = request.AuthType == VposAuthType.PreAuth ? "PreAuth" : "Auth",
            MD = request.MD,
            PayerTxnId = request.PayerTxnId,
            PayerAuthenticationCode = request.Cavv,
            Eci = request.Eci,
            BonusAmount = request.BonusAmount
        }.BuildRequest();

        var content = await SendRequestAsync(_posInfo.NonSecureUrl, payment3DRequest);

        var parseResponse = new IvpPayment3DModelResponse()
            .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = string.Empty;
            response.ResponseMessage = string.Empty;
            response.AuthCode = parseResponse.AuthCode;
            response.RrnNumber = parseResponse.TransId;
            response.OrderNumber = parseResponse.OrderId;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.TrxDate = parseResponse.TrxDate;
        }

        return response;
    }

    public async Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request)
    {
        var postAuthRequest = new IvpPostAuthRequest
        {
            ShopCode = _posInfo.ShopCode,
            UserCode = _posInfo.UserCode,
            UserPass = _posInfo.UserPass,
            PurcAmount = FormatAmount(request.Amount),
            OrgOrderId = request.OrgOrderNumber,
            OrderId = request.OrderNumber,
            Moto = 0,
            Lang = request.LanguageCode,
            Currency = request.Currency,
            CardType = GetCardTypeNumber(request.CardBrand),
            SecureType = nameof(VposSecureType.NonSecure),
            TxnType = nameof(VposTransactionType.PostAuth),
            SubMerchantCode = request.SubMerchantId
        }.BuildRequest();

        var content = await SendRequestAsync(_posInfo.NonSecureUrl, postAuthRequest);

        var parseResponse = new IvpPostAuthResponse()
            .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = string.Empty;
            response.ResponseMessage = string.Empty;
            response.AuthCode = parseResponse.AuthCode;
            response.RrnNumber = parseResponse.TransId;
            response.OrderNumber = parseResponse.OrderId;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.TrxDate = parseResponse.TrxDate;
        }

        return response;
    }

    public async Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request)
    {
        var paymentDetailRequest = new IvpPaymentDetailRequest
        {
            ShopCode = _posInfo.ShopCode,
            UserCode = _posInfo.UserCode,
            UserPass = _posInfo.UserPass,
            OrgOrderId = request.OrderNumber,
            Lang = request.LanguageCode,
            SecureType = nameof(VposSecureType.NonSecure),
            TxnType = nameof(VposTransactionType.StatusHistory)
        }.BuildRequest();

        var content = await SendRequestAsync(_posInfo.NonSecureUrl, paymentDetailRequest);

        var parseResponse = new IvpPaymentDetailResponse()
            .Parse(content);

        var response = new PosPaymentDetailResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = string.Empty;
            response.ResponseMessage = string.Empty;
            response.AuthCode = parseResponse.AuthCode;
            response.RrnNumber = parseResponse.TransId;
            response.OrderNumber = parseResponse.OrderId;
            response.TransactionDate = parseResponse.TrxDate;
            response.Amount = parseResponse.PurchAmount;
            response.RefundedAmount = parseResponse.RefundedAmount;
            response.TrxDate = parseResponse.TrxDate;
            response.AuthCode = parseResponse.AuthCode;
            response.BatchNo = parseResponse.BatchNo;
            response.OrderStatus = GetOrderStatus(parseResponse);
        }
        else if (parseResponse.ProcReturnCode is OrderNotFoundCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = OrderNotFoundCode;
            response.ResponseMessage = string.Empty;
            response.OrderStatus = OrderStatus.OrderNotFound;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.OrderStatus = OrderStatus.Unknown;
        }

        return response;
    }

    public async Task<PosRefundResponse> Refund(PosRefundRequest request)
    {
        var refundRequest = new IvpRefundRequest
        {
            ShopCode = _posInfo.ShopCode,
            UserCode = _posInfo.UserCode,
            UserPass = _posInfo.UserPass,
            PurcAmount = FormatAmount(request.Amount),
            OrderId = request.OrderNumber,
            OrgOrderId = request.OrgOrderNumber,
            Moto = 0,
            Lang = request.LanguageCode,
            Currency = request.Currency,
            CardType = GetCardTypeNumber(request.CardBrand),
            SecureType = nameof(VposSecureType.NonSecure),
            TxnType = nameof(VposTransactionType.Refund),
            SubMerchantCode = request.SubMerchantId,
        }.BuildRequest();

        var content = await SendRequestAsync(_posInfo.NonSecureUrl, refundRequest);

        var parseResponse = new IvpRefundResponse()
            .Parse(content);

        var response = new PosRefundResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = SuccessCode;
            response.ResponseMessage = string.Empty;
            response.AuthCode = parseResponse.AuthCode;
            response.RrnNumber = parseResponse.TransId;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.TrxDate = parseResponse.TrxDate;
        }

        return response;
    }

    public async Task<PosVoidResponse> Void(PosVoidRequest request)
    {
        var voidRequest = new IvpVoidRequest
        {
            ShopCode = _posInfo.ShopCode,
            UserCode = _posInfo.UserCode,
            UserPass = _posInfo.UserPass,
            OrderId = request.OrderNumber,
            OrgOrderId = request.OrgOrderNumber,
            Moto = 0,
            Lang = request.LanguageCode,
            Currency = request.Currency,
            SecureType = nameof(VposSecureType.NonSecure),
            TxnType = nameof(VposTransactionType.Void)
        }.BuildRequest();

        var content = await SendRequestAsync(_posInfo.NonSecureUrl, voidRequest);

        var parseResponse = new IvpVoidResponse()
            .Parse(content);

        var response = new PosVoidResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = string.Empty;
            response.ResponseMessage = string.Empty;
            response.AuthCode = parseResponse.AuthCode;
            response.RrnNumber = parseResponse.TransId;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.TrxDate = parseResponse.TrxDate;
        }

        return response;
    }

    private static int GetCardTypeNumber(CardBrand cardBrand)
    {
        return cardBrand switch
        {
            CardBrand.Amex => 3,
            CardBrand.Visa => 0,
            CardBrand.Troy => 2,
            CardBrand.MasterCard => 1,
            CardBrand.UnionPay => 0,
            _ => 0,
        };
    }

    private static OrderStatus GetOrderStatus(IvpPaymentDetailResponse response)
    {
        if (response.ChargeTypeCd is "C")
        {
            return OrderStatus.SaleRefund;
        }

        if (response.ChargeTypeCd is not "S" and not "-")
        {
            return OrderStatus.Unknown;
        }
        else
        {
            if (response.RefundedAmount != 0)
            {
                return OrderStatus.Refunded;
            }

            return response.TxnStatus switch
            {
                "D" => OrderStatus.Rejected,
                "V" => OrderStatus.Cancelled,
                "A" => OrderStatus.PreAuth,
                "K" => OrderStatus.PostAuth,
                "S" => OrderStatus.EndOfDayCompleted,
                "C" => OrderStatus.WaitingEndOfDay,
                _ => OrderStatus.Unknown
            };
        }
    }


    protected override string FormatAmount(decimal amount)
    {
        return amount.ToString(new CultureInfo("en-US"));
    }

    protected override string FormatExpiryDate(string month, string year)
    {
        if (year.Length > 2)
        {
            return $"{month}{year.Substring(2, 2)}";
        }

        return $"{month}{year}";
    }

    private static Dictionary<string, object> GetFormParams(string formhtml)
    {
        var keyValues = new Dictionary<string, object>();

        if (string.IsNullOrEmpty(formhtml))
            return keyValues;

        MatchCollection match = Regex.Matches(formhtml, "<input.*name=\"(.*?)\".*value=\"(.*?)\".*>");

        if (match != null && match.Count > 0)
        {
            foreach (Match item in match)
            {
                if (!keyValues.ContainsKey(item.Groups[1].Value))
                    keyValues.Add(item.Groups[1].Value, item.Groups[2].Value);
            }
        }

        return keyValues;
    }

    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    private async Task<string> SendRequestAsync(string url, string data)
    {
        var correlationId = Guid.NewGuid();

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        await SendIntegrationRequest(data, correlationId);

        var response = await client.PostAsync(url, new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded"));

        await SendIntegrationResponse(response, correlationId);

        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();
        return Encoding.UTF8.GetString(bytes);
    }

    private async Task SendIntegrationRequest(string data, Guid correlationId)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.InterVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.InterVpos,
                    Type = nameof(IntegrationLogType.Vpos),
                    Date = DateTime.Now,
                    Request = data,
                    DataType = IntegrationLogDataType.Text
                };

                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.InterVpos} - Exception {exception}");
        }

    }

    private async Task SendIntegrationResponse(HttpResponseMessage data, Guid correlationId)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.InterVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var bytes = await data.Content.ReadAsByteArrayAsync();
                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.InterVpos,
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
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.InterVpos} - Exception {exception}");
        }

    }

    public void SetServiceParameters(object serviceParameters, Guid? merchantId = null)
    {
        _posInfo = (IvpPosInfo)serviceParameters;
    }
}