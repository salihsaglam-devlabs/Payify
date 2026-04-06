using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.ExternalServiceConfiguration;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Interfaces;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Requests;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.Exceptions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing
{
    public class SekerBankApi : ISekerBankApi
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SekerBankApi> _logger;
        private readonly IVaultClient _vaultClient;
        private readonly IIntegrationLogger _integrationLogger;
        private readonly SekerBankServiceSettings _serviceSettings;

        private readonly int _defaultTimeout = 10000;

        public SekerBankApi(ILogger<SekerBankApi> logger, IConfiguration configuration, IVaultClient vaultClient, IIntegrationLogger integrationLogger)
        {
            _logger = logger;
            _configuration = configuration;
            _vaultClient = vaultClient;
            _integrationLogger = integrationLogger;

            var serviceSettings = new ServiceSettings();

            _configuration.GetSection(nameof(ServiceSettings)).Bind(serviceSettings);
            _serviceSettings = serviceSettings.SekerBankServiceSettings;
        }

        public async Task<SekerBankResponse<SekerBankAuthorizationResponse>> LoginAsync(SekerBankAuthorizationRequest request)
        {
            return await ExecuteAsync<SekerBankAuthorizationResponse, SekerBankAuthorizationRequest>("oauth/token", Method.Post, string.Empty, request, SekerBankMethodType.Technical);
        }

        public async Task<SekerBankResponse<List<SekerBankInstitutionListResponse>>> GetInstitutionListAsync(string authorizationToken)
        {
            return await ExecuteAsync<List<SekerBankInstitutionListResponse>, object>("api/institutions", Method.Get, authorizationToken, Array.Empty<object>(), SekerBankMethodType.Technical);
        }

        public async Task<SekerBankResponse<SekerBankBillInquiryResponse>> InquireBillsAsync(string authorizationToken, SekerBankBillInquiryRequest request)
        {
            return await ExecuteAsync<SekerBankBillInquiryResponse>("api/bills/check", Method.Get, authorizationToken, request, SekerBankMethodType.Inquiry, true);
        }

        public async Task<SekerBankResponse<SekerBankBillPaymentResponse>> PayInquiredBillsAsync(string authorizationToken, SekerBankBillPaymentRequest request)
        {
            return await ExecuteAsync<SekerBankBillPaymentResponse>("api/bills/pay", Method.Post, authorizationToken, request, SekerBankMethodType.Payment, true);
        }

        public async Task<SekerBankResponse<SekerBankBillStatusResponse>> InquireBillStatusAsync(string authorizationToken, SekerBankBillStatusRequest request)
        {
            return await ExecuteAsync<SekerBankBillStatusResponse, SekerBankBillStatusRequest>("api/bills/status", Method.Get, authorizationToken, request, SekerBankMethodType.Inquiry);
        }

        public async Task<SekerBankResponse<SekerBankBillPaymentCancelResponse>> CancelBillPaymentAsync(string authorizationToken, SekerBankBillPaymentCancelRequest request)
        {
            return await ExecuteAsync<SekerBankBillPaymentCancelResponse, SekerBankBillPaymentCancelRequest>("/api/bills/cancel", Method.Put, authorizationToken, request, SekerBankMethodType.Cancellation);
        }

        public async Task<SekerBankResponse<SekerBankReconciliationSummaryResponse>> GetReconciliationSummaryAsync(string authorizationToken, SekerBankReconciliationSummaryRequest request)
        {
            return await ExecuteAsync<SekerBankReconciliationSummaryResponse>("api/reconciliation/summary", Method.Post, authorizationToken, request, SekerBankMethodType.Reconciliation, true);
        }

        public async Task<SekerBankResponse<SekerBankReconciliationDetailsResponse>> GetReconciliationDetailsAsync(string authorizationToken, SekerBankReconciliationDetailsRequest request)
        {
            return await ExecuteAsync<SekerBankReconciliationDetailsResponse, SekerBankReconciliationDetailsRequest>("api/reconciliation/summary-detail", Method.Get, authorizationToken, request, SekerBankMethodType.Reconciliation);
        }

        public async Task<SekerBankResponse<SekerBankInstitutionReconciliationDetailsResponse>> GetInstitutionReconciliationDetailsAsync(string authorizationToken, SekerBankReconciliationDetailsRequest request)
        {
            return await ExecuteAsync<SekerBankInstitutionReconciliationDetailsResponse, SekerBankReconciliationDetailsRequest>("api/reconciliation/summary-detail", Method.Get, authorizationToken, request, SekerBankMethodType.Reconciliation);
        }

        public async Task<SekerBankResponse<SekerBankInstitutionPaymentDetailsResponse>> GetInstitutionPaymentDetailsAsync(string authorizationToken, SekerBankReconciliationDetailsRequest request)
        {
            return await ExecuteAsync<SekerBankInstitutionPaymentDetailsResponse, SekerBankReconciliationDetailsRequest>("api/reconciliation/institution-detail", Method.Get, authorizationToken, request, SekerBankMethodType.Reconciliation);
        }

        public async Task<SekerBankResponse<SekerBankInquireInstitutionReconciliationResponse>> InquireInstitutionReconciliationStatusAsync(string authorizationToken, SekerBankInquireInstitutionReconciliationRequest request)
        {
            return await ExecuteAsync<SekerBankInquireInstitutionReconciliationResponse, SekerBankInquireInstitutionReconciliationRequest>("api/reconciliation/summary-detail-close", Method.Post, authorizationToken, request, SekerBankMethodType.Reconciliation);
        }

        private async Task<string> GetAuthorizationHeaderAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {

                var serviceSettings = await _vaultClient.GetSecretValueAsync<SekerBankServiceSettings>("BillingSecrets", "SekerBank");
                var clientKey = $"{serviceSettings.ClientId}:{serviceSettings.ClientSecret}";
                var encodedClientKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientKey));

                return $"Basic {encodedClientKey}";
            }
            else
            {
                return $"Bearer {token}";
            }
        }

        private string GetContentType(string token)
        {
            return string.IsNullOrEmpty(token) ?
                _serviceSettings.FormContentType : _serviceSettings.ContentType;
        }

        private async Task<SekerBankResponse<T>> ExecuteAsync<T, R>(string endPoint, Method requestType, string token, object parameters, SekerBankMethodType methodType)
        {
            return await ExecuteAsync<T>(endPoint, requestType, token, parameters, methodType, false);
        }

        private async Task<SekerBankResponse<T>> ExecuteAsync<T>(string endPoint, Method requestType, string token, object parameters, SekerBankMethodType methodType, bool throwOnTimeout)
        {
            var authorization = await GetAuthorizationHeaderAsync(token);
            var contentType = GetContentType(token);

            var timeout = GetTimeout(methodType);

            var restClient = new RestClient(new RestClientOptions
            {
                MaxTimeout = timeout,
                BaseUrl = new Uri(_serviceSettings.ServiceUrl)
            });
            var restRequest = new RestRequest(endPoint, requestType);

            restRequest.AddHeader("Authorization", authorization);
            restRequest.AddHeader("Content-Type", contentType);

            TokenIsNullOrEmptyControl(requestType, token, parameters, contentType, restRequest);

            var integrationLogData = new IntegrationLog
            {
                CorrelationId = Guid.NewGuid().ToString(),
                Name = "SekerBank",
                Type = "Billing",
                Request = JsonSerializer.Serialize(restRequest),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            };

            await _integrationLogger.QueueLogAsync(integrationLogData);

            var restResponse = await restClient.ExecuteAsync(restRequest);

            try
            {
                integrationLogData.Request = string.Empty;
                integrationLogData.HttpCode = restResponse?.StatusCode.ToString();
                integrationLogData.ErrorCode = restResponse?.StatusCode.ToString();
                integrationLogData.ErrorMessage = restResponse?.ErrorMessage;
                integrationLogData.Response = JsonSerializer.Serialize(restResponse);
            }
            catch
            {
                integrationLogData.Response = $"Content: {restResponse?.Content}, " +
                    $"Exception: {restResponse?.ErrorException?.Message}, " +
                    $"InnerException: {restResponse?.ErrorException?.InnerException?.Message}";
            }

            await _integrationLogger.QueueLogAsync(integrationLogData);

            switch (restResponse.StatusCode)
            {
                case HttpStatusCode.OK:
                    {
                        var result = restResponse.Content;
                        return new SekerBankResponse<T>
                        {
                            IsSuccess = true,
                            ErrorDescription = string.Empty,
                            Data = JsonSerializer.Deserialize<T>(result)
                        };
                    }
                default:
                    {
                        if (restResponse.ResponseStatus == ResponseStatus.TimedOut && throwOnTimeout)
                        {
                            throw new TimeoutException($"SekerBankApiRequestTimedOut, Endpoint: {endPoint}");
                        }

                        var apiResponse = new SekerBankResponse<T>
                        {
                            IsSuccess = false
                        };

                        if (restResponse.Content != null)
                        {
                            apiResponse.ErrorResponse = JsonSerializer.Deserialize<SekerBankErrorResponse>(restResponse.Content);
                            apiResponse.ErrorDescription = $"ErrorRequestingSekerBankApi, Endpoint: {endPoint}, StatusCode: {restResponse.StatusCode}";
                        }
                        else
                        {
                            apiResponse.ErrorDescription = $"ErrorRequestingSekerBankApi, " +
                                $"Content: {restResponse.Content}, " +
                                $"Exception: {restResponse.ErrorException?.Message}, " +
                                $"InnerException: {restResponse.ErrorException?.InnerException?.Message}";
                        }

                        if (apiResponse.ErrorResponse == null)
                        {
                            var errorMessage = restResponse.Content == null ? apiResponse.ErrorDescription : restResponse.Content;

                            _logger.LogError("SekerBankApiErrorWithNoContent, StatusCode: {StatusCode}, ErrorDescription: {ErrorDescription}", restResponse.StatusCode, apiResponse.ErrorDescription);

                            throw new ApiException(ApiErrorCode.NoErrorReponse, errorMessage);
                        }
                        else
                        {
                            var errorDetail = $"ErrorCode: {apiResponse.ErrorResponse.errorCode}, ErrorMessage: {apiResponse.ErrorResponse.errorMessage}";
                            var errorCode = (SekerBankExceptions)Enum.Parse(typeof(SekerBankExceptions), apiResponse.ErrorResponse.errorCode);

                            _logger.LogError(errorDetail);

                            throw errorCode switch
                            {
                                SekerBankExceptions.NOT_FOUND or
                                SekerBankExceptions.USER_NOT_AUTHORIZED or
                                SekerBankExceptions.INVALID_REQUEST or
                                SekerBankExceptions.USER_NOT_FOUND or
                                SekerBankExceptions.PAYMENT_CENTER_NOT_FOUND or
                                SekerBankExceptions.PASSIVE_ACCOUNT or
                                SekerBankExceptions.LOCKED_ACCOUNT or
                                SekerBankExceptions.IP_NOT_ALLOWED =>
                                    new VendorTechnicalException(errorDetail),
                                SekerBankExceptions.INVALID_INPUT or
                                SekerBankExceptions.INVALID_SUBS_NO_FORMAT =>
                                    new InvalidInputException(),
                                SekerBankExceptions.BANKING_SIDE_EXCEPTION =>
                                    ParseBankingSideException(apiResponse.ErrorResponse.errorMessage),
                                SekerBankExceptions.PAYMENT_NOT_FOUND =>
                                    new InvalidPaymentException(),
                                SekerBankExceptions.UNEXPECTED_EXCEPTION =>
                                    new UnexpectedErrorException(),
                                SekerBankExceptions.INVALID_DEBT =>
                                    new InvalidDebtException(),
                                _ =>
                                    new ArgumentException($"UnsupportedExceptionType: {errorDetail}"),
                            };
                        }
                    }
            }
        }

        private ApiException ParseBankingSideException(string errorMessage)
        {
            if (
                    errorMessage.Contains("abone", StringComparison.OrdinalIgnoreCase)
                    ||
                    errorMessage.Contains("borç", StringComparison.OrdinalIgnoreCase)
                    ||
                    errorMessage.Contains("borc", StringComparison.OrdinalIgnoreCase)
               )
            {
                return new InvalidSubscriptionOrDebtException();
            }
            else if (errorMessage.Contains("mutabakat", StringComparison.OrdinalIgnoreCase))
            {
                return new ReconciliationException(errorMessage);
            }
            else if (errorMessage.Contains("ödeme kaydı bulunamadı", StringComparison.OrdinalIgnoreCase))
            {
                return new NotPaidException();
            }
            else
            {
                return new ExternalApiException(errorMessage);
            }
        }

        private static void TokenIsNullOrEmptyControl(Method requestType, string token, object parameters, string contentType, RestRequest restRequest)
        {
            if (string.IsNullOrEmpty(token) || requestType == Method.Get)
            {
                var parametersDictionary = parameters.GetType()
                    .GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(parameters));

                foreach (var parameter in parametersDictionary)
                {
                    var value = parameter.Value;

                    if (value is null || value.ToString() == "0" || string.IsNullOrEmpty(value.ToString()))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(token))
                    {
                        restRequest.AddParameter(parameter.Key, value.ToString(), false);
                    }
                    else
                    {
                        restRequest.AddQueryParameter(parameter.Key, value.ToString());
                    }
                }
            }
            else
            {
                restRequest.AddParameter(contentType, JsonSerializer.Serialize(parameters), ParameterType.RequestBody);
            }
        }

        private int GetTimeout(SekerBankMethodType methodType)
        {

            var timeout = methodType switch
            {
                SekerBankMethodType.Technical => _serviceSettings.TechnicalTimeout,
                SekerBankMethodType.Reconciliation => _serviceSettings.ReconciliationTimeout,
                SekerBankMethodType.Payment => _serviceSettings.PaymentTimeout,
                SekerBankMethodType.Cancellation => _serviceSettings.CancellationTimeout,
                SekerBankMethodType.Inquiry => _serviceSettings.InquiryTimeout,
                _ => _defaultTimeout,
            };

            return timeout == 0 ? _defaultTimeout : timeout;
        }
    }
}
