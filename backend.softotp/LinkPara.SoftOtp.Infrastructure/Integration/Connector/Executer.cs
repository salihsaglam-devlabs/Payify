using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LinkPara.SoftOtp.Infrastructure.Integration.Connector
{
    public class Executer
    {
        private static HttpClient _httpClient;
        private static HttpClient _backOfficeHttpClient;

        public static T Post<T, Y>(BaseRequest request, string methodName, PowerfactorConfig config)
            where T : PowerFactorResponseBase where Y : BaseRequest
        {
            try
            {
                var pwfRequest = Helper.EncryptRequest(Helper.ObjectToJson(request), config);
                pwfRequest.IsMutualAuthenticationRequired = config.IsMutualAuthenticationRequired.Equals("true");

                _httpClient = GetHttpClient(config);
                HttpContent contentPost = new StringContent(Helper.ObjectToJson(pwfRequest), Encoding.UTF8, "application/json");

                var httpResponse = _httpClient.PostAsync(string.Format("{0}{1}", config.ServicePath, methodName), contentPost).Result;
                httpResponse.EnsureSuccessStatusCode();
                var asyncResponse = httpResponse.Content.ReadAsStringAsync().Result;

                var responseObject = (T)Helper.JsonToObject(typeof(T), asyncResponse);

                if (pwfRequest.IsMutualAuthenticationRequired && responseObject.CipherText is not null)
                {
                    var response = ((PowerFactorResponseBase)responseObject);
                    if (!Helper.VerifySign(response.CipherText, response.VerificationSignature, config))
                    {
                        var failureResponse = new PowerFactorResponseBase();
                        failureResponse.Results.Add(
                            new Result()
                            {
                                ErrorCode = "InvalidSign",
                                ErrorMessage = "InvalidSign"
                            });
                        return (T)failureResponse;
                    }

                    var aesToken = Helper.RsaDecryption(response.SecretKey, config);
                    var clearText = Helper.AesDecryption(response.CipherText, aesToken);

                    return (T)Helper.JsonToObject(typeof(T), clearText);
                }

                return responseObject;
            }
            catch (Exception exception)
            {
                var failureResponse = Activator.CreateInstance<T>();
                failureResponse.Results.Add(
                    new Result()
                    {
                        ErrorCode = "Exception",
                        ErrorMessage = exception.Message,
                        ErrorMessageDetails = exception.InnerException?.Message,
                        Exception = exception.InnerException?.InnerException?.Message
                    });

                return failureResponse;
            }
        }
        private static HttpClient GetHttpClient(PowerfactorConfig config)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls |
                                                              System.Net.SecurityProtocolType.Tls11 |
                                                              System.Net.SecurityProtocolType.Tls12;
            if (_httpClient == null) 
            {
                var isSslEnabled = config.SSLEnabled;
                _httpClient = isSslEnabled.Equals("true") ? new HttpClient(GetCertificate()) : new HttpClient();
                _httpClient.BaseAddress = new Uri(config.ServerIP);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            return _httpClient;
        }

        private static HttpClient GetHttpBackOfficeClient(PowerfactorConfig config)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls |
                                                              System.Net.SecurityProtocolType.Tls11 |
                                                              System.Net.SecurityProtocolType.Tls12;
            if (_backOfficeHttpClient == null)
            {
                var isSslEnabled = config.SSLEnabled;
                _backOfficeHttpClient = isSslEnabled.Equals("true") ? new HttpClient(GetCertificate()) : new HttpClient();
                _backOfficeHttpClient.BaseAddress = new Uri(config.BackOfficeIP);
                _backOfficeHttpClient.DefaultRequestHeaders.Accept.Clear();
                _backOfficeHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            return _backOfficeHttpClient;
        }
        private static HttpClientHandler GetCertificate()
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly);
            var collection = store.Certificates.Find(X509FindType.FindBySerialNumber, "", true);

            if (collection.Count == 0)
            {
                return null;
            }

            var certificate = collection[0];

            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ClientCertificates.Add(certificate);

            return handler;
        }
        public static T PostBackOffice<T>(PowerfactorConfig config, object request, string methodName) where T : PowerFactorResponseBase
        {
            try
            {
                var encodeString = Helper.Base64Encode(Helper.ObjectToJson(request));
                PowerFactorRequest pwfRequest = Helper.EncryptRequest(encodeString, config);
                pwfRequest.IsMutualAuthenticationRequired = false;
                pwfRequest.MethodName = methodName;

                _backOfficeHttpClient = GetHttpBackOfficeClient(config);
                HttpContent contentPost = new StringContent(Helper.ObjectToJson(pwfRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage httpresponse = _backOfficeHttpClient.PostAsync(string.Format("{0}{1}", config.BackOfficePath, "Call"), contentPost).Result;
                httpresponse.EnsureSuccessStatusCode();

                string asyncResponse = httpresponse.Content.ReadAsStringAsync().Result;
                var genreralResponseObject = (PowerFactorResponseBase)Helper.JsonToObject(typeof(PowerFactorResponseBase), asyncResponse);
                T responseObject;
                if (pwfRequest.IsMutualAuthenticationRequired && genreralResponseObject.CipherText != null)
                {
                    if (!Helper.VerifySign(genreralResponseObject.CipherText, genreralResponseObject.VerificationSignature, config))
                    {
                        PowerFactorResponseBase failureResponse = new PowerFactorResponseBase();
                        failureResponse.Results.Add(
                            new Result()
                            {
                                ErrorCode = "InvalidSign",
                                ErrorMessage = "InvalidSign"
                            });
                        return (T)failureResponse;
                    }
                    var aesToken = Helper.RsaDecryption(genreralResponseObject.SecretKey, config);
                    var clearText = Helper.AesDecryption(genreralResponseObject.CipherText, aesToken);
                    byte[] data = Convert.FromBase64String(clearText);
                    string jsonContent = Encoding.UTF8.GetString(data);
                    responseObject = (T)Helper.JsonToObject(typeof(T), jsonContent);
                }
                else if (genreralResponseObject.Content != null)
                {
                    byte[] data = Convert.FromBase64String(genreralResponseObject.Content);
                    string jsonContent = Encoding.UTF8.GetString(data);
                    responseObject = (T)Helper.JsonToObject(typeof(T), jsonContent);
                }
                else
                {
                    responseObject = (T)Activator.CreateInstance(typeof(T), new object[] { });
                    responseObject.Results.AddRange(genreralResponseObject.Results);
                }
                return responseObject;
            }
            catch (Exception exception)
            {
                var failureResponse = Activator.CreateInstance<T>();
                failureResponse.Results.Add(
                    new Result()
                    {
                        ErrorCode = "Exception",
                        ErrorMessage = exception.Message,
                        ErrorMessageDetails = exception.InnerException?.Message,
                        Exception = exception.InnerException?.InnerException?.Message
                    });
                return failureResponse;
            }
        }
    }
}