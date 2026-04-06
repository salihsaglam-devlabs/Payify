using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Request;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Response;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos;

public class VakifVpos : VposBase, IVposApi
{
    private const string SuccessCode = "0000";
    private const string FullSecureStatusCode = "Y";
    private const string HalfSecureStatusCode = "A";
    private const string MerchantType = "2";
    private const string TransactionDeviceSource = "0";

    private VakifPosInfo _vakifPos;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;
    private readonly ILogger<VakifVpos> _logger;
    public VakifVpos(
        IParameterService parameterService,
        IBus bus,
        ILogger<VakifVpos> logger)
    {
        _parameterService = parameterService;
        _bus = bus;
        _logger = logger;
    }
    public async Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request)
    {
        var cardBrand = GetCardBrand(request.CardBrand);

        var enrollmentRequest = new VakifEnrollmentRequest
        {
            MerchantId = _vakifPos.MerchantId,
            Password = _vakifPos.Password,
            TransactionId = request.OrderNumber,
            CurrencyAmount = FormatAmount(request.Amount),
            OrderId = request.OrderNumber,
            CurrencyCode = request.Currency,
            Cvv = request.Cvv2,
            Expiry = $"{request.ExpireYear}{request.ExpireMonth}",
            NumberOfInstallments = request.Installment,
            Pan = request.CardNumber,
            BrandName = cardBrand,
            SuccessUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            FailureUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            HostSubMerchantId = request.ServiceProviderPspMerchantId,
            MerchantType = request.IsTopUpPayment == true ? "1" : MerchantType,
            IsTopUpPayment = request.IsTopUpPayment,
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.EnrollmentUrl, enrollmentRequest);

        if (string.IsNullOrEmpty(content))
        {
            return new PosInit3DModelResponse { IsSuccess = false, HtmlContent = string.Empty };
        }

        var parseResponse = new VakifEnrollmentResponse()
           .Parse(content);

        if (parseResponse.Status == HalfSecureStatusCode || parseResponse.Status == FullSecureStatusCode)
        {
            return new PosInit3DModelResponse
            {
                IsSuccess = true,
                HtmlContent = Base64Encode(parseResponse),
                ResponseCode = string.Empty,
                ResponseMessage = string.Empty,
            };
        }
        else
        {
            return new PosInit3DModelResponse
            {
                IsSuccess = false,
                ResponseCode = parseResponse.MessageErrorCode,
                ResponseMessage = string.Empty,
                HtmlContent = Base64Encode(parseResponse)
            };
        }
    }

    public async Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request)
    {
        var expiryDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear);
        var authRequest = new VakifPayment3DModelRequest
        {
            MerchantId = _vakifPos.MerchantId,
            Password = _vakifPos.Password,
            Pan = request.CardNumber,
            Expiry = expiryDate,
            CurrencyAmount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            TransactionId = request.OrderNumber,
            NumberOfInstallments = request.Installment ?? 1,
            TransactionDeviceSource = TransactionDeviceSource,
            HostSubMerchantId = request.ServiceProviderPspMerchantId,
            TransactionType = request.AuthType == VposAuthType.PreAuth ? "Auth" : "Sale",
            Eci = request.Eci,
            Cavv = request.Cavv,
            MpiTransactionId = request.PayerTxnId,
            ClientIp = request.ClientIp,
            MerchantType = request.IsTopUpPayment == true ? "1" : MerchantType,
            TerminalNo = request.IsTopUpPayment == true ? _vakifPos.TerminalNo : request.SubMerchantCode,
            IsTopUpPayment = request.IsTopUpPayment,
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.ThreeDSecureUrl, authRequest);

        var parseResponse = new VakifPayment3DModelResponse()
           .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.TransactionId;
            response.TrxDate = parseResponse.HostDate;
            response.RrnNumber = parseResponse.Rrn;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.TrxDate = parseResponse.HostDate;
        }
        return response;
    }

    public async Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request)
    {
        var paymentDetailRequest = new VakifPaymentDetailRequest
        {
            MerchantId = request.ServiceProviderPspMerchantId,
            Password = _vakifPos.Password,
            TransactionId = request.OrderNumber,
            MerchantType = MerchantType,
            StartDate = request.StartDate.ToString("yyyy-MM-dd"),
            EndDate = request.EndDate.ToString("yyyy-MM-dd")
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.SearchUrl, paymentDetailRequest);

        var parseResponse = new VakifPaymentDetailResponse()
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
                response.TransactionDate = parseResponse.HostDate;
                response.Amount = FormatResponseAmount(parseResponse.Amount);
                response.RefundedAmount = FormatResponseAmount(parseResponse.TotalRefundAmount);
                response.TrxDate = parseResponse.HostDate;
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

    public async Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request)
    {
        var expiryDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear);
        var authRequest = new VakifPaymentNonSecureRequest
        {
            MerchantId = _vakifPos.MerchantId,
            Password = _vakifPos.Password,
            TerminalNo = request.SubMerchantCode,
            Pan = request.CardNumber,
            Expiry = expiryDate,
            MerchantType = MerchantType,
            CurrencyAmount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            TransactionId = request.OrderNumber,
            NumberOfInstallments = request.Installment ?? 1,
            Cvv = request.Cvv2,
            TransactionDeviceSource = TransactionDeviceSource,
            HostSubMerchantId = request.ServiceProviderPspMerchantId,
            TransactionType = request.AuthType == VposAuthType.PreAuth ? "Auth" : "Sale",
            ClientIp = request.ClientIp
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.ThreeDSecureUrl, authRequest);

        var parseResponse = new VakifPaymentNonSecureResponse()
            .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.TransactionId;
            response.TrxDate = parseResponse.HostDate;
            response.RrnNumber = parseResponse.Rrn;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.TrxDate = parseResponse.HostDate;
        }
        return response;
    }

    public async Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request)
    {
        var postAuthRequest = new VakifPointInquiryRequest
        {
            MerchantId = _vakifPos.MerchantId,
            Password = _vakifPos.Password,
            TerminalNo = request.SubMerchantCode,
            Cvv = request.Cvv2,
            Expiry = FormatExpiryDate(request.ExpireMonth, request.ExpireYear),
            Pan = request.CardNumber,
            TransactionType = "PointSearch",
            ClientIp = request.ClientIp,
            TransactionDeviceSource = TransactionDeviceSource,
            HostSubMerchantId = request.ServiceProviderPspMerchantId,
            MerchantType = MerchantType
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.ThreeDSecureUrl, postAuthRequest);

        var parseResponse = new VakifPointInquiryResponse()
           .Parse(content);

        var response = new PosPointInquiryResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.TransactionId;
            response.AvailablePoint = FormatResponseAmount(parseResponse.TotalPoint);
            response.TrxDate = parseResponse.HostDate;
            response.RrnNumber = parseResponse.Rrn;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.TrxDate = parseResponse.HostDate;
        }

        return response;
    }

    public async Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request)
    {
        var postAuthRequest = new VakifPostAuthRequest
        {
            MerchantId = _vakifPos.MerchantId,
            Password = _vakifPos.Password,
            TerminalNo = request.SubMerchantCode,
            CurrencyAmount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            ReferenceTransactionId = request.OrgOrderNumber,
            MerchantType = MerchantType,
            HostSubMerchantId = request.ServiceProviderPspMerchantId,
            TransactionType = "Capture",
            TransactionId = request.OrderNumber,
            ClientIp = request.ClientIp
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.ThreeDSecureUrl, postAuthRequest);

        var parseResponse = new VakifPostAuthResponse()
           .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.TransactionId;
            response.TrxDate = parseResponse.HostDate;
            response.RrnNumber = parseResponse.Rrn;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.TrxDate = parseResponse.HostDate;
        }

        return response;
    }

    public async Task<PosRefundResponse> Refund(PosRefundRequest request)
    {
        var refundRequest = new VakifRefundRequest
        {
            MerchantId = _vakifPos.MerchantId,
            Password = _vakifPos.Password,
            CurrencyAmount = FormatAmount(request.Amount),
            TransactionId = request.OrderNumber,
            ReferenceTransactionId = request.OrgOrderNumber,
            HostSubMerchantId = request.ServiceProviderPspMerchantId,
            TransactionType = nameof(VposTransactionType.Refund),
            ClientIp = request.ClientIp,
            MerchantType = request.IsTopUpPayment == true ? "1" : MerchantType,
            IsTopUpPayment = request.IsTopUpPayment,
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.ThreeDSecureUrl, refundRequest);

        var parseResponse = new VakifRefundResponse()
            .Parse(content);

        var response = new PosRefundResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.TransactionId;
            response.TrxDate = parseResponse.HostDate;
            response.RrnNumber = parseResponse.Rrn;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.TrxDate = parseResponse.HostDate;
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

        var formString = string.Join(", ", form.Select(kv => $"{kv.Key}: {kv.Value}"));
        _logger.LogError($"Verify3dModel {formString}");

        form.TryGetValue("Status", out string status);
        form.TryGetValue("MerchantId", out string merchantId);
        form.TryGetValue("Xid", out string orderId);
        form.TryGetValue("Cavv", out string Cavv);
        form.TryGetValue("Eci", out string eci);
        form.TryGetValue("VerifyEnrollmentRequestId", out string verifyEnrollmentRequestId);
        form.TryGetValue("PurchAmount", out string purchAmount);
        form.TryGetValue("SessionInfo", out string sessionInfo);
        form.TryGetValue("PurchCurrency", out string purchCurrency);
        form.TryGetValue("Pan", out string pan);
        form.TryGetValue("Expiry", out string expiry);
        form.TryGetValue("InstallmentCount", out string installmentCount);
        form.TryGetValue("ErrorCode", out string errorCode);
        form.TryGetValue("ErrorMessage", out string errorMessage);
        form.TryGetValue("MD", out string md);

        response.MdStatus = status == FullSecureStatusCode ? "1" : status == HalfSecureStatusCode ? "2" : "0";
        response.MdErrorMessage = errorMessage;
        response.TxnStat = status;

        if (string.IsNullOrEmpty(status))
        {
            response.ResponseCode = errorCode;
            response.ResponseMessage = errorMessage;
            return await Task.FromResult(response);
        }

        if (status != HalfSecureStatusCode && status != FullSecureStatusCode)
        {
            response.ResponseCode = "";
            if (!string.IsNullOrEmpty(errorCode))
            {
                response.ResponseMessage = errorCode;
            }
            response.ResponseMessage = errorMessage;
            return await Task.FromResult(response);
        }

        response.Cavv = Cavv;
        response.MD = md;
        response.Eci = eci;
        response.OrderNumber = orderId;                
        response.PayerTxnId = verifyEnrollmentRequestId;
        response.TxnStat = status;
        response.IsSuccess = true;        

        return await Task.FromResult(response);
    }

    public async Task<PosVoidResponse> Void(PosVoidRequest request)
    {
        var voidRequest = new VakifVoidRequest
        {
            MerchantId = _vakifPos.MerchantId,
            Password = _vakifPos.Password,
            ReferenceTransactionId = request.OrgOrderNumber,
            HostSubMerchantId = request.ServiceProviderPspMerchantId,
            TransactionType = "Cancel",
            ClientIp = request.ClientIp,
            MerchantType = request.IsTopUpPayment == true ? "1" : MerchantType,
            IsTopUpPayment = request.IsTopUpPayment,
        }.BuildRequest();

        var content = await SendVakifRequestAsync(_vakifPos.ThreeDSecureUrl, voidRequest);

        var parseResponse = new VakifVoidResponse()
          .Parse(content);

        var response = new PosVoidResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.TransactionId;
            response.TrxDate = parseResponse.HostDate;
            response.RrnNumber = parseResponse.Rrn;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.TrxDate = parseResponse.HostDate;
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
    private static string Base64Encode(VakifEnrollmentResponse response)
    {
        string postBackForm =
                  @"<html>
                          <head>
                            <meta name=""viewport"" content=""width=device-width"" />
                            <title>MpiForm</title>
                            <script>
                              function postPage() {
                              document.forms[""frmMpiForm""].submit();
                              }
                            </script>
                          </head>
                          <body onload=""javascript:postPage();"">
                            <form action=""@ACSUrl"" method=""post"" id=""frmMpiForm"" name=""frmMpiForm"">
                              <input type=""hidden"" name=""PaReq"" value=""@PAReq"" />
                              <input type=""hidden"" name=""TermUrl"" value=""@TermUrl"" />
                              <input type=""hidden"" name=""MD"" value=""@MD "" />
                              <noscript>
                                <input type=""submit"" id=""btnSubmit"" value=""Gönder"" />
                              </noscript>
                            </form>
                          </body>
                        </html>";

        postBackForm = postBackForm.Replace("@ACSUrl", response.ACSUrl);
        postBackForm = postBackForm.Replace("@PAReq", response.PAReq);
        postBackForm = postBackForm.Replace("@TermUrl", response.TermURL);
        postBackForm = postBackForm.Replace("@MD", response.MD);

        var plainTextBytes = Encoding.UTF8.GetBytes(postBackForm);
        return Convert.ToBase64String(plainTextBytes);
    }
    private string GetCardBrand(CardBrand cardBrand)
    {
        return cardBrand switch
        {
            CardBrand.Visa => "100",
            CardBrand.MasterCard => "200",
            CardBrand.Troy => "300",
            CardBrand.Amex => "400",
            _ => "0"
        };
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
    private async Task SendIntegrationRequest(string data, Guid correlationId, IntegrationLogDataType integrationLogDataType)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.VakifVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.VakifVpos,
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
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.VakifVpos} - Exception {exception}");
        }

    }

    private async Task SendIntegrationResponse(HttpResponseMessage httpResponse, Guid correlationId)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.VakifVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.VakifVpos,
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
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.VakifVpos} - Exception {exception}");
        }
        
    }
    public void SetServiceParameters(object serviceParameters, Guid? merchantId = null)
    {
        _vakifPos = (VakifPosInfo)serviceParameters;
    }
    private static OrderStatus GetOrderStatus(VakifPaymentDetailResponse paymentDetail)
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
            return paymentDetail.HostDate.Date == DateTime.Now.Date
                                         ? OrderStatus.WaitingEndOfDay
                                         : OrderStatus.EndOfDayCompleted;
        }

        return OrderStatus.Unknown;
    }
}
