using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Response;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos
{
    public class PosnetVpos : VposBase, IVposApi
    {
        private const string SuccessCode = "1";
        private readonly string ThreeDFullSecureStatus = "Y";
        private readonly string ThreeDHalfSecureStatus = "A";

        private PosnetPosInfo _posNetPos;
        private readonly IParameterService _parameterService;
        private readonly IBus _bus;
        private readonly ILogger<PosnetVpos> _logger;
        public PosnetVpos(
            IParameterService parameterService,
            IBus bus,
            ILogger<PosnetVpos> logger)
        {
            _parameterService = parameterService;
            _bus = bus;
            _logger = logger;
        }
        public async Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request)
        {
            PosnetInit3DModelRequest init3DModelObject = new PosnetInit3DModelRequest
            {
                MerchantId = _posNetPos.MerchantId,
                TerminalId = _posNetPos.TerminalId,
                PosnetId = _posNetPos.PosnetId,
                OrderId = request.OrderNumber,
                Amount = FormatAmount(request.Amount),
                CurrencyCode = ConvertCurrencyCode(request.CurrencyCode),
                NumberOfInstallments = request.Installment != 0 ? request.Installment : 1,
                TransactionType = request.AuthType == VposAuthType.PreAuth ? "Auth" : "Sale",
                CardHolderName = request.CardHolderName,
                CardNo = request.CardNumber,
                Cvv = request.Cvv2,
                LanguageCode = request.LanguageCode != "undefined" ? request.LanguageCode.ToLower() : "tr",
                ExpireDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear),
                SuccessUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
                FailureUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}"

            };
            string init3DModelRequest = init3DModelObject.BuildRequest();

            var content = await SendPosnetRequestAsync(_posNetPos.XmlServiceUrl, init3DModelRequest);

            if (string.IsNullOrEmpty(content))
            {
                return new PosInit3DModelResponse { IsSuccess = false, HtmlContent = string.Empty };
            }

            var parseResponse = new PosnetInit3DModelResponse()
               .Parse(content);


            var response = new PosInit3DModelResponse();

            if (parseResponse.Approved is SuccessCode)
            {
                string htmlValue = BuildPosnet3DHtmlModel(init3DModelObject, parseResponse);

                return new PosInit3DModelResponse
                {
                    IsSuccess = true,
                    HtmlContent = Base64Encode(htmlValue),
                    ResponseCode = string.Empty,
                    ResponseMessage = string.Empty
                };
            }
            else
            {
                return new PosInit3DModelResponse
                {
                    IsSuccess = false,
                    ResponseCode = parseResponse.ErrorCode,
                    ResponseMessage = parseResponse.ErrorMessage,
                    HtmlContent = String.Empty
                };
            }
        }
        public async Task<PosVerify3dModelResponse> Verify3DModel(Dictionary<string, string> form)
        {
            if (form == null)
            {
                var resp = new PosVerify3dModelResponse
                {
                    IsSuccess = false,
                    ResponseCode = String.Empty,
                    ResponseMessage = "Invalid Form",
                };
                return await Task.FromResult(resp);
            }

            form.TryGetValue("MerchantPacket", out string merchantPacket);
            form.TryGetValue("BankPacket", out string bankPacket);
            form.TryGetValue("Sign", out string sign);
            form.TryGetValue("CCPrefix", out string ccPrefix);
            form.TryGetValue("TranType", out string tranType);
            form.TryGetValue("Amount", out string amount);
            form.TryGetValue("Xid", out string xid);
            form.TryGetValue("MerchantId", out string merchantId);
            form.TryGetValue("CurrencyCode", out string currency);

            string verify3DModelRequest = new PosnetVerifyUserModelRequest
            {
                MerchantId = _posNetPos.MerchantId,
                TerminalId = _posNetPos.TerminalId,
                EncryptionKey = _posNetPos.EncryptionKey,
                BankPacket = bankPacket,
                MerchantPacket = merchantPacket,
                Sign = sign,
                Amount = amount,
                OrderId = xid,
                CurrencyCode = ConvertCurrencyCode(currency),
            }.BuildRequest();

            var content = await SendPosnetRequestAsync(_posNetPos.XmlServiceUrl, verify3DModelRequest);

            if (string.IsNullOrEmpty(content))
            {
                return new PosVerify3dModelResponse { IsSuccess = false };
            }

            var parseResponse = new PosnetVerifyUserModelResponse()
               .Parse(content);

            if (parseResponse.MdStatus.Trim() == "1")
                parseResponse.TxStatus = ThreeDFullSecureStatus;
            else if (parseResponse.MdStatus.Trim() == "2" || parseResponse.MdStatus.Trim() == "3" || parseResponse.MdStatus.Trim() == "4")
                parseResponse.TxStatus = ThreeDHalfSecureStatus;

            if (parseResponse.Approved is SuccessCode)
            {

                return new PosVerify3dModelResponse
                {
                    IsSuccess = true,
                    ResponseCode = string.Empty,
                    ResponseMessage = string.Empty,
                    OrderNumber = parseResponse.OrderNumber,
                    MdStatus = parseResponse.MdStatus,
                    MdErrorMessage = parseResponse.MdErrorMessage,
                    TxnStat = parseResponse.TxStatus,
                    Hash = parseResponse.Mac,
                    BankPacket = bankPacket
                };
            }
            else
            {
                return new PosVerify3dModelResponse
                {
                    IsSuccess = false,
                    ResponseCode = parseResponse.ErrorCode,
                    ResponseMessage = parseResponse.ErrorMessage,
                };
            }
        }
        public async Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request)
        {
            var payment3DModelRequest = new PosnetPayment3DModelRequest
            {
                MerchantId = _posNetPos.MerchantId,
                TerminalId = _posNetPos.TerminalId,
                EncryptionKey = _posNetPos.EncryptionKey,
                BankPacket = request.BankPacket,
                Amount = FormatAmount(request.Amount),
                OrderId = request.OrderNumber,
                CurrencyCode = ConvertCurrencyCode(request.CurrencyCode)

            }.BuildRequest();

            var content = await SendPosnetRequestAsync(_posNetPos.XmlServiceUrl, payment3DModelRequest);

            var parseResponse = new PosnetPayment3DModelResponse()
               .Parse(content);

            var response = new PosPaymentProvisionResponse();

            if (parseResponse.Approved is SuccessCode)
            {
                response.IsSuccess = true;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
                response.AuthCode = parseResponse.AuthCode;
                response.OrderNumber = parseResponse.HostLogKey;
            }
            else
            {
                response.IsSuccess = false;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
            }
            return response;
        }

        public async Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request)
        {
            var paymentDetailRequest = new PosnetPaymentDetailRequest
            {
                MerchantId = _posNetPos.MerchantId,
                TerminalId = _posNetPos.TerminalId,
                OrderId = request.OrderNumber,
                OrderDate = FormatOrderDate(request.OrderDate)

            }.BuildRequest();

            var content = await SendPosnetRequestAsync(_posNetPos.XmlServiceUrl, paymentDetailRequest);

            var parseResponse = new PosnetPaymentDetailResponse()
                .Parse(content);

            var response = new PosPaymentDetailResponse();

            if (parseResponse.Approved is SuccessCode)
            {
                response.IsSuccess = true;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
                response.TransId = parseResponse.OrderNumber;
                response.CardInformation = parseResponse.CardNo;
                response.AuthCode = parseResponse.AuthCode;
                response.OrderNumber = parseResponse.HostLogKey;
                response.TrxDate = parseResponse.TranDate;
                response.Amount = FormatResponseAmount(parseResponse.Amount);
                response.OrderStatus = GetOrderStatus(parseResponse);

            }
            else
            {
                response.IsSuccess = false;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
                response.TrxDate = parseResponse.TranDate;
            }


            return response;
        }

        public async Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request)
        {

            var authRequest = new PosnetPaymentNonSecureRequest
            {
                MerchantId = _posNetPos.MerchantId,
                TerminalId = _posNetPos.TerminalId,
                TranDateRequired = 1,
                Amount = FormatAmount(request.Amount),
                CardNo = request.CardNumber,
                CurrencyCode = ConvertCurrencyCode(request.CurrencyCode),
                Cvv = request.Cvv2,
                ExpireDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear),
                OrderId = request.OrderNumber,
                NumberOfInstallments = request.Installment ?? 1,
                SubMerchantId = request.SubMerchantId,
                MrcPfId = _posNetPos.MrcPfId,
                Mcc = request.SubMerchantMcc,
                Vkn = request.SubMerchantTaxNumber,
                SubDealerCode = request.SubMerchantCode,
                TransactionType = request.AuthType == VposAuthType.PreAuth ? "Auth" : "Sale"
            }.BuildRequest();

            var content = await SendPosnetRequestAsync(_posNetPos.XmlServiceUrl, authRequest);

            var parseResponse = new PosnetPaymentNonSecureResponse()
           .Parse(content);

            var response = new PosPaymentProvisionResponse();

            if (parseResponse.Approved is SuccessCode)
            {
                response.IsSuccess = true;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
                response.AuthCode = parseResponse.AuthCode;
                response.OrderNumber = parseResponse.HostLogKey;
                response.TrxDate = parseResponse.TranDate;
            }
            else
            {
                response.IsSuccess = false;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
            }
            return response;
        }

        public async Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request)
        {
            var pointInquiryRequest = new PosnetPointInquiryRequest
            {
                MerchantId = _posNetPos.MerchantId,
                TerminalId = _posNetPos.TerminalId,
                TranDateRequired = 1,
                CardNo = request.CardNumber,
                ExpireDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear),
                GetPointDetail = "N"
            }.BuildRequest();

            var content = await SendPosnetRequestAsync(_posNetPos.XmlServiceUrl, pointInquiryRequest);

            var parseResponse = new PosnetPointInquiryResponse()
               .Parse(content);

            var response = new PosPointInquiryResponse();

            if (parseResponse.Approved is SuccessCode)
            {
                response.IsSuccess = true;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
                response.AvailablePoint = FormatResponseAmount(parseResponse.PointAmount);
            }
            else
            {
                response.IsSuccess = false;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
            }

            return response;
        }

        public async Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request)
        {
            var postAuthRequest = new PosnetPostAuthRequest
            {
                MerchantId = _posNetPos.MerchantId,
                TerminalId = _posNetPos.TerminalId,
                Amount = FormatAmount(request.Amount),
                CurrencyCode = ConvertCurrencyCode(request.CurrencyCode),
                OrderId = request.OrgOrderNumber,
                NumberOfInstallments = request.Installment ?? 1,
                OrderDate = FormatOrderDate(request.OrderDate)
            }.BuildRequest();

            var content = await SendPosnetRequestAsync(_posNetPos.XmlServiceUrl, postAuthRequest);

            var parseResponse = new PosnetPostAuthResponse()
               .Parse(content);

            var response = new PosPaymentProvisionResponse();

            if (parseResponse.Approved is SuccessCode)
            {
                response.IsSuccess = true;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
                response.AuthCode = parseResponse.AuthCode;
                response.OrderNumber = parseResponse.HostLogKey;
            }
            else
            {
                response.IsSuccess = false;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
            }

            return response;
        }

        public async Task<PosRefundResponse> Refund(PosRefundRequest request)
        {
            var refundRequest = new PosnetRefundRequest
            {
                MerchantId = _posNetPos.MerchantId,
                TerminalId = _posNetPos.TerminalId,
                TranDateRequired = 1,
                Amount = FormatAmount(request.Amount),
                CurrencyCode = ConvertCurrencyCode(request.CurrencyCode),
                OrderId = request.OrgAuthProcessOrderNo,
                OrderDate = FormatOrderDate(request.OrderDate)

            }.BuildRequest();

            var content = await SendPosnetRequestAsync(_posNetPos.XmlServiceUrl, refundRequest);

            var parseResponse = new PosnetRefundResponse()
                .Parse(content);

            var response = new PosRefundResponse();

            if (parseResponse.Approved is SuccessCode)
            {
                response.IsSuccess = true;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
                response.AuthCode = parseResponse.AuthCode;
                response.OrderNumber = parseResponse.HostLogKey;
                response.TrxDate = parseResponse.TranDate;
            }
            else
            {
                response.IsSuccess = false;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
                response.TrxDate = parseResponse.TranDate;
            }

            return response;
        }

        public void SetServiceParameters(object serviceParameters, Guid? merchantId = null)
        {
            _posNetPos = (PosnetPosInfo)serviceParameters;
        }

        public async Task<PosVoidResponse> Void(PosVoidRequest request)
        {
            var voidRequest = new PosnetVoidRequest
            {
                MerchantId = _posNetPos.MerchantId,
                TerminalId = _posNetPos.TerminalId,
                ReverseType = GetReverseTransactionType(request.ReverseType),
                OrderId = request.OrgAuthProcessOrderNo,
                OrderDate = FormatOrderDate(request.OrderDate)

            }.BuildRequest();

            var content = await SendPosnetRequestAsync(_posNetPos.XmlServiceUrl, voidRequest);

            var parseResponse = new PosnetVoidResponse()
              .Parse(content);

            var response = new PosVoidResponse();

            if (parseResponse.Approved is SuccessCode)
            {
                response.IsSuccess = true;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
                response.AuthCode = parseResponse.AuthCode;
                response.OrderNumber = parseResponse.HostLogKey;
            }
            else
            {
                response.IsSuccess = false;
                response.ResponseCode = parseResponse.ErrorCode;
                response.ResponseMessage = parseResponse.ErrorMessage;
                response.TrxDate = parseResponse.TranDate;
            }

            return response;
        }

        private async Task<string> SendPosnetRequestAsync(string url, string data)
        {
            try
            {
                var correlationId = Guid.NewGuid();

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                await SendIntegrationRequest(data, correlationId, IntegrationLogDataType.Soap);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var httpParameters = new Dictionary<string, string>();
                httpParameters.Add("xmldata", data);

                var response = await client.PostAsync(url, new FormUrlEncodedContent(httpParameters));

                await SendIntegrationResponse(response, correlationId);

                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();

                return responseContent;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Send Posnet Request Error {ex.Message}");
                throw;
            }

        }

        private async Task SendIntegrationRequest(string data, Guid correlationId, IntegrationLogDataType integrationLogDataType)
        {
            try
            {
                var isLogEnable = await _parameterService.GetParameterAsync
                (VposConsts.ParameterGroupCode, VposConsts.PosnetVpos);
                if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
                {

                    var log = new IntegrationLog()
                    {
                        CorrelationId = correlationId.ToString(),
                        Name = VposConsts.PosnetVpos,
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
                _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.PosnetVpos} - Exception {exception}");
            }

        }

        private async Task SendIntegrationResponse(HttpResponseMessage httpResponse, Guid correlationId)
        {
            try
            {
                var isLogEnable = await _parameterService.GetParameterAsync
                (VposConsts.ParameterGroupCode, VposConsts.PosnetVpos);
                if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
                {

                    var log = new IntegrationLog()
                    {
                        CorrelationId = correlationId.ToString(),
                        Name = VposConsts.PosnetVpos,
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
                _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.PosnetVpos} - Exception {exception}");
            }

        }
        private static OrderStatus GetOrderStatus(PosnetPaymentDetailResponse paymentDetail)
        {
            OrderStatus status = OrderStatus.Unknown;
            switch (paymentDetail.Status)
            {
                case "Sale":
                    status = paymentDetail.TranDate.Date == DateTime.Now.Date
                                             ? OrderStatus.WaitingEndOfDay
                                             : OrderStatus.EndOfDayCompleted;
                    break;
                case "Authorization":
                    status = OrderStatus.PreAuth;
                    break;
                case "Capture":
                    status = OrderStatus.PostAuth;
                    break;
                case "Sale_Reverse":
                case "Return_Reverse":
                case "Authorization_Reverse":
                case "Capture_Reverse":
                    status = OrderStatus.Cancelled;
                    break;
                case "Return":
                    status = OrderStatus.Refunded;
                    break;

                default:
                    status = OrderStatus.Unknown;
                    break;
            }

            return status;
        }

        private static string GetReverseTransactionType(TransactionType txnType)
        {
            string transactionType = string.Empty;
            switch (txnType)
            {
                case TransactionType.Auth:
                    transactionType = "sale";
                    break;
                case TransactionType.PreAuth:
                    transactionType = "auth";
                    break;
                case TransactionType.PostAuth:
                    transactionType = "capt";
                    break;
                case TransactionType.Return:
                    transactionType = "return";
                    break;
                default:
                    transactionType = string.Empty;
                    break;
            }

            return transactionType;
        }

        private static string ConvertCurrencyCode(string currency)
        {
            string resultCurrency = string.Empty;

            switch (currency)
            {
                case "TRY":
                    resultCurrency = "TL";
                    break;
                case "EUR":
                    resultCurrency = "EU";
                    break;
                case "USD":
                    resultCurrency = "US";
                    break;
                default:
                    resultCurrency = "TL";
                    break;
            }


            return resultCurrency;
        }

        protected override string FormatAmount(decimal amount)
        {
            return (amount * 100m).ToString("0.##", new CultureInfo("en-US"));
        }

        private decimal FormatResponseAmount(string amount)
        {
            return !string.IsNullOrEmpty(amount) ? decimal.Parse(amount, CultureInfo.InvariantCulture) : 0;
        }
        protected override string FormatExpiryDate(string month, string year)
        {
            if (year.Length > 2)
            {
                return $"{year.Substring(2, 2)}{month}";
            }

            return $"{year}{month}";
        }
        private string FormatOrderDate(DateTime orderDate)
        {
            var year = orderDate.Year.ToString();
            var month = orderDate.Month.ToString("D2");
            var day = orderDate.Day.ToString("D2");

            return $"{year}{month}{day}";
        }

        private string BuildPosnet3DHtmlModel(PosnetInit3DModelRequest initRequest, PosnetInit3DModelResponse parsedResponse)
        {

            var request = new StringBuilder();

            request.AppendLine("<!DOCTYPE html>");
            request.AppendLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
            request.AppendLine("<head>");
            request.AppendLine("<meta charset=\"utf-8\" />");
            request.AppendLine("<title></title>");
            request.AppendLine("<script type=\"text/javascript\" src=\"https://posnet.yapikredi.com.tr/3DSWebService/scriptler/posnet.js\"></script>");
            request.AppendLine("<script type=\"text/javascript\"> function submitFormEx(Form, OpenNewWindowFlag, WindowName) { submitForm(Form, OpenNewWindowFlag, WindowName) Form.submit(); }</script>");
            request.AppendLine("<script type=\"text/javascript\"> function triggerButton() { document.getElementById('auto-submit-btn').click(); } window.addEventListener('load', triggerButton); </script>");
            request.AppendLine("</head>");

            request.AppendLine("<body>");
            request.AppendLine("<form name=\"formName\" method=\"post\" action=\"https://setmpos.ykb.com/3DSWebService/YKBPaymentService\">");
            request.AppendLine($"<input  name=\"mid\" type=\"hidden\" id=\"mid\" value=\"{initRequest.MerchantId}\"/>");
            request.AppendLine($"<input  name=\"posnetID\" type=\"hidden\" id=\"PosnetID\" value=\"{initRequest.PosnetId}\"/>");
            request.AppendLine($"<input  name=\"posnetData\" type=\"hidden\" id=\"posnetData\" value=\"{parsedResponse.PosnetData}\"/>");
            request.AppendLine($"<input  name=\"posnetData2\" type=\"hidden\" id=\"posnetData2\" value=\"{parsedResponse.PosnetData2}\"/>");
            request.AppendLine($"<input  name=\"digest\" type=\"hidden\" id=\"sign\" value=\"{parsedResponse.Digest}\"/>");
            request.AppendLine($"<input name=\"vftCode\" type=\"hidden\" id=\"vftCode\" value=\"\" />");
            request.AppendLine($"<input  name=\"merchantReturnURL\" type=\"hidden\" id=\"merchantReturnURL\" value=\"{initRequest.SuccessUrl}\"/>");
            request.AppendLine($"<input  name=\"lang\" type=\"hidden\" id=\"lang\" value=\"{initRequest.LanguageCode}\"/>");
            request.AppendLine($"<input  name=\"url\" type=\"hidden\" id=\"url\" value=\"\"/>");
            request.AppendLine("<input name=\"openANewWindow\" type=\"hidden\" id=\"openANewWindow\" value=\"0\" />");
            request.AppendLine($"<input id=\"auto-submit-btn\" style=\"display: none;\" type=\"submit\" name=\"Submit\" value=\"Doğrulama Yap\" onclick=\"submitFormEx(formName, 0, 'YKBWindow')\" />");
            request.AppendLine("</form>");
            request.AppendLine("</body>");
            request.AppendLine("</html>");

            return request.ToString();

        }

        private static string Base64Encode(string htmlValue)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(htmlValue);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
