using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Vault;
using LinkPara.Kkb.Application.Commons.Interfaces;
using LinkPara.Kkb.Application.Commons.Models.KkbSettings;
using LinkPara.Kkb.Application.Features.Kkb;
using LinkPara.Kkb.Domain.Entities;
using LinkPara.Kkb.Infrastructure.ExternalServices.Kkb.Models;
using LinkPara.Security.Helpers;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using static LinkPara.Kkb.Infrastructure.ExternalServices.Kkb.Models.KkbValidationResponse;

namespace LinkPara.Kkb.Infrastructure.ExternalServices.Kkb
{
    public class KkbValidationService : IKkbValidationService
    {
        private readonly HttpClient _client;
        private readonly KkbAuthorizationService _kkbAuthorizationService;
        private readonly KkbApiAuthorizationSettings _settings;
        private readonly ILogger<KkbValidationService> _logger;
        private readonly IBus _bus;
        private readonly IParameterService _parameterService;
        private readonly IVaultClient _vaultClient;
        private readonly IGenericRepository<AccountIban> _accountIbanRepository;
        private readonly IApplicationUserService _applicationUserService;
        public const string ParameterGroupCode = "IntegrationLoggerState";
        public const string ParameterCode = "Kkb";

        public KkbValidationService(HttpClient client,
                                    KkbAuthorizationService kkbAuthorizationService,
                                    KkbApiAuthorizationSettings settings,
                                    ILogger<KkbValidationService> logger,
                                    IBus bus,
                                    IParameterService parameterService,
                                    IConfiguration configuration,
                                    IVaultClient vaultClient,
                                    IGenericRepository<AccountIban> kkbValidationRepository,
                                    IApplicationUserService applicationUserService)
        {
            _client = client;
            _kkbAuthorizationService = kkbAuthorizationService;
            _settings = settings;
            _logger = logger;
            _bus = bus;
            _parameterService = parameterService;
            _vaultClient = vaultClient;
            _accountIbanRepository = kkbValidationRepository;
            _applicationUserService = applicationUserService;
        }

        public async Task<ValidateIbanResponse> IbanValidationAsync(string iban, string identityNo)
        {
            if (await CheckValidation(iban, identityNo))
            {
                return new ValidateIbanResponse
                {
                    IsValid = true
                };
            }

            bool isLogEnable;
            string requestXml = string.Empty;
            string responseBody = string.Empty;
            var apiVersion = _vaultClient.GetSecretValue<string>("KkbSecrets", "KkbApiAuthorization", "ApiVersion");
            bool useNewIbanValidationMethod = (apiVersion == "V1") ? false : true;
            try
            {
                var token = await _kkbAuthorizationService.GetTokenAsync();

                _client.BaseAddress = new Uri(_settings.ApiUrl);
                var request = new HttpRequestMessage(HttpMethod.Post, "inb/findeks/yeni-dogrulama");

                var securePass = _settings.Password.ToSecureString();

                request.Headers.Add("Authorization", token);
                requestXml = KkbIbanValidationRequest.CreateRequestXmlForIbanValidation(_settings.UserName, securePass, iban, identityNo, useNewIbanValidationMethod);
                request.Content = new StringContent(requestXml);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");

                var response = await _client.SendAsync(request);
                responseBody = await response.Content.ReadAsStringAsync();

                try
                {
                    try
                    {
                        var param = await _parameterService.GetParameterAsync(ParameterGroupCode, ParameterCode);
                        isLogEnable = Convert.ToBoolean(param.ParameterValue);
                    }
                    catch
                    {
                        isLogEnable = true;
                    }

                    if (isLogEnable)
                    {
                        var correlationId = Guid.NewGuid();
                        var log = new IntegrationLog()
                        {
                            CorrelationId = correlationId.ToString(),
                            Name = ParameterCode,
                            Type = "ValidateIban",
                            Date = DateTime.Now,
                            Request = requestXml,
                            Response = responseBody,
                            DataType = IntegrationLogDataType.Soap
                        };
                        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                        await endpoint.Send(log, cancellationToken.Token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("IntegrationLog Error: Kkb ValidateIban - Exception {ex}", ex);
                }

                response.EnsureSuccessStatusCode();

                var serializer = new XmlSerializer(typeof(Envelope));
                using var reader = new StringReader(responseBody);
                var result = (Envelope)serializer.Deserialize(reader);

                if (useNewIbanValidationMethod)
                {
                    if (result?.Body?.yeniBsgIbanDogrulamaResponse.@return.islemSonucu != null)
                    {
                        var isValid = result.Body.yeniBsgIbanDogrulamaResponse.@return.ibanKimlikNoDogrulamaSonucu;

                        if (isValid)
                            await SaveIbanAsync(iban, identityNo);

                        return new ValidateIbanResponse
                        {
                            IsValid = result.Body.yeniBsgIbanDogrulamaResponse.@return.ibanKimlikNoDogrulamaSonucu
                        };
                    }
                    else
                    {
                        throw new Exception($"IbanValidation Response is not proper: {result?.Body?.yeniBsgIbanDogrulamaResponse.@return.hataMesaji}");
                    }
                }
                else
                {
                    if (result?.Body.bsgIbanDogrulamaResponse?.@return?.islemSonucu != null)
                    {
                        var isValid = result.Body.bsgIbanDogrulamaResponse.@return.ibanKimlikNoDogrulamaSonucu;

                        if (isValid)
                            await SaveIbanAsync(iban, identityNo);

                        return new ValidateIbanResponse()
                        {
                            IsValid = result.Body.bsgIbanDogrulamaResponse.@return.ibanKimlikNoDogrulamaSonucu
                        };
                    }
                    else
                    {
                        throw new Exception($"IbanValidation Response is not proper: {result?.Body?.bsgIbanDogrulamaResponse.@return.hataMesaji}");
                    }
                }

                throw new Exception();

            }
            catch (Exception exception)
            {
                var correlationId = Guid.NewGuid();
                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = ParameterCode,
                    Type = ParameterCode,
                    Date = DateTime.Now,
                    Request = requestXml,
                    Response = responseBody,
                    DataType = IntegrationLogDataType.Soap
                };
                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);

                _logger.LogError("IbanValidation exception detail:\n{exception}", exception);
                throw;
            }
        }
        private async Task<bool> CheckValidation(string iban, string identityNo)
        {
            var isValidate = await _accountIbanRepository.GetAll()
                .FirstOrDefaultAsync(v =>
                v.Iban == iban
                && v.IdentityNo == identityNo
                && v.RecordStatus == RecordStatus.Active);

            return isValidate is not null;
        }
        
        private async Task SaveIbanAsync(string iban, string identityNo)
        {
            var newValidation = new AccountIban
            {
                Iban = iban,
                IdentityNo = identityNo,
                CreateDate = DateTime.Now,
                RecordStatus = RecordStatus.Active,
                CreatedBy = _applicationUserService.ApplicationUserId.ToString()
            };
            await _accountIbanRepository.AddAsync(newValidation);
        }

        public async Task<InquireIbanResponse> IbanInquireAsync(string iban)
        {
            bool isLogEnable;
            string requestXml = string.Empty;
            string responseBody = string.Empty;

            try
            {
                var token = await _kkbAuthorizationService.GetTokenAsync();
                _client.BaseAddress = new Uri(_settings.ApiUrl);
                var request = new HttpRequestMessage(HttpMethod.Post, "inb/findeks/yeni-dogrulama");
                
                request.Headers.Add("Authorization", token);
                var securePass = _settings.Password.ToSecureString();
                requestXml = KkbIbanInquireRequest.CreateRequestXmlForIbanInquire(_settings.UserName, securePass, iban);
                request.Content = new StringContent(requestXml);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");

                var response = await _client.SendAsync(request);
                responseBody = await response.Content.ReadAsStringAsync();

                try
                {
                    try
                    {
                        var param = await _parameterService.GetParameterAsync(ParameterGroupCode, ParameterCode);
                        isLogEnable = Convert.ToBoolean(param.ParameterValue);
                    }
                    catch
                    {
                        isLogEnable = true;
                    }

                    if (isLogEnable)
                    {
                        var correlationId = Guid.NewGuid();
                        var log = new IntegrationLog()
                        {
                            CorrelationId = correlationId.ToString(),
                            Name = ParameterCode,
                            Type = "InquireIban",
                            Date = DateTime.Now,
                            Request = requestXml,
                            Response = responseBody,
                            DataType = IntegrationLogDataType.Soap
                        };
                        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                        await endpoint.Send(log, cancellationToken.Token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("IntegrationLog Error: Kkb InquireIban - Exception {ex}", ex);
                }

                response.EnsureSuccessStatusCode();
                               
                var InquireIbanResponse = await FromXmlAsync(responseBody);
                var result = new InquireIbanResponse();
                result.TitleList = InquireIbanResponse.TitleList;
                return result;
            }
            catch (Exception exception)
            {
                var correlationId = Guid.NewGuid();
                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = ParameterCode,
                    Type = "InquireIban",
                    Date = DateTime.Now,
                    Request = requestXml,
                    Response = responseBody,
                    DataType = IntegrationLogDataType.Soap
                };
                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);

                _logger.LogError("InquireIban exception detail:\n{exception}", exception);
                throw;
            }
        }

        public static async Task<IbanInquiryResult> FromXmlAsync(string xml)
        {
            var doc = new XmlDocument();
            if (xml.TrimStart().StartsWith("<?xml"))
            {
                int endOfDeclaration = xml.IndexOf("?>");
                if (endOfDeclaration > -1)
                {
                    string declaration = xml.Substring(0, endOfDeclaration + 2);
                    string fixedDeclaration = declaration.Replace("'", "\"");
                    xml = fixedDeclaration + xml.Substring(endOfDeclaration + 2);
                }
            }
                        
            xml = Regex.Replace(xml, @"xmlns:(\w+)=((http|https)[^ >]*)", @"xmlns:$1=""$2""");
            xml = Regex.Replace(xml, @"xmlns=([^""\s>]+)", @"xmlns=""$1""");

            doc.LoadXml(xml);

            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("S", "http://schemas.xmlsoap.org/soap/envelope/");
            namespaceManager.AddNamespace("ns0", "http://wsv2.ers.kkb.com.tr/");

            var returnNode = doc.SelectSingleNode("//ns0:yeniBsgIbanSorgulamaResponse/return", namespaceManager);

            string errorCode = returnNode.SelectSingleNode("hataKodu")?.InnerText;
            string errorMessage = returnNode.SelectSingleNode("hataMesaji")?.InnerText;
            string operationResult = returnNode.SelectSingleNode("islemSonucu")?.InnerText;

            var result = new IbanInquiryResult
            {
                OperationResult = operationResult,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };

            if (operationResult == "1")
            {
                result.AccountCurrency = returnNode.SelectSingleNode("hesapDovizCinsi")?.InnerText;

                var titleNodes = returnNode.SelectNodes("ibanBilgiList/unvan");
                if (titleNodes != null)
                {
                    result.TitleList = titleNodes
                        .Cast<XmlNode>()
                        .Select(n => n.InnerText.Trim())
                        .Where(text => !string.IsNullOrWhiteSpace(text))
                        .ToList();
                }
            }

            return await Task.FromResult(result);
        }

    }
}
