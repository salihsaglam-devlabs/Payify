using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Request;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Response;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos;

public class FinansVpos : VposBase, IVposApi
{
    private const string ThreeDSuccessCode = "V033";
    private const string SuccessCode = "00";
    private const string MbrId = "5";
    private const string Auth3DModel = "3DModel";
    private const string Auth3DModelPayment = "3DModelPayment";

    private FinansPosInfo _finansPos;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;
    private readonly ILogger<FinansVpos> _logger;
    public FinansVpos(
        IParameterService parameterService,
        IBus bus,
        ILogger<FinansVpos> logger)
    {
        _parameterService = parameterService;
        _bus = bus;
        _logger = logger;
    }
    protected override string FormatAmount(decimal amount)
    {
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero)
        .ToString(new CultureInfo("en-US"));
    }

    protected override string FormatExpiryDate(string month, string year)
    {
        var date = $"{month}{year}";
        return date;
    }

    public async Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request)
    {
        var expiryDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear);
        string AuthType = "";
        if (request.BonusAmount > 0)
        {
            AuthType = "ParaPuanAuth";
        }
        else
        {
            AuthType = request.AuthType == VposAuthType.PreAuth ? "PreAuth" : "Auth";
        }
        var init3DRequest = await new FinansInit3dModelRequest
        {
            MbrId = MbrId,
            MerchantId = _finansPos.MerchantId,
            UserCode = _finansPos.UserCode,
            UserPass = _finansPos.UserPass,
            MerchantPass = _finansPos.MerchantPass,
            SecureType = Auth3DModel,
            TxnType = AuthType,
            InstallmentCount = request.Installment > 1 ? request.Installment : 0,
            CurrencyCode = request.Currency,
            OkUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            FailUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            PostUrl = _finansPos.PostUrl,
            OrderId = request.OrderNumber,
            PurchAmount = FormatAmount(request.Amount),
            BonusAmount = request.BonusAmount.ToString(),
            LanguageCode = request.LanguageCode,
            CardHolderName = request.CardHolderName,
            Pan = request.CardNumber,
            Expiry = expiryDate,
            MOTO = "0",
            Cvv2 = request.Cvv2,
            SubmerchantId = request.SubMerchantCode,
            PaymentFacilitatorId = _finansPos.PaymentFacilitatorId,
            SubmerchantMcc = request.SubMerchantMcc,
            CardAcceptorName = CleanUnicodeCharacters(request.SubMerchantName),
            CardAcceptorStreet = CleanUnicodeCharacters(request.SubmerchantAddress),
            CardAcceptorCity = CleanUnicodeCharacters(request.SubMerchantCity),
            CardAcceptorPostalCode = request.SubMerchantPostalCode,
            CardAcceptorState = CleanUnicodeCharacters(request.SubmerchantDistrict),
            CardAcceptorCountry = request.SubMerchantCountry
        }.BuildRequest();

        await SendIntegrationRequest(init3DRequest, Guid.NewGuid(), IntegrationLogDataType.Soap);

        return new PosInit3DModelResponse
        {
            IsSuccess = true,
            HtmlContent = Base64Encode(init3DRequest),
            ResponseCode = string.Empty,
            ResponseMessage = string.Empty,
        };
    }

    public async Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request)
    {
        var authRequest = new FinansPayment3DModelRequest
        {
            UserCode = _finansPos.UserCode,
            UserPass = _finansPos.UserPass,
            SecureType = Auth3DModelPayment,
            OrderId = request.OrderNumber,
            RequestGuid = request.PayerTxnId,
        }.BuildRequest();

        var content = await SendFinansRequestAsync(_finansPos.NonSecureUrl, authRequest);

        var parseResponse = new FinansPayment3DModelResponse()
            .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderId;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
        }
        return response;
    }

    public async Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request)
    {
        var paymentDetailRequest = new FinansPaymentDetailRequest
        {
            MbrId = MbrId,
            MerchantId = _finansPos.MerchantId,
            UserCode = _finansPos.UserCode,
            UserPass = _finansPos.UserPass,
            SecureType = "Inquiry",
            TxnType = "OrderInquiry",
            OrderId = request.OrderNumber,
            CurrencyCode = request.Currency,
            LanguageCode = request.LanguageCode
        }.BuildRequest();

        var content = await SendFinansRequestAsync(_finansPos.NonSecureUrl, paymentDetailRequest);

        var parseResponse = new FinansPaymentDetailResponse()
            .Parse(content);

        var response = new PosPaymentDetailResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderStatus = GetOrderStatus(parseResponse);
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

    public async Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request)
    {
        var expiryDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear);
        string AuthType = "";
        if (request.BonusAmount > 0)
        {
            AuthType = "ParaPuanAuth";
        }
        else
        {
            AuthType = request.AuthType == VposAuthType.PreAuth ? "PreAuth" : "Auth";
        }
        var authRequest = new FinansPaymentNonSecureRequest
        {
            MbrId = MbrId,
            MerchantId = _finansPos.MerchantId,
            UserCode = _finansPos.UserCode,
            UserPass = _finansPos.UserPass,
            LanguageCode = request.LanguageCode,
            CardHolderName = request.SubMerchantName,
            SecureType = nameof(VposSecureType.NonSecure),
            TxnType = AuthType,
            Pan = request.CardNumber,
            Expiry = expiryDate,
            MOTO = "0",
            PurchAmount = FormatAmount(request.Amount),
            BonusAmount = request.BonusAmount.ToString(),
            CurrencyCode = request.Currency,
            OrderId = request.OrderNumber,
            InstallmentCount = request.Installment > 1 ? request.Installment : 0,
            Cvv2 = request.Cvv2,
            SubmerchantId = request.SubMerchantCode,
            PaymentFacilitatorId = _finansPos.PaymentFacilitatorId,
            SubmerchantMcc = request.SubMerchantMcc,
            CardAcceptorName = request.SubMerchantName,
            CardAcceptorStreet = request.SubMerchantAddress,
            CardAcceptorCity = request.SubMerchantCity,
            CardAcceptorPostalCode = request.SubMerchantPostalCode,
            CardAcceptorState = request.SubMerchantDistrict,
            CardAcceptorCountry = request.SubMerchantCountry
        }.BuildRequest();

        var content = await SendFinansRequestAsync(_finansPos.NonSecureUrl, authRequest);

        var parseResponse = new FinansPaymentNonSecureResponse()
            .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderId;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
        }
        return response;
    }

    public async Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request)
    {
        var pointInquiryRequest = new FinansPointInquiryRequest
        {
            MbrId = MbrId,
            MerchantId = _finansPos.MerchantId,
            UserCode = _finansPos.UserCode,
            UserPass = _finansPos.UserPass,
            SecureType = "Inquiry",
            CurrencyCode = request.Currency,
            Pan = request.CardNumber,
            OrderId = request.OrderNumber,
            TxnType = "ParaPuanInquiry",
            LanguageCode = request.LanguageCode,
        }.BuildRequest();

        var content = await SendFinansRequestAsync(_finansPos.NonSecureUrl, pointInquiryRequest);

        var parseResponse = new FinansPointInquiryResponse()
           .Parse(content);

        var response = new PosPointInquiryResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.AvailablePoint = parseResponse.AvailablePoint;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
        }

        return response;
    }

    public async Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request)
    {
        var postAuthRequest = new FinansPostAuthRequest
        {
            MbrId = MbrId,
            MerchantId = _finansPos.MerchantId,
            UserCode = _finansPos.UserCode,
            UserPass = _finansPos.UserPass,
            LanguageCode = request.LanguageCode,
            SecureType = nameof(VposSecureType.NonSecure),
            TxnType = nameof(VposTransactionType.PostAuth),
            PurchAmount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            OrderId = request.OrgOrderNumber,
            SubmerchantId = request.SubMerchantCode,
            PaymentFacilitatorId = _finansPos.PaymentFacilitatorId,
            SubmerchantMcc = request.SubMerchantMcc,
            CardAcceptorName = request.SubMerchantName,
            CardAcceptorStreet = request.SubMerchantAddress.Substring(0, Math.Min(request.SubMerchantAddress.Length, 32)),
            CardAcceptorCity = request.SubMerchantCity,
            CardAcceptorPostalCode = request.SubMerchantPostalCode,
            CardAcceptorState = request.SubMerchantDistrict,
            CardAcceptorCountry = request.SubMerchantCountry
        }.BuildRequest();

        var content = await SendFinansRequestAsync(_finansPos.NonSecureUrl, postAuthRequest);

        var parseResponse = new FinansPostAuthResponse()
            .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderId;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
        }
        return response;
    }

    public async Task<PosRefundResponse> Refund(PosRefundRequest request)
    {
        var refundRequest = new FinansRefundRequest
        {
            MbrId = MbrId,
            MerchantId = _finansPos.MerchantId,
            UserCode = _finansPos.UserCode,
            UserPass = _finansPos.UserPass,
            OrgOrderId = request.OrgOrderNumber,
            SecureType = nameof(VposSecureType.NonSecure),
            TxnType = nameof(VposTransactionType.Refund),
            PurchAmount = FormatAmount(request.Amount),
            Currency = request.Currency,
            Lang = request.LanguageCode
        }.BuildRequest();

        var content = await SendFinansRequestAsync(_finansPos.NonSecureUrl, refundRequest);

        var parseResponse = new FinansRefundResponse()
            .Parse(content);

        var response = new PosRefundResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderId;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
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

        form.TryGetValue("3DStatus", out string status);
        form.TryGetValue("AuthCode", out string authCode);
        form.TryGetValue("ProcReturnCode", out string procReturnCode);
        form.TryGetValue("ResponseRnd", out string responseRnd);
        form.TryGetValue("ResponseHash", out string responseHash);
        form.TryGetValue("OrderId", out string orderId);
        form.TryGetValue("Cavv", out string Cavv);
        form.TryGetValue("Eci", out string eci);
        form.TryGetValue("ErrorCode", out string errorCode);
        form.TryGetValue("ErrMsg", out string errorMessage);
        form.TryGetValue("RequestGuid", out string requestGuid);
        form.TryGetValue("D3Stat", out string d3stat);
        form.TryGetValue("MD", out string md);

        response.MdStatus = status;
        response.MdErrorMessage = errorMessage;
        response.TxnStat = d3stat;

        if (string.IsNullOrEmpty(status))
        {
            response.ResponseCode = errorCode;
            response.ResponseMessage = errorMessage;
            return await Task.FromResult(response);
        }

        if (status != "1")
        {
            response.ResponseCode = "";
            if (!string.IsNullOrEmpty(errorCode))
            {
                response.ResponseMessage = errorCode;
            }
            response.ResponseMessage = errorMessage;
            return await Task.FromResult(response);
        }

        if (procReturnCode != ThreeDSuccessCode)
        {
            response.ResponseCode = errorCode;
            response.ResponseMessage = errorMessage;
            return await Task.FromResult(response);
        }

        string str = _finansPos.MerchantId + _finansPos.MerchantPass + orderId + authCode + procReturnCode + status + responseRnd + _finansPos.UserCode;
        SHA1 sha = SHA1.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        byte[] hashingbytes = sha.ComputeHash(bytes);
        string calculatedHash = Convert.ToBase64String(hashingbytes);

        if (responseHash != calculatedHash)
        {
            response.Hash = responseHash;
            response.ResponseCode = "";
            response.ResponseMessage = "Invalid Hash";
            return await Task.FromResult(response);
        }

        if (eci == "05" || eci == "02")
        {
            response.MdStatus = "1";
            response.TxnStat = "Y";
        }
        else if (eci == "06" || eci == "01")
        {
            response.MdStatus = "2";
            response.TxnStat = "A";
        }
        else
        {
            response.MdStatus = "0";
            response.TxnStat = "N";
        }

        response.PayerTxnId = requestGuid;
        response.MD = md;
        response.Cavv = Cavv;
        response.Eci = eci;
        response.OrderNumber = orderId;
        response.IsSuccess = true;

        return await Task.FromResult(response);
    }

    public async Task<PosVoidResponse> Void(PosVoidRequest request)
    {
        var voidRequest = new FinansVoidRequest
        {
            MbrId = MbrId,
            MerchantId = _finansPos.MerchantId,
            UserCode = _finansPos.UserCode,
            UserPass = _finansPos.UserPass,
            OrgOrderId = request.OrgAuthProcessOrderNo,
            Lang = request.LanguageCode,
            Currency = request.Currency,
            SecureType = nameof(VposSecureType.NonSecure),
            TxnType = nameof(VposTransactionType.Void)
        }.BuildRequest();

        var content = await SendFinansRequestAsync(_finansPos.NonSecureUrl, voidRequest);

        var parseResponse = new FinansVoidResponse()
            .Parse(content);

        var response = new PosVoidResponse();

        if (parseResponse.ProcReturnCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderNumber = parseResponse.OrderId;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ProcReturnCode;
            response.ResponseMessage = parseResponse.ErrorMessage;
        }

        return response;
    }

    public void SetServiceParameters(object serviceParameters, Guid? merchantId = null)
    {
        _finansPos = (FinansPosInfo)serviceParameters;
    }
    private async Task<string> SendFinansRequestAsync(string url, string data)
    {
        var correlationId = Guid.NewGuid();

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        await SendIntegrationRequest(data, correlationId, IntegrationLogDataType.Soap);

        var response = await client.PostAsync(url, new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded"));

        await SendIntegrationResponse(response, correlationId);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }
    private async Task SendIntegrationRequest(string data, Guid correlationId, IntegrationLogDataType integrationLogDataType)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync(VposConsts.ParameterGroupCode, VposConsts.FinansVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.FinansVpos,
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
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.FinansVpos} - Exception {exception}");
        }

    }
    private async Task SendIntegrationResponse(HttpResponseMessage data, Guid correlationId)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.FinansVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var bytes = await data.Content.ReadAsByteArrayAsync();
                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.FinansVpos,
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
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.FinansVpos} - Exception {exception}");
        }

    }
    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
    private static OrderStatus GetOrderStatus(FinansPaymentDetailResponse paymentDetail)
    {
        if (paymentDetail.IsRefunded)
        {
            return OrderStatus.Refunded;
        }

        if (paymentDetail.IsVoided)
        {
            return OrderStatus.Cancelled;
        }

        if (paymentDetail.TxnType == "PostAuth")
        {
            return OrderStatus.PostAuth;
        }

        if (paymentDetail.TxnType == "PreAuth")
        {
            return OrderStatus.PreAuth;
        }

        if (paymentDetail.TxnResult == "Failed")
        {
            return OrderStatus.Rejected;
        }

        if (paymentDetail.TxnType == "Auth")
        {
            return paymentDetail.TxnDate == DateTime.Now.Date
                                             ? OrderStatus.WaitingEndOfDay
                                             : OrderStatus.EndOfDayCompleted;
        }

        return OrderStatus.Unknown;
    }
    private string CleanUnicodeCharacters(string input)
    {
        var unaccentedMessage = String.Join("", input.Normalize(NormalizationForm.FormD)
             .Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        return unaccentedMessage.Replace("ı", "i");
    }
}
