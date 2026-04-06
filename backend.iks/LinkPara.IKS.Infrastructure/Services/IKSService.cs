using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.Configurations;
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Response;
using LinkPara.IKS.Application.Commons.Models.IKSModels.CardBins.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Response;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Response;
using LinkPara.IKS.Application.Enums;
using LinkPara.IKS.Application.Features.Annulments.Queries.GetAnnulments;
using LinkPara.IKS.Application.Features.Merchant.Command.SaveMerchant;
using LinkPara.IKS.Domain.Entities;
using LinkPara.IKS.Domain.Enums;
using LinkPara.IKS.Infrastructure.ExternalService.IKS.Models;
using LinkPara.IKS.Infrastructure.ExternalService.IKS.Models.Responses;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static MassTransit.ValidationResultExtensions;
using IKSError = LinkPara.IKS.Application.Commons.Models.IKSModels.IKSError;
using IKSMerchantResponse = LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Response.IKSMerchantResponse;

namespace LinkPara.IKS.Infrastructure.Services
{
    public class IKSService : IIKSService
    {
        private readonly ICacheService _cacheService;
        private IKSTokenResponse cachedToken;
        private HttpClient _client;

        private readonly IKSSettings _settings;
        private readonly ILogger<SaveMerchantCommand> _logger;
        private readonly IGenericRepository<IKSTransaction> _iKSTransactionRepository;
        private readonly IGenericRepository<TimeoutIKSTransaction> _timeout_IKSTransactionRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IParameterService _parameterService;
        private readonly IBus _bus;
        private readonly IGenericRepository<IksTerminal> _iksTerminalRepository;
        private readonly IGenericRepository<IksTerminalHistory> _iksTerminalHistoryRepository;
        private readonly IContextProvider _contextProvider;
        private readonly IUserService _userService;
        private readonly IApplicationUserService _applicationUserService;
        private const string timeOutStatusCode = "-1686";
        private const string certificatePath = "/iks/certificate.pfx";
        public const string ParameterGroupCode = "IntegrationLoggerState";
        public const string ParameterCode = "Iks";

        public IKSService(
                          IVaultClient vaultClient,
                          ILogger<SaveMerchantCommand> logger,
                          ICacheService cacheService,
                          IGenericRepository<IKSTransaction> iKSTransactionRepository,
                          IAuditLogService auditLogService,
                          IGenericRepository<TimeoutIKSTransaction> timeout_IKSTransactionRepository,
                          IBus bus,
                          IParameterService parameterService,
                          IGenericRepository<IksTerminal> iksTerminalRepository,
                          IContextProvider contextProvider,
                          IUserService userService,
                          IApplicationUserService applicationUserService,
                          IGenericRepository<IksTerminalHistory> iksTerminalHistoryRepository)
        {
            _cacheService = cacheService;
            _logger = logger;


            _settings = vaultClient.GetSecretValue<IKSSettings>("IKSSecrets", "IKSSettings", null);
            _iKSTransactionRepository = iKSTransactionRepository;
            _auditLogService = auditLogService;
            _timeout_IKSTransactionRepository = timeout_IKSTransactionRepository;
            _bus = bus;
            _parameterService = parameterService;
            _iksTerminalRepository = iksTerminalRepository;
            _contextProvider = contextProvider;
            _userService = userService;
            _applicationUserService = applicationUserService;
            _iksTerminalHistoryRepository = iksTerminalHistoryRepository;
        }


        #region Merchant
        public async Task<IKSResponse<IKSMerchantResponse>> SaveMerchantAsync(MerchantRequest request)
        {
            try
            {
                var result = await ExecuteAsync<IKSMerchantResponse>(_settings.APIEndPoint + _settings.CreateMerchantEndPoint, request, IKSRequestType.POST, IksOperationType.SaveMerchant);

                string requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);
                string responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data?.merchant != null
                    ? result.Data?.merchant : result.Error, Newtonsoft.Json.Formatting.Indented);

                //x.code == "-1682"; Timeouta düstüyse;tekrar tetiklendiginde "-1682" işlem devam ediyor lütfen bekleyiniz hatası alıyorum.
                var timeOut = result.Data?.merchant?.additionalInfo?.FirstOrDefault(x => x.code == timeOutStatusCode);
                var operation = "Save Merchant";
                result.StatusCode = "200";

                if (timeOut != null)
                {
                    operation = "Timeout Save Merchant";
                    result.StatusCode = timeOutStatusCode;

                    await _timeout_IKSTransactionRepository.AddAsync(
                    new TimeoutIKSTransaction
                    {
                        CreatedBy = Guid.Empty.ToString(),
                        IsSuccess = false,
                        ResponseCode = result.IsSuccess ? timeOutStatusCode : result.Error?.httpCode.ToString(),
                        Operation = operation,
                        MerchantId = request.MerchantId,
                        RequestDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson),
                        ResponseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)
                    }
                    );
                }
                else
                {
                    await _iKSTransactionRepository.AddAsync(
                    new IKSTransaction
                    {
                        CreatedBy = Guid.Empty.ToString(),
                        IsSuccess = result.IsSuccess,
                        ResponseCode = result.IsSuccess ? "200" : result.Error?.httpCode.ToString(),
                        Operation = operation,
                        MerchantId = request.MerchantId,
                        RequestDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson),
                        ResponseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)
                    }
                    );
                }

                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = result.IsSuccess,
                    LogDate = DateTime.Now,
                    Operation = operation,
                    SourceApplication = "IKS",
                    Resource = "IKSTransaction",
                    Details = new Dictionary<string, string>
                    {
                         {"MerchantName", request.MerchantName },
                         {"PspMerchantIds", request.PspMerchantId },
                         {"ManagerName", request.ManagerName }
                    }
                });

                ConfigureErrorObject(ref result);

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKS savemerchant error : {exception}");
                throw;
            }
        }

        private void ConfigureErrorObject(ref IKSResponse<IKSMerchantResponse> result)
        {

            if (result.Error != null && result.Error.errors != null && result.Error.errors.Length > 0)
            {
                result.Error.moreInformation = (!String.IsNullOrEmpty(result.Error.moreInformation) ? $"{result.Error.moreInformation} - " : String.Empty) + string.Join(" ", result.Error.errors.Select(x => x.description));
            }
        }

        public async Task<IKSResponse<IKSMerchantResponse>> UpdateMerchantAsync(UpdateMerchantRequest request)
        {
            try
            {
                var result = await ExecuteAsync<IKSMerchantResponse>(_settings.APIEndPoint + _settings.UpdateMerchantEndPoint, request, IKSRequestType.PUT, IksOperationType.UpdateMerchant);

                string requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);
                string responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data?.merchant != null
                    ? result.Data?.merchant : result.Error, Newtonsoft.Json.Formatting.Indented);

                //x.code == "-1682"; Timeouta düstüyse;tekrar tetiklendiginde "-1682" işlem devam ediyor lütfen bekleyiniz hatası alıyorum.
                var timeOut = result.Data?.merchant?.additionalInfo?.FirstOrDefault(x => x.code == timeOutStatusCode);
                var operation = "Update Merchant";
                result.StatusCode = "200";

                if (timeOut != null)
                {
                    operation = "Timeout Update Merchant";
                    result.StatusCode = timeOutStatusCode;

                    await _timeout_IKSTransactionRepository.AddAsync(
                    new TimeoutIKSTransaction
                    {
                        CreatedBy = Guid.Empty.ToString(),
                        IsSuccess = false,
                        ResponseCode = result.IsSuccess ? timeOutStatusCode : result.Error?.httpCode.ToString(),
                        Operation = operation,
                        MerchantId = request.MerchantId,
                        RequestDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson),
                        ResponseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)
                    }
                    );

                }
                else
                {

                    await _iKSTransactionRepository.AddAsync(
                      new IKSTransaction
                      {
                          CreatedBy = Guid.Empty.ToString(),
                          IsSuccess = result.IsSuccess,
                          ResponseCode = result.IsSuccess ? "200" : result.Error?.httpCode.ToString(),
                          Operation = "Update Merchant",
                          MerchantId = request.MerchantId,
                          RequestDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson),
                          ResponseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)
                      }
                    );
                }

                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = result.IsSuccess,
                    LogDate = DateTime.Now,
                    Operation = operation,
                    SourceApplication = "IKS",
                    Resource = "IKSTransaction",
                    Details = new Dictionary<string, string>
                    {
                          {"GlobalMerchantId", request.GlobalMerchantId },
                          {"MerchantName", request.MerchantName },
                          {"ManagerName", request.ManagerName }
                    }
                });

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKS update merchant error : {exception}");
                throw;
            }
        }

        public async Task<IKSResponse<IKSMerchantsQueryResponse>> MerchantsQueryAsync(MerchantsQueryRequest request)
        {
            try
            {
                var result = await ExecuteAsync<IKSMerchantsQueryResponse>(_settings.APIEndPoint + _settings.MerchantsQueryEndPoint, request, IKSRequestType.POST, IksOperationType.MerchantsQuery);

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKS Get Merchants Query error : {exception}");
                throw;
            }
        }

        #endregion

        #region Terminal

        public async Task<IKSResponse<IKSTerminalResponse>> SaveTerminalAsync(SaveTerminalRequest request)
        {
            try
            {
                var result = await ExecuteAsync<IKSTerminalResponse>(_settings.APIEndPoint + _settings.TerminalEndPoint, request, IKSRequestType.POST, IksOperationType.SaveTerminal);

                string requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);
                string responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data?.terminal != null
                    ? result.Data?.terminal : result.Error, Newtonsoft.Json.Formatting.Indented);

                await _iKSTransactionRepository.AddAsync(
                  new IKSTransaction
                  {
                      CreatedBy = Guid.Empty.ToString(),
                      IsSuccess = result.IsSuccess,
                      ResponseCode = result.IsSuccess ? "200" : result.Error?.httpCode.ToString(),
                      Operation = "Save Terminal",
                      MerchantId = request.MerchantId,
                      RequestDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson),
                      ResponseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)
                  }
                );

                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = result.IsSuccess,
                    LogDate = DateTime.Now,
                    Operation = "SaveTerminal",
                    SourceApplication = "IKS",
                    Resource = "IKSTransaction",
                    Details = new Dictionary<string, string>
                    {
                           {"GlobalMerchantId", request.GlobalMerchantId },
                           {"PspMerchantId", request.PspMerchantId },
                           {"TerminalId", request.TerminalId },
                           {"OwnerTerminalId", request.OwnerTerminalId }
                    }
                });

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKS saveterminal error : {exception}");
                throw;
            }
        }
        
        public async Task<IKSResponse<IKSTerminalResponse>> CreateTerminalAsync(CreateTerminalRequest request)
        {
            try
            {
                var userId = _contextProvider.CurrentContext.UserId;
                var parseUserId = userId != null ? Guid.Parse(userId) : _applicationUserService.ApplicationUserId;
                var user = await _userService.GetUserAsync(parseUserId);

                var result = await ExecuteAsync<IKSTerminalResponse>(_settings.APIEndPoint + _settings.TerminalCreateEndPoint, request, IKSRequestType.POST, IksOperationType.CreateTerminal);
                
                var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);
                var responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data?.terminal != null
                    ? result.Data?.terminal : result.Error, Newtonsoft.Json.Formatting.Indented);

                await _iKSTransactionRepository.AddAsync(
                  new IKSTransaction
                  {
                      CreatedBy = parseUserId.ToString(),
                      IsSuccess = result.IsSuccess,
                      ResponseCode = result.IsSuccess ? "200" : result.Error?.httpCode.ToString(),
                      Operation = "Create Terminal",
                      MerchantId = request.MerchantId,
                      RequestDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson),
                      ResponseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)
                  }
                );

                var terminalResponse = result.Data?.terminal;
                var errorMessage = string.Join("; ", result.Error?.errors.Select(s => s.description) ?? Enumerable.Empty<string>());
                var terminalStatus = TerminalStatus.SentRequest;
                if (!string.IsNullOrWhiteSpace(terminalResponse.terminalId))
                {
                    terminalStatus = TerminalStatus.Active;
                }

                if (terminalResponse.responseCode != null && terminalResponse.responseCode != "00" && terminalResponse.responseCode != "200")
                {
                    terminalStatus = TerminalStatus.Reject;
                }

                if (terminalResponse.statusCode != "0")
                {
                    terminalStatus = TerminalStatus.Passive;
                }

                await _iksTerminalRepository.AddAsync(new IksTerminal
                {
                    MerchantId = request.MerchantId,
                    ReferenceCode = terminalResponse?.referenceCode,
                    GlobalMerchantId = request.GlobalMerchantId,
                    PspMerchantId = request.PspMerchantId,
                    TerminalId = string.Empty,
                    StatusCode = request.StatusCode,
                    Type = request.Type,
                    BrandCode = request.BrandCode,
                    Model = request.Model,
                    SerialNo = request.SerialNo,
                    OwnerPspNo = request.OwnerPspNo,
                    OwnerTerminalId = request.OwnerTerminalId,
                    BrandSharing = string.Empty,
                    PinPad = request.PinPad.HasValue ? request.PinPad.ToString() : string.Empty,
                    Contactless = request.Contactless.HasValue ? request.Contactless.ToString() : string.Empty,
                    ConnectionType = request.ConnectionType,
                    VirtualPosUrl = request.VirtualPosUrl,
                    HostingTaxNo = request.HostingTaxNo,
                    HostingTradeName = request.HostingTradeName,
                    HostingUrl = request.HostingUrl,
                    PaymentGwTaxNo = request.PaymentGwTaxNo,
                    PaymentGwTradeName = request.PaymentGwTradeName,
                    PaymentGwUrl = request.PaymentGwUrl,
                    ServiceProviderPspNo = request.ServiceProviderPspNo,
                    FiscalNo = request.FiscalNo,
                    TechPos = request.TechPos,
                    ServiceProviderPspMerchantId = string.Empty,
                    PfMainMerchantId = request.PfMainMerchantId,
                    ResponseCode = result.IsSuccess ? "200" : result.Error?.httpCode.ToString(),
                    ResponseCodeExplanation = errorMessage[..Math.Min(2000, errorMessage.Length)],
                    CreatedBy = parseUserId.ToString(),
                    VposId = request.VposId,
                    CreatedNameBy = user is not null ? (user.FirstName + " " + user.LastName) : string.Empty,
                    TerminalStatus = terminalStatus,
                    PhysicalPosId = request.PhysicalPosId
                });

                await _iksTerminalHistoryRepository.AddAsync(new IksTerminalHistory
                {
                    MerchantId = request.MerchantId,
                    VposId = request.VposId,
                    PhysicalPosId = request.PhysicalPosId,
                    OldData = string.Empty,
                    NewData =  string.Empty,
                    ChangedField = string.Empty,
                    ReferenceCode = terminalResponse?.referenceCode,
                    ResponseCode = result.IsSuccess ? "200" : result.Error?.httpCode.ToString(),
                    ResponseCodeExplanation = errorMessage[..Math.Min(2000, errorMessage.Length)],
                    CreatedBy = parseUserId.ToString(),
                    QueryDate = DateTime.Now,
                    TerminalRecordType = TerminalRecordType.NewTerminalRequest
                });
                
                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = result.IsSuccess,
                    LogDate = DateTime.Now,
                    Operation = "CreateTerminal",
                    SourceApplication = "IKS",
                    Resource = "IKSTransaction",
                    Details = new Dictionary<string, string>
                    {
                           {"GlobalMerchantId", request.GlobalMerchantId },
                           {"PspMerchantId", request.PspMerchantId },
                           {"ReferenceCode", result.Data?.terminal?.referenceCode }
                    }
                });

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKS create terminal error : {exception}");
                throw;
            }
        }

        public async Task<IKSResponse<IksTerminalQueryResponse>> GetTerminalStatusQueryAsync(GetTerminalStatusRequest request)
        {
            try
            {
                var userId = _contextProvider.CurrentContext.UserId;
                var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
                
                var result = await ExecuteAsync<IksTerminalQueryResponse>(_settings.APIEndPoint + _settings.TerminalCreateEndPoint, request, IKSRequestType.GET, IksOperationType.CreateTerminal);
                
                var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);
                var responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data != null
                    ? result.Data : result.Error, Newtonsoft.Json.Formatting.Indented);

                await _iKSTransactionRepository.AddAsync(
                  new IKSTransaction
                  {
                      CreatedBy = parseUserId.ToString(),
                      IsSuccess = result.IsSuccess,
                      ResponseCode = result.IsSuccess ? "200" : result.Error?.httpCode.ToString(),
                      Operation = !string.IsNullOrEmpty(request.ReferenceCode) ? "CheckTerminalStatusByReferenceCode" : "CheckTerminalStatusCron",
                      MerchantId = Guid.Empty,
                      RequestDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson),
                      ResponseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)
                  }
                );
                
                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKS get terminal status error : {exception}");
                throw;
            }
        }

        public IksTerminal UpdateIksTerminalFields(IksTerminal iksTerminal,
            Application.Commons.Models.IKSModels.Terminal.Response.IKSTerminal response)
        {
            var terminalStatus = TerminalStatus.SentRequest;

            if (!string.IsNullOrWhiteSpace(response.terminalId))
            {
                terminalStatus = TerminalStatus.Active;
            }

            if (response.responseCode != null && response.responseCode != "00" && response.responseCode != "200")
            {
                terminalStatus = TerminalStatus.Reject;
            }

            if (response.statusCode != "0")
            {
                terminalStatus = TerminalStatus.Passive;
            }            

            iksTerminal.GlobalMerchantId = response.globalMerchantId;
            iksTerminal.PspMerchantId = response.pspMerchantId;
            iksTerminal.TerminalId = response.terminalId;
            iksTerminal.StatusCode = response.statusCode;
            iksTerminal.Type = response.type;
            iksTerminal.BrandCode = response.brandCode;
            iksTerminal.Model = response.model;
            iksTerminal.SerialNo = response.serialNo;
            iksTerminal.OwnerPspNo = response.ownerPspNo;
            iksTerminal.OwnerTerminalId = response.ownerTerminalId;
            iksTerminal.BrandSharing = response.brandSharing;
            iksTerminal.PinPad = response.pinPad;
            iksTerminal.Contactless = response.contactless;
            iksTerminal.ConnectionType = response.connectionType;
            iksTerminal.VirtualPosUrl = response.virtualPosUrl;
            iksTerminal.HostingTaxNo = response.hostingTaxNo;
            iksTerminal.HostingTradeName = response.hostingTradeName;
            iksTerminal.HostingUrl = response.hostingUrl;
            iksTerminal.PaymentGwTaxNo = response.paymentGwTaxNo;
            iksTerminal.PaymentGwTradeName = response.paymentGwTradeName;
            iksTerminal.PaymentGwUrl = response.paymentGwUrl;
            iksTerminal.ServiceProviderPspNo = response.serviceProviderPspNo;
            iksTerminal.FiscalNo = response.fiscalNo;
            iksTerminal.TechPos = Convert.ToInt32(response.techPos);
            iksTerminal.ServiceProviderPspMerchantId = response.serviceProviderPspMerchantId;
            iksTerminal.PfMainMerchantId = response.pfMainMerchantId;
            iksTerminal.ResponseCode = response.responseCode;
            iksTerminal.ResponseCodeExplanation = response.responseCodeExplanation;
            iksTerminal.TerminalStatus = terminalStatus;

            return iksTerminal;
        }

        public IksTerminal UpdatePassiveIksTerminalFields(IksTerminal iksTerminal,
            Application.Commons.Models.IKSModels.Terminal.Response.IKSTerminal response)
        {
            iksTerminal.GlobalMerchantId = response.globalMerchantId;
            iksTerminal.PspMerchantId = response.pspMerchantId;
            iksTerminal.TerminalId = response.terminalId;
            iksTerminal.StatusCode = response.statusCode;
            iksTerminal.Type = response.type;
            iksTerminal.BrandCode = response.brandCode;
            iksTerminal.Model = response.model;
            iksTerminal.SerialNo = response.serialNo;
            iksTerminal.OwnerPspNo = response.ownerPspNo;
            iksTerminal.OwnerTerminalId = response.ownerTerminalId;
            iksTerminal.BrandSharing = response.brandSharing;
            iksTerminal.PinPad = response.pinPad;
            iksTerminal.Contactless = response.contactless;
            iksTerminal.ConnectionType = response.connectionType;
            iksTerminal.VirtualPosUrl = response.virtualPosUrl;
            iksTerminal.HostingTaxNo = response.hostingTaxNo;
            iksTerminal.HostingTradeName = response.hostingTradeName;
            iksTerminal.HostingUrl = response.hostingUrl;
            iksTerminal.PaymentGwTaxNo = response.paymentGwTaxNo;
            iksTerminal.PaymentGwTradeName = response.paymentGwTradeName;
            iksTerminal.PaymentGwUrl = response.paymentGwUrl;
            iksTerminal.ServiceProviderPspNo = response.serviceProviderPspNo;
            iksTerminal.FiscalNo = response.fiscalNo;
            iksTerminal.TechPos = Convert.ToInt32(response.techPos);
            iksTerminal.ServiceProviderPspMerchantId = response.serviceProviderPspMerchantId;
            iksTerminal.PfMainMerchantId = response.pfMainMerchantId;
            iksTerminal.ResponseCode = response.responseCode;
            iksTerminal.ResponseCodeExplanation = response.responseCodeExplanation;
            iksTerminal.TerminalStatus = TerminalStatus.Passive;

            return iksTerminal;
        }

        public async Task CreateTerminalHistoryAsync(IksTerminal iksTerminal,
            Application.Commons.Models.IKSModels.Terminal.Response.IKSTerminal response)
        {
            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : _applicationUserService.ApplicationUserId;

            await _iksTerminalHistoryRepository.AddAsync(new IksTerminalHistory
            {
                MerchantId = iksTerminal.MerchantId,
                VposId = iksTerminal.VposId,
                PhysicalPosId = iksTerminal.PhysicalPosId,
                OldData = "Active",
                NewData = "Passive",
                ChangedField = "RecordStatus",
                ReferenceCode = iksTerminal.ReferenceCode,
                ResponseCode = response.responseCode,
                ResponseCodeExplanation = response.responseCodeExplanation,
                CreatedBy = parseUserId.ToString(),
                QueryDate = DateTime.Now,
                TerminalRecordType = TerminalRecordType.PfTerminalUpdate
            });

        }

        public async Task<IKSResponse<IKSTerminalResponse>> UpdateTerminalAsync(UpdateTerminalRequest request)
        {
            try
            {
                var result = await ExecuteAsync<IKSTerminalResponse>(_settings.APIEndPoint + _settings.TerminalEndPoint, request, IKSRequestType.PUT, IksOperationType.UpdateTerminal);

                string requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);
                string responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data?.terminal != null
                    ? result.Data?.terminal : result.Error, Newtonsoft.Json.Formatting.Indented);

                await _iKSTransactionRepository.AddAsync(
                  new IKSTransaction
                  {
                      CreatedBy = Guid.Empty.ToString(),
                      IsSuccess = result.IsSuccess,
                      ResponseCode = result.IsSuccess ? "200" : result.Error?.httpCode.ToString(),
                      Operation = "Update Terminal",
                      MerchantId = request.MerchantId,
                      RequestDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson),
                      ResponseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)
                  }
                );

                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = result.IsSuccess,
                    LogDate = DateTime.Now,
                    Operation = "UpdateTerminal",
                    SourceApplication = "IKS",
                    Resource = "IKSTransaction",
                    Details = new Dictionary<string, string>
                    {
                          {"GlobalMerchantId", request.GlobalMerchantId },
                          {"PspMerchantId", request.PspMerchantId },
                          {"TerminalId", request.TerminalId },
                          {"OwnerTerminalId", request.OwnerTerminalId }
                    }
                });

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKS update terminal error : {exception}");
                throw;
            }
        }
        #endregion

        #region Annulments
        public async Task<IKSResponse<IKSAnnulmentResponse>> SaveAnnulmentAsync(SaveAnnulmentRequest request)
        {
            try
            {
                var result = await ExecuteAsync<IKSAnnulmentResponse>(_settings.APIEndPoint + _settings.AnnulmentsEndPoint, request, IKSRequestType.POST, IksOperationType.SaveAnnulment);

                string requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);
                string responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data?.annulment != null
                    ? result.Data?.annulment : result.Error, Newtonsoft.Json.Formatting.Indented);

                await _iKSTransactionRepository.AddAsync(
                    new IKSTransaction
                    {
                        CreatedBy = Guid.Empty.ToString(),
                        IsSuccess = result.IsSuccess,
                        ResponseCode = result.IsSuccess ? "200" : result.Error?.httpCode.ToString(),
                        Operation = "Save Annulment",
                        MerchantId = request.MerchantId,
                        RequestDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson),
                        ResponseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)
                    }
                );

                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = result.IsSuccess,
                    LogDate = DateTime.Now,
                    Operation = " SaveAnnulment",
                    SourceApplication = "IKS",
                    Resource = "IKSTransaction",
                    Details = new Dictionary<string, string>
                    {
                            {"GlobalMerchantId", request.GlobalMerchantId },
                            {"Code", request.Code }
                    }
                });

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKS saveAnnulmentAsync error : {exception}");
                throw;
            }
        }

        public async Task<IKSResponse<IKSAnnulmentResponse>> UpdateAnnulmentAsync(UpdateAnnulmentRequest request)
        {
            try
            {
                var result = await ExecuteAsync<IKSAnnulmentResponse>(_settings.APIEndPoint + _settings.AnnulmentsEndPoint, request, IKSRequestType.PUT, IksOperationType.UpdateAnnulment);

                string requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);
                string responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data?.annulment != null
                    ? result.Data?.annulment : result.Error, Newtonsoft.Json.Formatting.Indented);

                await _iKSTransactionRepository.AddAsync(
                    new IKSTransaction
                    {
                        CreatedBy = Guid.Empty.ToString(),
                        IsSuccess = result.IsSuccess,
                        ResponseCode = result.IsSuccess ? "200" : result.Error?.httpCode.ToString(),
                        Operation = "Update Annulment",
                        MerchantId = request.MerchantId,
                        RequestDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson),
                        ResponseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)
                    }
                );

                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = result.IsSuccess,
                    LogDate = DateTime.Now,
                    Operation = " SaveAnnulment",
                    SourceApplication = "IKS",
                    Resource = "IKSTransaction",
                    Details = new Dictionary<string, string>
                    {
                           {"GlobalMerchantId", request.GlobalMerchantId },
                           {"Code", request.Code }
                    }
                });

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKS update annulment error : {exception}");
                throw;
            }
        }

        public async Task<IKSResponse<AnnulmentCodesResponse>> GetAnnulmentCodesAsync()
        {
            try
            {
                var result = await ExecuteAsync<AnnulmentCodesResponse>(_settings.APIEndPoint + _settings.AnnulmentCodesEndPoint, null, IKSRequestType.GET, IksOperationType.GetAnnulmentCodes);
                result.Data.annulmentCodes = result.Data.annulmentCodes.OrderBy(c => c.description).ToArray();
                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKS Get Annulment Codes error : {exception}");
                throw;
            }
        }

        public async Task<IKSResponse<IKSAnnulmentsQueryResponse>> GetAnnulmentsQueryAsync(GetAnnulmetsQuery request)
        {
            try
            {
                var result = await ExecuteAsync<IKSAnnulmentsQueryResponse>(_settings.APIEndPoint + _settings.AnnulmentsQueryEndPoint, request, IKSRequestType.GET, IksOperationType.AnnulmentsQuery);

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKS Get Annulments Query error : {exception}");
                throw;
            }
        }

        #endregion

        #region Execute
        private async Task<IKSResponse<T>> ExecuteAsync<T>(string endPoint, object parameters, IKSRequestType requestType, IksOperationType operationType)
        {
            cachedToken = _cacheService.Get<IKSTokenResponse>("IKSToken");

            if (cachedToken is null || DateTime.Now > cachedToken.ExpireDate)
            {
                var token = await GetToken();
                token.ExpireDate = DateTime.Now.AddSeconds(token.expires_in);
                _cacheService.Add("IKSToken", token, 60);
                cachedToken = token;
            }
            _client = CreatePfxFile();

            _client.BaseAddress = new Uri(endPoint);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cachedToken.access_token);

            HttpResponseMessage response = new HttpResponseMessage();
            response = await IKSRequestTypeAsync(endPoint, parameters, requestType, response, operationType);

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(responseString);

            if (!response.IsSuccessStatusCode)
            {
                var errorResult = JsonSerializer.Deserialize<IKSError>(responseString);

                return new IKSResponse<T>
                {
                    Error = new IKSError
                    {
                        httpCode = (int)errorResult.httpCode,
                        httpMessage = errorResult.httpMessage,
                        type = errorResult.type,
                        path = errorResult.path,
                        moreInformation = errorResult.moreInformation,
                        errors = errorResult.errors,
                        timestamp = errorResult.timestamp,
                    }
                };
            }
            return new IKSResponse<T>()
            {
                Data = result
            };

        }
        public async Task<IKSResponse<T>> CardBinExecuteAsync<T>(string endPoint, object parameters, IKSRequestType requestType, IksOperationType operationType)
        {
            cachedToken = _cacheService.Get<IKSTokenResponse>("IKSToken");

            if (cachedToken is null || DateTime.Now > cachedToken.ExpireDate)
            {
                var token = await GetToken();
                token.ExpireDate = DateTime.Now.AddSeconds(token.expires_in);
                _cacheService.Add("IKSToken", token, 60);
                cachedToken = token;
            }
            _client = CreatePfxFile();

            _client.BaseAddress = new Uri(endPoint);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cachedToken.access_token);

            HttpResponseMessage response = new HttpResponseMessage();

            response = await _client.GetAsync(endPoint);

            var responseString = await response.Content.ReadAsStringAsync();
            string requestString = JsonSerializer.Serialize(parameters);
            try
            {

                var correlationId = Guid.NewGuid();
                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = "BkmCardBin",
                    Type = operationType.ToString(),
                    Date = DateTime.Now,
                    Request = requestString,
                    Response = responseString,
                    DataType = IntegrationLogDataType.Text
                };
                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);

            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrationLog Error: Iks Card Bin - Exception {ex}");
                throw;
            }

            var result = JsonSerializer.Deserialize<T>(responseString);

            if (!response.IsSuccessStatusCode)
            {
                var errorResult = JsonSerializer.Deserialize<IKSError>(responseString);

                return new IKSResponse<T>
                {
                    Error = new IKSError
                    {
                        httpCode = (int)errorResult.httpCode,
                        httpMessage = errorResult.httpMessage,
                        type = errorResult.type,
                        path = errorResult.path,
                        moreInformation = errorResult.moreInformation,
                        errors = errorResult.errors,
                        timestamp = errorResult.timestamp,
                    }
                };
            }
            return new IKSResponse<T>()
            {
                Data = result
            };

        }
        private async Task<HttpResponseMessage> IKSRequestTypeAsync(string endPoint, object parameters, IKSRequestType requestType,
            HttpResponseMessage response, IksOperationType operationType)
        {
            string requestString = JsonSerializer.Serialize(parameters);
            string responseString = string.Empty;
            try
            {

                if (requestType == IKSRequestType.POST)
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    };

                    var postRequest = new HttpRequestMessage(HttpMethod.Post, endPoint);
                    postRequest.Content = new StringContent(JsonSerializer.Serialize(parameters, options), Encoding.UTF8, "application/json");
                    postRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await _client.SendAsync(postRequest);
                }
                else if (requestType == IKSRequestType.PUT)
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    };

                    var httpContent = new StringContent(JsonSerializer.Serialize(parameters, options), Encoding.UTF8, "application/json");

                    response = await _client.PutAsync(endPoint, httpContent);
                }
                else if (requestType == IKSRequestType.GET && operationType != IksOperationType.CreateTerminal)
                {
                    if (parameters != null)
                    {
                        var model = (GetAnnulmetsQuery)(parameters);
                        response = await _client.GetAsync($"{endPoint}?globalMerchantId={model?.GlobalMerchantId}");
                    }
                    else
                    {
                        response = await _client.GetAsync(endPoint);
                    }

                }
                else if (requestType == IKSRequestType.GET && operationType == IksOperationType.CreateTerminal)
                {
                    var model = (GetTerminalStatusRequest)(parameters);
                    var queryString = BuildQueryString(model);
                    var url = string.IsNullOrEmpty(queryString) ? endPoint : $"{endPoint}?{queryString}";
                    response = await _client.GetAsync(url);

                }
                responseString = await response.Content.ReadAsStringAsync();
                try
                {
                    bool isLogEnable;
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
                            Type = operationType.ToString(),
                            Date = DateTime.Now,
                            Request = requestString,
                            Response = responseString,
                            DataType = IntegrationLogDataType.Text
                        };
                        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                        await endpoint.Send(log, cancellationToken.Token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrationLog Error: Iks - Exception {ex}");
                    throw;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"IKSRequest Error : {exception}");
                var correlationId = Guid.NewGuid();
                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = ParameterCode,
                    Type = operationType.ToString(),
                    Date = DateTime.Now,
                    Request = requestString,
                    Response = responseString,
                    ErrorMessage = exception.ToString(),
                    DataType = IntegrationLogDataType.Text
                };
                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
            }

            return response;
        }
        
        private string BuildQueryString(object obj)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var sb = new StringBuilder();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                if (value is string str)
                {
                    if (!string.IsNullOrWhiteSpace(str))
                        Append(sb, prop.Name, str);
                }
                else if (value is DateTime dt)
                {
                    Append(sb, prop.Name, dt.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else if (value != null)
                {
                    Append(sb, prop.Name, value.ToString()!);
                }
            }

            return sb.ToString().TrimStart('&');
        }

        private void Append(StringBuilder sb, string name, string value)
        {
            if (sb.Length > 0) sb.Append('&');
            sb.Append(Uri.EscapeDataString(name))
                .Append('=')
                .Append(Uri.EscapeDataString(value));
        }
        
        private async Task<IKSTokenResponse> GetToken()
        {
            string requestString = string.Empty;
            string responseString = string.Empty;
            try
            {
                _client = CreatePfxFile();

                var values = new Dictionary<string, string>
                {
                    { "grant_type", _settings.Grant_Type },
                    { "scope", _settings.Scope},
                    { "client_secret", _settings.Client_Secret },
                    { "client_id", _settings.Client_ID }
                };
                requestString = JsonSerializer.Serialize(values);
                var content = new FormUrlEncodedContent(values);
                var response = await _client.PostAsync(_settings.APIEndPoint + _settings.TokenEndPoint, content);
                responseString = await response.Content.ReadAsStringAsync();

                try
                {
                    bool isLogEnable;
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
                            Type = "GetToken",
                            Date = DateTime.Now,
                            Request = requestString,
                            Response = responseString,
                            DataType = IntegrationLogDataType.Text
                        };
                        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                        await endpoint.Send(log, cancellationToken.Token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrationLog Error: Iks - Exception {ex}");
                }

                return JsonSerializer.Deserialize<IKSTokenResponse>(responseString);
            }
            catch (Exception exception)
            {
                _logger.LogError($"GetToken Error : {exception}");
                var correlationId = Guid.NewGuid();
                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = ParameterCode,
                    Type = "GetToken",
                    Date = DateTime.Now,
                    Request = requestString,
                    ErrorMessage = exception.ToString(),
                    DataType = IntegrationLogDataType.Text
                };
                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
                throw;
            }
        }
        
        private HttpClient CreatePfxFile()
        {
            try
            {

                X509Certificate2 pfxCertWithKey = new X509Certificate2(certificatePath, _settings.CsrFilePassword);

                HttpClientHandler handler = new HttpClientHandler();
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => { return true; };
                handler.ClientCertificates.Add(pfxCertWithKey);
                _client = new HttpClient(handler);

                return _client;
            }
            catch (Exception exception)
            {
                _logger.LogError($"CreatePfxFile Error : {exception}");
                throw;
            }
        }

        #endregion
    }
}
