using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Request;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Response;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using LinkPara.ContextProvider;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos
{
    public class OzanPayVpos : VposBase, IVposApi
    {
        private OzanPayPosInfo _ozanPayPos;
        private readonly ILogger<OzanPayVpos> _logger;
        private readonly IParameterService _parameterService;
        private readonly IBus _bus;
        private readonly IGenericRepository<ThreeDVerification> _threeDVerificationRepository;
        private readonly IGenericRepository<MerchantVpos> _merchantVposRepository;
        private readonly IConfiguration _configuration;
        private readonly IContextProvider _contextProvider;

        private readonly string StatusErrorCode = "ERROR";
        private readonly string StatusApprove = "APPROVED";
        private readonly string StatusApproveCode = "Y";
        private readonly string StatusWaiting = "WAITING";
        private readonly string PurchasePath = "purchase";
        private readonly string PaymentDetailPath = "transaction/detail";
        private readonly string CancelPath = "cancel";
        private readonly string RefundPath = "refund";
        private readonly string PostAuthPath = "capture";
        private string _providerKey = "";
        private string _apiKey = "";

        private OzanPayDataModel DummyData = new OzanPayDataModel();
        public OzanPayVpos(
            ILogger<OzanPayVpos> logger,
            IParameterService parameterService,
            IBus bus,
            IConfiguration configuration,
            IGenericRepository<ThreeDVerification> threeDVerificationRepository,
            IGenericRepository<MerchantVpos> merchantVposRepository,
            IContextProvider contextProvider)
        {
            _threeDVerificationRepository = threeDVerificationRepository;
            _merchantVposRepository = merchantVposRepository;
            _logger = logger;
            _parameterService = parameterService;
            _bus = bus;
            _configuration = configuration;
            _contextProvider = contextProvider;
            _configuration.GetSection("OzanPayDummyData").Bind(DummyData);     
        }

        public async Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request)
        {
            var init3DRequest = new OzanPaymentRequest
            {
                ApiKey = _apiKey,
                ProviderKey = _providerKey,
                Amount = int.Parse(FormatAmount(request.Amount)),
                Currency = request.CurrencyCode,
                Number = request.CardNumber,
                ExpiryMonth = request.ExpireMonth,
                ExpiryYear = request.ExpireYear,
                Cvv = request.Cvv2,
                ReferenceNo = request.OrderNumber,
                Is3D = true,
                Only3D = true,
                IsPreAuth = request.AuthType == VposAuthType.PreAuth,
                Installment = request.Installment > 0 ? request.Installment : 1,
                ReturnUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
                Description = DummyData.Description,
                BillingFirstName = DummyData.BillingFirstName,
                BillingLastName = DummyData.BillingLastName,
                Email = DummyData.Email,
                BillingCompany = DummyData.BillingCompany,
                BillingPhone = DummyData.BillingPhone,
                BillingAddress1 = DummyData.BillingAddress1,
                BillingCountry = DummyData.BillingCountry,
                BillingCity = DummyData.BillingCity,
                BillingPostCode = DummyData.BillingPostCode,
                BasketItems = new List<object>
                {
                    new
                    {
                        name = DummyData.Product,
                        description = DummyData.Product,
                        category = DummyData.Product,
                        extraField = "",
                        quantity = 1,
                        unitPrice = int.Parse(FormatAmount(request.Amount))
                    }
                },
                CustomerIp = request.ClientIp ?? _contextProvider.CurrentContext.ClientIpAddress,
                CustomerUserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36",
                BrowserInfo = new
                {
                    language = "en-US",
                    colorDepth = 24,
                    screenHeight = 900,
                    screenWidth = 1440,
                    screenTZ = "-180",
                    javaEnabled = false,
                    acceptHeader = "/"
                }
            }.BuildRequest();

            var response = await SendOzanPayRequestAsync(PurchasePath, init3DRequest);

            var parseResponse = VposHelper.ParseHelper<OzanPaymentResponse>(response);

            parseResponse.Form3D = CreateHtmlForm(parseResponse.Form3D);
            
            return new PosInit3DModelResponse
            {
                IsSuccess = parseResponse.Status == StatusWaiting,
                ResponseCode = parseResponse.Code.ToString(),
                ResponseMessage = parseResponse.Message,
                HtmlContent = parseResponse.Form3D is null ? null : Base64Encode(parseResponse.Form3D)
            };
        }

        public async Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request)
        {
            var threeDVerification = await _threeDVerificationRepository.GetAll()
                                   .Where(t => t.OrderId == request.OrgOrderNumber).FirstOrDefaultAsync();

            return new PosPaymentProvisionResponse
            {
                IsSuccess = threeDVerification.TxnStat == StatusApproveCode,
                ResponseCode = threeDVerification.BankResponseCode,
                ResponseMessage = threeDVerification.BankResponseDescription,
                OrderNumber = threeDVerification.PayerTxnId,
                TransId = request.OrderNumber,
                TrxDate = DateTime.Now
            };
        }

        public async Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request)
        {
            var paymentDetailRequest = new OzanPaymentDetailRequest
            {
                ApiKey = _apiKey,
                ReferenceNo = request.OrderNumber
            }.BuildRequest();

            var content = await SendOzanPayRequestAsync(PaymentDetailPath, paymentDetailRequest);

            var parseResponse = VposHelper.ParseHelper<OzanPaymentDetailResponse>(content);

            var response = new PosPaymentDetailResponse
            {
                IsSuccess = parseResponse.Status == StatusApprove,
                OrderStatus = GetOrderStatus(parseResponse),
                Amount = parseResponse.Amount,
                TransId = parseResponse.TransactionId,
                OrderNumber = parseResponse.ReferenceNo,
                ResponseCode = parseResponse.Code.ToString(),
                ResponseMessage = parseResponse.Message
            };

            return response;
        }

        public async Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request)
        {
            int installmentCount = 1;

            if (request.Installment is not null)
            {
                installmentCount = (int)(request.Installment > 0 ? request.Installment : 1);
            }

            var nonSecureRequest = new OzanPaymentRequest
            {
                ApiKey = _apiKey,
                ProviderKey = _providerKey,
                Amount = int.Parse(FormatAmount(request.Amount)),
                Currency = request.CurrencyCode,
                Number = request.CardNumber,
                ExpiryMonth = request.ExpireMonth,
                ExpiryYear = request.ExpireYear,
                Cvv = request.Cvv2,
                ReferenceNo = request.OrderNumber,
                Is3D = false,
                Only3D = false,
                IsPreAuth = request.AuthType == VposAuthType.PreAuth,
                Installment = installmentCount,
                Description = DummyData.Description,
                BillingFirstName = DummyData.BillingFirstName,
                BillingLastName = DummyData.BillingLastName,
                Email = DummyData.Email,
                BillingCompany = DummyData.BillingCompany,
                BillingPhone = DummyData.BillingPhone,
                BillingAddress1 = DummyData.BillingAddress1,
                BillingCountry = DummyData.BillingCountry,
                BillingCity = DummyData.BillingCity,
                BillingPostCode = DummyData.BillingPostCode,
                BasketItems = new List<object>
                {
                    new
                    {
                        name = DummyData.Product,
                        description = DummyData.Product,
                        category = DummyData.Product,
                        extraField = "",
                        quantity = 1,
                        unitPrice = int.Parse(FormatAmount(request.Amount))
                    }
                },
                CustomerIp = request.ClientIp ?? _contextProvider.CurrentContext.ClientIpAddress,
                CustomerUserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36",
                BrowserInfo = new
                {
                    language = "en-US",
                    colorDepth = 24,
                    screenHeight = 900,
                    screenWidth = 1440,
                    screenTZ = "-180",
                    javaEnabled = false,
                    acceptHeader = "/"
                }
            }.BuildRequest();

            var response = await SendOzanPayRequestAsync(PurchasePath, nonSecureRequest);

            var parseResponse = VposHelper.ParseHelper<OzanPaymentResponse>(response);

            var posResponse = new PosPaymentProvisionResponse
            {
                IsSuccess = parseResponse.Status == StatusApprove,
                ResponseCode = parseResponse.Code.ToString(),
                ResponseMessage = parseResponse.Message,
                OrderNumber = parseResponse.TransactionId,
            };

            return posResponse;
        }

        public Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request)
        {
            var postAuthRequest = new OzanPayPostAuthRequest
            {
                ApiKey = _apiKey,
                Amount = int.Parse(FormatAmount(request.Amount)),
                Currency = request.CurrencyCode,
                ReferenceNo = request.OrgOrderNumber,
                TransactionId = request.BankOrderId
            }.BuildRequest();

            var response = await SendOzanPayRequestAsync(PostAuthPath, postAuthRequest);

            var parseResponse = VposHelper.ParseHelper<OzanPayPostAuthResponse>(response);

            var posResponse = new PosPaymentProvisionResponse
            {
                IsSuccess = parseResponse.Status == StatusApprove,
                ResponseCode = parseResponse.Code.ToString(),
                ResponseMessage = parseResponse.Message,
                OrderNumber = parseResponse.TransactionId,
            };

            return posResponse;
        }

        public async Task<PosRefundResponse> Refund(PosRefundRequest request)
        {
            var refundRequest = new OzanPayRefundRequest
            {
                ApiKey = _apiKey,
                ReferenceNo = request.OrgAuthProcessOrderNo,
                TransactionId = request.BankOrderId,
                Amount = FormatAmount(request.Amount),
                Currency = request.CurrencyCode
            }.BuildRequest();

            var content = await SendOzanPayRequestAsync(RefundPath, refundRequest);

            var parseResponse = VposHelper.ParseHelper<OzanPayRefundResponse>(content);

            var response = new PosRefundResponse
            {
                IsSuccess = parseResponse.Status == StatusApprove,
                OrderNumber = parseResponse.TransactionId,
                ResponseCode = parseResponse.Code.ToString(),
                ResponseMessage = parseResponse.Message,
                TransId = parseResponse.TransactionId
            };

            return response;
        }

        public void SetServiceParameters(object serviceParameters, Guid? merchantId = null)
        {
            _ozanPayPos = (OzanPayPosInfo)serviceParameters;
            var merchantVpos = _merchantVposRepository.GetAll()
                .FirstOrDefault(m => m.MerchantId == merchantId && m.VposId == _ozanPayPos.VposId && m.RecordStatus == RecordStatus.Active);
            
            _providerKey = merchantVpos != null ? merchantVpos.ProviderKey : "";
            _apiKey = merchantVpos != null ? merchantVpos.ApiKey : "";
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

            form.TryGetValue("amount", out string amount);
            form.TryGetValue("transactionId", out string transactionId);
            form.TryGetValue("referenceNo", out string referenceNo);
            form.TryGetValue("currency", out string currency);
            form.TryGetValue("status", out string status);
            form.TryGetValue("message", out string message);
            form.TryGetValue("code", out string code);
            form.TryGetValue("mdStatus", out string mdStatus);
            form.TryGetValue("checksum", out string checksum);

            var calcChecksum = referenceNo + amount + currency + status + message + code + _providerKey;

            var hashChecksum = VposHelper.GetHexSha256(calcChecksum);

            if (hashChecksum != checksum)
            {
                response.IsSuccess = false;
            }
            else
            {
                response.IsSuccess = status == StatusApprove;
            }
   
            response.TxnStat = response.IsSuccess ? StatusApproveCode : StatusErrorCode;
            response.ResponseMessage = message;
            response.MdStatus = mdStatus;
            response.ResponseCode = code;
            response.ThreeDStatus = status;
            response.PayerTxnId = transactionId;
            response.OrderNumber = referenceNo;
            response.Hash = hashChecksum;

            return response;
        }

        public async Task<PosVoidResponse> Void(PosVoidRequest request)
        {

            var refundRequest = new OzanPayVoidRequest
            {
                ApiKey = _apiKey,
                ReferenceNo = request.OrgAuthProcessOrderNo,
                TransactionId = request.BankOrderId,
                Amount = FormatAmount(request.Amount),
                Currency = request.CurrencyCode
            };

            string requestString;
            var content = "";

            if (request.TransactionType == TransactionType.PreAuth)
            {
                requestString = refundRequest.BuildPreauthRequest();
                content = await SendOzanPayRequestAsync(CancelPath, requestString);
            }
            else
            {
                requestString = refundRequest.BuildRequest();
                content = await SendOzanPayRequestAsync(RefundPath, requestString);
            }

            var parseResponse = VposHelper.ParseHelper<OzanPayVoidResponse>(content);

            var response = new PosVoidResponse
            {
                IsSuccess = parseResponse.Status == StatusApprove,
                OrderNumber = parseResponse.TransactionId,
                ResponseCode = parseResponse.Code.ToString(),
                ResponseMessage = parseResponse.Message,
                TransId = request.OrderNumber
            };

            return response;
        }

        private async Task<string> SendOzanPayRequestAsync(string path, string data)
        {
            await SendIntegrationRequest(data, Guid.NewGuid());

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_ozanPayPos.BaseUrl + path),
                Headers =
                {
                    { "Accept", "application/json" },
                },
                Content = new StringContent(data, Encoding.UTF8, "application/json")
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            using (var response = await client.SendAsync(request))
            {
                await SendIntegrationResponse(response, Guid.NewGuid());

                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                return body;
            }
        }

        private async Task SendIntegrationRequest(string data, Guid correlationId)
        {
            try
            {
                var isLogEnable = await _parameterService.GetParameterAsync
                (VposConsts.ParameterGroupCode, VposConsts.OzanPayVpos);
                if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
                {

                    var log = new IntegrationLog()
                    {
                        CorrelationId = correlationId.ToString(),
                        Name = VposConsts.OzanPayVpos,
                        Type = nameof(IntegrationLogType.Vpos),
                        Date = DateTime.Now,
                        Request = data,
                        DataType = IntegrationLogDataType.Json
                    };

                    using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                    await endpoint.Send(log, cancellationToken.Token);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.OzanPayVpos} - Exception {exception}");
            }

        }

        private async Task SendIntegrationResponse(HttpResponseMessage data, Guid correlationId)
        {
            try
            {
                var isLogEnable = await _parameterService.GetParameterAsync
                (VposConsts.ParameterGroupCode, VposConsts.OzanPayVpos);
                if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
                {

                    var bytes = await data.Content.ReadAsByteArrayAsync();
                    var log = new IntegrationLog()
                    {
                        CorrelationId = correlationId.ToString(),
                        Name = VposConsts.OzanPayVpos,
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
                _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.OzanPayVpos} - Exception {exception}");
            }

        }

        protected override string FormatAmount(decimal amount)
        {
            int amountInPenny = (int)(amount * 100);
            return amountInPenny.ToString();
        }

        protected override string FormatExpiryDate(string month, string year)
        {
            throw new NotImplementedException();
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private OrderStatus GetOrderStatus(OzanPaymentDetailResponse response)
        {
            if (response.Status == "DECLINED")
            {
                return OrderStatus.Rejected;
            }

            switch (response.Operation)
            {
                case "REFUNDED":
                    return OrderStatus.Refunded;
                case "VOID":
                case "CANCEL":
                    return OrderStatus.Cancelled;
                case "3DAUTH":
                case "DIRECT":
                    return response.Type == "PREAUTH" ? OrderStatus.PreAuth
                        : DateTime.Parse(response.Created_At).Date == DateTime.UtcNow.Date
                        ? OrderStatus.WaitingEndOfDay : OrderStatus.EndOfDayCompleted;
                default:
                    return OrderStatus.Unknown;
            }
        }
        
        private string CreateHtmlForm(string formUrl)
        {

            return $"<!DOCTYPE html>\n" +
                   "<html lang=\"tr\">\n" +
                   "<head>\n<meta charset=\"UTF-8\">\n" +
                   "<title>Yönlendiriliyor...</title>\n" +
                   "<script>\n" +
                   "   window.onload = function () {\n" +
                   "     window.location.href = \"" + formUrl + "\";\n   };\n" +
                   "</script>\n" +
                   "</head>\n" +
                   "<body>\n" +
                   "<h1>Yönlendiriliyorsunuz...</h1>\n" +
                   "<p>Lütfen bekleyin, <a href=\""+formUrl+"\"> yönlendiriliyorsunuz...</a></p>\n" +
                   "</body>\n" +
                   "</html>";
        }
    }
}
