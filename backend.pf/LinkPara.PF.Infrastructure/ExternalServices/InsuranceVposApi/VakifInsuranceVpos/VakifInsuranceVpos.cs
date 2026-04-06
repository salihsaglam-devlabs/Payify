using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Request;
using LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Response;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos;

public class VakifInsuranceVpos : VposBase, IVposApi
{
    private const string SuccessCode = "0000";
    private const string SubMerchantType = "2";
    private const string TransactionDeviceSource = "0";

    private VakifInsuranceVposInfo _vakifPos;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;
    private readonly ILogger<VakifInsuranceVpos> _logger;
    
    public VakifInsuranceVpos(
        IParameterService parameterService,
        IBus bus,
        ILogger<VakifInsuranceVpos> logger)
    {
        _parameterService = parameterService;
        _bus = bus;
        _logger = logger;
    }
    public Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request)
    {
        throw new NotImplementedException();
    }
    
    public async Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request)
    {
        var paymentDetailRequest = new VakifInsurancePaymentDetailRequest
        {
            MerchantId = request.ServiceProviderPspMerchantId,
            Password = _vakifPos.Password,
            TransactionId = request.OrderNumber,
            MerchantType = SubMerchantType,
            StartDate = request.StartDate.ToString("yyyy-MM-dd"),
            EndDate = request.EndDate.ToString("yyyy-MM-dd")
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.SearchUrl, paymentDetailRequest);

        var parseResponse = new VakifInsurancePaymentDetailResponse()
            .Parse(content);

        var response = new PosPaymentDetailResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;

            if (parseResponse.TotalItemCount == 0)
            {
                response.OrderStatus = OrderStatus.OrderNotFound;
            }
            else
            {
                response.AuthCode = parseResponse.AuthCode;
                response.TransId = parseResponse.TransactionId;
                response.TransactionDate = ParseHostDate(parseResponse.HostDate);
                response.Amount = FormatResponseAmount(parseResponse.Amount);
                response.RefundedAmount = FormatResponseAmount(parseResponse.TotalRefundAmount);
                response.TrxDate = ParseHostDate(parseResponse.HostDate);
                response.AuthCode = parseResponse.AuthCode;
                response.RrnNumber = parseResponse.Rrn;
                response.OrderStatus = GetOrderStatus(parseResponse);
            }
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.OrderStatus = OrderStatus.Unknown;
        }

        return response;
    }

    private static DateTime ParseHostDate(string hostDate)
    {
        if (DateTime.TryParseExact(hostDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            return parsedDate;
        }
        return DateTime.MinValue;
    }
    public async Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request)
    {
        var installmentCount = string.Empty;
        if (request.Installment is not null && request.Installment > 1)
        {
            installmentCount = request.Installment.Value.ToString("D2");
        }
        
        if (string.IsNullOrWhiteSpace(request.CardNumber) || request.CardNumber.Length < 12)
        {
            throw new ArgumentException("Invalid card number", nameof(request.CardNumber));
        }

        var authRequest = new VakifInsurancePaymentNonSecureRequest
        {
            MerchantId = _vakifPos.MerchantId,
            Password = _vakifPos.Password,
            TerminalNo = _vakifPos.TerminalNo,
            TransactionType = "Sale",
            InquiryValue = request.CardHolderIdentityNumber,
            CurrencyCode = request.Currency,
            CurrencyAmount = FormatAmount(request.Amount),
            CardNoFirst = request.CardNumber[..8],
            CardNoLast = request.CardNumber[^4..],
            ClientIp = request.ClientIp,
            TransactionDeviceSource = TransactionDeviceSource,
            TransactionId = request.OrderNumber,
            MerchantType = SubMerchantType,
            InstallmentCount = installmentCount,
            HostSubMerchantId = request.ServiceProviderPspMerchantId
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.ThreeDSecureUrl, authRequest);
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
        
        var parseResponse = JsonSerializer.Deserialize<VakifInsurancePaymentNonSecureResponse>(content, options);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.TransactionId;
            response.TrxDate = ParseHostDate(parseResponse.HostDate);
            response.RrnNumber = parseResponse.Rrn;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.TrxDate = ParseHostDate(parseResponse.HostDate);
        }
        return response;
    }

    public Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<PosRefundResponse> Refund(PosRefundRequest request)
    {
        var refundRequest = new VakifInsuranceRefundInnerRequest
        {
            MerchantId = _vakifPos.MerchantId,
            Password = _vakifPos.Password,
            TransactionType = "Refund",
            CurrencyAmount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            ReferenceTransactionId = request.OrgOrderNumber,
            ClientIp = request.ClientIp,
            HostSubMerchantId = request.ServiceProviderPspMerchantId,
            TransactionId = request.OrderNumber,
            MerchantType = SubMerchantType
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.EnrollmentUrl, refundRequest);

        var parseResponse = new VakifInsuranceRefundResponse().Parse(content);

        var response = new PosRefundResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.TransactionId;
            response.TrxDate = ParseHostDate(parseResponse.HostDate);
            response.RrnNumber = parseResponse.Rrn;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.TrxDate = ParseHostDate(parseResponse.HostDate);
        }

        return response;
    }

    public Task<PosVerify3dModelResponse> Verify3DModel(Dictionary<string, string> form)
    {
        throw new NotImplementedException();
    }

    public async Task<PosVoidResponse> Void(PosVoidRequest request)
    {
        var voidRequest = new VakifInsuranceVoidInnerRequest
        {
            MerchantId = _vakifPos.MerchantId,
            Password = _vakifPos.Password,
            ReferenceTransactionId = request.OrgOrderNumber,
            TransactionType = "Cancel",
            ClientIp = request.ClientIp,
            HostSubMerchantId = request.ServiceProviderPspMerchantId,
            MerchantType = SubMerchantType
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.EnrollmentUrl, voidRequest);

        var parseResponse = new VakifInsuranceVoidResponse().Parse(content);

        var response = new PosVoidResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.TransactionId;
            response.TrxDate = ParseHostDate(parseResponse.HostDate);
            response.RrnNumber = parseResponse.Rrn;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.TrxDate = ParseHostDate(parseResponse.HostDate);
        }

        return response;
    }

    protected override string FormatAmount(decimal amount)
    {
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero)
            .ToString("0.00", new CultureInfo("en-US"));
    }
    private decimal FormatResponseAmount(string amount)
    {
        return !string.IsNullOrEmpty(amount) ? decimal.Parse(amount, CultureInfo.InvariantCulture) : 0;
    }
    protected override string FormatExpiryDate(string month, string year)
    {
        var date = $"{DateTime.Now.Year.ToString().Substring(0, 2)}{year}{month}";
        return date;
    }
    
    private async Task<string> SendVakifRequestAsync(string url, Dictionary<string, string> data)
    {
        var correlationId = Guid.NewGuid();

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        await SendIntegrationRequest(JsonSerializer.Serialize(data), correlationId, IntegrationLogDataType.Json);

        var response = await client.PostAsync(url, new FormUrlEncodedContent(data));

        await SendIntegrationResponse(response, correlationId);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }
    
    private async Task<string> SendVakifRequestAsync(string url, string data)
    {
        var correlationId = Guid.NewGuid();

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        await SendIntegrationRequest(data, correlationId, IntegrationLogDataType.Soap);

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(data), "prmstr");

        var response = await client.PostAsync(url, content);

        await SendIntegrationResponse(response, correlationId);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }

    private async Task SendIntegrationRequest(string data, Guid correlationId, IntegrationLogDataType integrationLogDataType)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.VakifInsuranceVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.VakifInsuranceVpos,
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
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.VakifInsuranceVpos} - Exception {exception}");
        }

    }

    private async Task SendIntegrationResponse(HttpResponseMessage httpResponse, Guid correlationId)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.VakifInsuranceVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.VakifInsuranceVpos,
                    Type = nameof(IntegrationLogType.Vpos),
                    Date = DateTime.Now,
                    Response = await httpResponse.Content.ReadAsStringAsync(),
                    HttpCode = ((int)httpResponse.StatusCode).ToString(),
                    ErrorCode = httpResponse.StatusCode.ToString(),
                    ErrorMessage = httpResponse.StatusCode.ToString(),
                    DataType = IntegrationLogDataType.Soap
                };

                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.VakifInsuranceVpos} - Exception {exception}");
        }
        
    }
    public void SetServiceParameters(object serviceParameters, Guid? merchantId = null)
    {
        _vakifPos = (VakifInsuranceVposInfo)serviceParameters;
    }
    private static OrderStatus GetOrderStatus(VakifInsurancePaymentDetailResponse paymentDetail)
    {
        if (paymentDetail.IsRefunded)
        {
            return OrderStatus.Refunded;
        }

        if (paymentDetail.IsCanceled || paymentDetail.IsReversed)
        {
            return OrderStatus.Cancelled;
        }

        if (paymentDetail.IsCaptured)
        {
            return OrderStatus.PostAuth;
        }

        if (paymentDetail.TransactionType == "SaleRefund")
        {
            return OrderStatus.SaleRefund;
        }

        if (paymentDetail.TransactionType == "Auth")
        {
            return OrderStatus.PreAuth;
        }

        if (paymentDetail.TransactionType == "Sale")
        {
            var parsedHostDate = ParseHostDate(paymentDetail.HostDate);
            return parsedHostDate == DateTime.Now.Date
                                         ? OrderStatus.WaitingEndOfDay
                                         : OrderStatus.EndOfDayCompleted;
        }

        return OrderStatus.Unknown;
    }
}