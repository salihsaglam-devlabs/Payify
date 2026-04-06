using AutoMapper;
using LinkPara.Cache;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Request;
using LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Response;
using LinkPara.Fraud.Application.Features.Transactions;
using LinkPara.Fraud.Application.Features.Transactions.Queries.GetAllTransactions;
using LinkPara.Fraud.Domain.Entities;
using LinkPara.Fraud.Domain.Enums;
using LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using Serilog;
using MassTransit;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LinkPara.Fraud.Infrastructure.Services;

public class TransactionService : SanctionScannerBase, ITransactionService
{
    private readonly ILogger<TransactionService> _logger;
    private readonly IGenericRepository<TransactionMonitoring> _repository;
    private readonly IGenericRepository<TriggeredRule> _triggeredRuleRepository;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<IntegrationLog> _logRepository;
    private readonly ICacheService _cacheService;
    private readonly IGenericRepository<TriggeredRuleSetKey> _ruleSetKeyRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;
    private readonly string username;
    private readonly string password;
    private readonly string complianceUsername;
    private readonly string compliancePassword;
    private readonly string transactionBaseAddress;
    private readonly bool complianceMonitoringEnabled;
    private const string ruleSetCacheKey = "ruleSetKeys";

    public TransactionService(
        ILogger<TransactionService> logger,
        IGenericRepository<TransactionMonitoring> repository,
        IGenericRepository<TriggeredRule> triggeredRuleRepository,
        IMapper mapper,
        IGenericRepository<IntegrationLog> logRepository,
        IVaultClient vaultClient,
        ICacheService cacheService,
        IGenericRepository<TriggeredRuleSetKey> ruleSetKeyRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IParameterService parameterService,
        IBus bus)
        : base()
    {
        _logger = logger;
        _repository = repository;
        _triggeredRuleRepository = triggeredRuleRepository;
        _mapper = mapper;
        _logRepository = logRepository;
        _cacheService = cacheService;
        _ruleSetKeyRepository = ruleSetKeyRepository;
        username = vaultClient.GetSecretValue<string>("FraudSecrets", "SanctionScannerCredentials", "TransactionUsername");
        password = vaultClient.GetSecretValue<string>("FraudSecrets", "SanctionScannerCredentials", "TransactionPassword");
        complianceUsername = vaultClient.GetSecretValue<string>("FraudSecrets", "SanctionScannerCredentials", "ComplianceUsername");
        compliancePassword = vaultClient.GetSecretValue<string>("FraudSecrets", "SanctionScannerCredentials", "CompliancePassword");
        transactionBaseAddress = vaultClient.GetSecretValue<string>("FraudSecrets", "SanctionScannerCredentials", "TransactionBaseAddress");
        complianceMonitoringEnabled = vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "ComplianceMonitoringEnabled");
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _parameterService = parameterService;
        _bus = bus;
    }

    public async Task<CancelTransactionResponse> CancelTransactionAsync(string transactionId)
    {
        try
        {
            var response = await PostAsync($"{transactionBaseAddress}{SanctionScannerInfo.CancelTransaction}?TransactionId={transactionId}", string.Empty, username, password);

            var responseContent = await response.Content.ReadAsStringAsync();
            var transactionResponse = JsonConvert.DeserializeObject<CancelTransactionResponse>(responseContent);

            return transactionResponse;
        }
        catch (Exception exception)
        {
            _logger.LogError($"CancelTransactionError : {exception}");
            throw;
        }
    }

    public async Task<TransactionResponse> ExecuteTransactionAsync(FraudCheckRequest request)
    {
        var transactionResponse = new TransactionResponse { IsSuccess = false };
        try
        {
            var transactionApiRequest = await ConvertTransactionApiRequestAsync(request, false);

            var response = await PostAsync($"{transactionBaseAddress}{SanctionScannerInfo.ExecuteTransaction}", transactionApiRequest, username, password);
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<TransactionApiResponse>(responseContent);
            transactionResponse = ConvertResponse(apiResponse, transactionApiRequest.TransactionID);

            if (response.IsSuccessStatusCode)
            {
                await AddTransactionAsync(request, transactionApiRequest, transactionResponse, true, apiResponse);
            }
            else
            {
                var newApiResponse = new TransactionApiResponse
                {
                    ErrorCode = response.StatusCode.ToString(),
                    ErrorMessage = response.ReasonPhrase,
                    IsSuccess = false
                };
                await AddTransactionAsync(request, transactionApiRequest, transactionResponse, false, newApiResponse);

            }

            var complianceRiskLevel = RiskLevel.Unknown;
            if (complianceMonitoringEnabled)
            {
                var complianceTransactionApiRequest = await ConvertTransactionApiRequestAsync(request, true);
                var complianceResponse = await PostAsync($"{transactionBaseAddress}{SanctionScannerInfo.ExecuteTransaction}", complianceTransactionApiRequest, complianceUsername, compliancePassword);
                var complianceResponseContent = await complianceResponse.Content.ReadAsStringAsync();
                var complianceApiResponse = JsonConvert.DeserializeObject<TransactionApiResponse>(complianceResponseContent);
                var complianceTransactionResponse = ConvertResponse(complianceApiResponse, complianceTransactionApiRequest.TransactionID);
                complianceRiskLevel = complianceTransactionResponse.RiskLevel;

                if (complianceResponse.IsSuccessStatusCode)
                {
                    await AddTransactionAsync(request, complianceTransactionApiRequest, complianceTransactionResponse, true, complianceApiResponse);
                }
                else
                {
                    var newApiResponse = new TransactionApiResponse
                    {
                        ErrorCode = complianceResponse.StatusCode.ToString(),
                        ErrorMessage = complianceResponse.ReasonPhrase,
                        IsSuccess = false
                    };
                    await AddTransactionAsync(request, complianceTransactionApiRequest, complianceTransactionResponse, false, newApiResponse);

                }
            }                    

            if ((int)complianceRiskLevel > (int)transactionResponse.RiskLevel)
            {
                transactionResponse.RiskLevel = complianceRiskLevel;
            }

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "AddTransaction",
                SourceApplication = "Fraud",
                Resource = "TransactionMonitoring",
                Details = new Dictionary<string, string>
                {
                      {"Module",request.Module },
                      {"CommandName", request.CommandName },
                }
            });

            return transactionResponse;
        }
        catch (TaskCanceledException exception) when (exception.InnerException is TimeoutException)
        {
            _logger.LogError($"CreateExecuteTransactionTimeoutError : {exception}");
        }
        catch (TimeoutException exception)
        {
            _logger.LogError($"CreateExecuteTransactionTimeoutError : {exception}");
        }
        catch (Exception exception)
        {
            _logger.LogError($"CreateExecuteTransactionError : {exception}");
        }
        transactionResponse.ErrorMessage = "Timeout Error";
        return transactionResponse;
    }
    public async Task<PaginatedList<TransactionMonitoringDto>> GetListAsync(GetAllTransactionQuery request)
    {
        var transactionList = _repository.GetAll().Where(c => c.MonitoringStatus != MonitoringStatus.Failed);

        if (!string.IsNullOrEmpty(request.CommandName))
        {
            transactionList = transactionList.Where(b => b.CommandName == request.CommandName);
        }
        if (!string.IsNullOrEmpty(request.Module))
        {
            transactionList = transactionList.Where(b => b.Module.ToLower().Contains(request.Module.ToLower()));
        }
        if (!string.IsNullOrEmpty(request.SenderNumber))
        {
            transactionList = transactionList.Where(b => b.SenderNumber.Contains(request.SenderNumber));
        }
        if (!string.IsNullOrEmpty(request.ReceiverNumber))
        {
            transactionList = transactionList.Where(b => b.ReceiverNumber.Contains(request.ReceiverNumber));
        }
        if (request.TransactionDateStart is not null)
        {
            transactionList = transactionList.Where(b => b.TransactionDate
                               >= request.TransactionDateStart);
        }
        if (request.TransactionDateEnd is not null)
        {
            transactionList = transactionList.Where(b => b.TransactionDate
                                                         <= request.TransactionDateEnd);
        }
        if (request.RiskLevel is not null)
        {
            transactionList = transactionList.Where(b => b.RiskLevel
                               == request.RiskLevel);
        }
        if (request.MonitoringStatus is not null)
        {
            transactionList = transactionList.Where(b => b.MonitoringStatus
                               == request.MonitoringStatus);
        }
        if (!string.IsNullOrEmpty(request.SenderName))
        {
            transactionList = transactionList.Where(b => b.SenderName.ToLower()
                                             .Contains(request.SenderName.ToLower()));
        }
        if (!string.IsNullOrEmpty(request.ReceiverName))
        {
            transactionList = transactionList.Where(b => b.ReceiverName.ToLower()
                                             .Contains(request.ReceiverName.ToLower()));
        }
        return await transactionList
            .PaginatedListWithMappingAsync<TransactionMonitoring, TransactionMonitoringDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<TransactionApiResponse> GetTransactionAsync(string transactionId)
    {
        try
        {
            var response = await GetAsync($"{transactionBaseAddress}{SanctionScannerInfo.TransactionExists}?TransactionId={transactionId}", username, password);

            var transactionResponse = JsonConvert.DeserializeObject<TransactionApiResponse>(response);

            return transactionResponse;
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetTransactionExistsError : {exception}");
            throw;
        }
    }

    public async Task<TransactionDetailResponse> GetTransactionDetailAsync(string transactionId)
    {
        try
        {
            var response = await PostAsync($"{transactionBaseAddress}{SanctionScannerInfo.TransactionDetail}?TransactionId={transactionId}", string.Empty, username, password);
            var responseContent = await response.Content.ReadAsStringAsync();
            var transactionResponse = JsonConvert.DeserializeObject<TransactionDetailResponse>(responseContent);

            return transactionResponse;
        }
        catch (Exception exception)
        {
            _logger.LogError($"CreateExecuteTransactionError : {exception}");
            throw;
        }
    }

    private TransactionResponse ConvertResponse(TransactionApiResponse response, string transactionId)
    {
        var newResponse = new TransactionResponse();
        newResponse.TransactionId = transactionId;

        if (response is not null)
        {
            newResponse.IsSuccess = response.IsSuccess;
            newResponse.ErrorMessage = response.ErrorMessage;
            newResponse.ErrorCode = response.ErrorCode;
            newResponse.HttpStatusCode = response.HttpStatusCode;
        }
        else
        {
            newResponse.IsSuccess = false;
        }

        newResponse.RiskLevel = response?.Result?.TriggeredAlarm switch
        {
            "Very Low" => RiskLevel.VeryLow,
            "Low" => RiskLevel.Low,
            "Medium" => RiskLevel.Medium,
            "High" => RiskLevel.High,
            "Critical" => RiskLevel.Critical,
            _ => RiskLevel.Unknown,
        };
        return newResponse;
    }

    private async Task<ExecuteTransactionApiRequest> ConvertTransactionApiRequestAsync(FraudCheckRequest request, bool isCompliance)
    {
        var userLevel = request.AccountKycLevel switch
        {
            AccountKycLevel.Premium => "Kyc",
            AccountKycLevel.Kyc => "Kyc",
            AccountKycLevel.NoneKyc => "NoneKyc",
            _ => "NoneKyc",
        };

        var ruleSetKey = await GetTriggeredRuleSetKey(request.CommandName, userLevel, isCompliance);

        var ipAddress = !string.IsNullOrWhiteSpace(request.ClientIpAddress) && request.ClientIpAddress.ToLower() != "null"
                           ? request.ClientIpAddress
                           : _contextProvider.CurrentContext.ClientIpAddress;

        var transactionApiRequest = new ExecuteTransactionApiRequest
        {
            Amount = request.ExecuteTransaction.Amount,
            AmountCurrencyCode = request.ExecuteTransaction.AmountCurrencyCode,
            BeneficiaryAccountCurrencyCode = request.ExecuteTransaction.BeneficiaryAccountCurrencyCode,
            BeneficiaryBankID = request.ExecuteTransaction.BeneficiaryBankID,
            OriginatorAccountCurrencyCode = request.ExecuteTransaction.OriginatorAccountCurrencyCode,
            BeneficiaryNumber = request.ExecuteTransaction.BeneficiaryNumber,
            Beneficiary = request.ExecuteTransaction.Beneficiary,
            OriginatorNumber = request.ExecuteTransaction.OriginatorNumber,
            Originator = request.ExecuteTransaction.Originator,
            OriginatorBankID = request.ExecuteTransaction.OriginatorBankID,
            MccCode = request.ExecuteTransaction.MccCode ?? 0,
            Narrative = request.ExecuteTransaction.TransactionType,
            TransactionDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Originator​Type​Id = 1,
            TransactionID = Guid.NewGuid().ToString(),
            TransactionIPAddress = ipAddress,
            TriggeredRuleSetKey = ruleSetKey
        };
        transactionApiRequest.Channel = request.ExecuteTransaction.Channel switch
        {
            "Backoffice" => 1,
            "Web" => 1,
            "Mobile" => 2,
            _ => 1
        };
        transactionApiRequest.Direction = request.ExecuteTransaction.Direction switch
        {
            Direction.Inbound => 1,
            Direction.Outbound => 2,
            _ => 1,
        };
        transactionApiRequest.Source = request.ExecuteTransaction.FraudSource switch
        {
            FraudSource.P2PTransfer => 6,
            FraudSource.Remittance => 2,
            FraudSource.EFT => 1,
            FraudSource.Pos => 5,
            FraudSource.Fast => 6,
            FraudSource.Wallet => 7,
            FraudSource.MoneyTransfer => 6,
            FraudSource.Invoice => 9,
            FraudSource.Card => 8,
            _ => 6,
        };
        return transactionApiRequest;
    }

    private async Task AddTransactionAsync(FraudCheckRequest request, ExecuteTransactionApiRequest apiRequest, TransactionResponse transactionResponse, bool isSuccess, TransactionApiResponse apiResponse)
    {
        var monitoringStatus = MonitoringStatus.Failed;

        if (apiResponse.IsSuccess)
        {
            var parameter = await _parameterService
                .GetParameterAsync("FraudParameters", "RiskLevel");

            var riskLevel = Convert.ToInt32(parameter.ParameterValue);

            monitoringStatus =
                (int)transactionResponse.RiskLevel >= riskLevel
                    ? MonitoringStatus.Pending
                    : MonitoringStatus.Completed;
        }

        var ipAddress = !string.IsNullOrWhiteSpace(request.ClientIpAddress) && request.ClientIpAddress.ToLower() != "null"
                          ? request.ClientIpAddress
                          : _contextProvider.CurrentContext.ClientIpAddress;
        var totalScore = 0;

        if (apiResponse.Result?.TotalScore != null)
        {
            totalScore = Convert.ToInt32(apiResponse.Result?.TotalScore);
        }

        var transactionMonitoring = new TransactionMonitoring
        {
            Module = request.Module,
            CommandName = request.CommandName,
            Amount = apiRequest.Amount,
            ReceiverNumber = apiRequest.BeneficiaryNumber,
            ReceiverName = apiRequest.Beneficiary,
            SenderNumber = apiRequest.OriginatorNumber,
            SenderName = apiRequest.Originator,
            TransferRequest = JsonConvert.SerializeObject(apiRequest),
            CommandJson = request.CommandJson,
            MonitoringStatus = monitoringStatus,
            RiskLevel = transactionResponse.RiskLevel,
            TransactionId = apiRequest.TransactionID,
            TotalScore = totalScore,
            TransactionDate = DateTime.Now,
            ErrorCode = apiResponse.ErrorCode,
            ErrorMessage = apiResponse.ErrorMessage,
            CreatedBy = request.UserId,
            ClientIpAddress = ipAddress,
        };

        transactionMonitoring.CurrencyCode = apiRequest.AmountCurrencyCode switch
        {
            949 => "TRY",
            840 => "USD",
            _ => "TRY",
        };

        await _repository.AddAsync(transactionMonitoring);

        if (apiResponse.Result is not null && apiResponse.Result.TriggeredRulesList.Any())
        {
            var ruleList = apiResponse.Result.TriggeredRulesList.Select(b => new TriggeredRule
            {
                TransactionMonitoringId = transactionMonitoring.Id,
                RuleKey = b.RuleKey,
                Score = Convert.ToInt32(b.Score),
                CreatedBy = request.UserId
            }).ToList();

            await _triggeredRuleRepository.AddRangeAsync(ruleList);
        }

        var integrationLog = new IntegrationLog
        {
            TransactionMonitoringId = transactionMonitoring.Id,
            Request = JsonConvert.SerializeObject(request),
            Response = JsonConvert.SerializeObject(apiResponse),
            IsSuccess = isSuccess,
            CreatedBy = request.UserId
        };

        await _logRepository.AddAsync(integrationLog);

        var correlationId = Guid.NewGuid;
        var log = new LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.IntegrationLog()
        {
            CorrelationId = correlationId.ToString(),
            Name = "Fraud.Transaction",
            Type = nameof(IntegrationLogType.Fraud),
            Date = DateTime.Now,
            Request = JsonConvert.SerializeObject(apiRequest),
            Response = JsonConvert.SerializeObject(apiResponse),
            DataType = IntegrationLogDataType.Json
        };

        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
        await endpoint.Send(log, cancellationToken.Token);
    }

    public async Task<string> GetTriggeredRuleSetKey(string operation, string userLevel, bool isCompliance)
    {
        var ruleSetKey = await _cacheService.GetOrCreateAsync(ruleSetCacheKey,
          async () =>
          {
              return await _ruleSetKeyRepository
              .GetAll().ToListAsync();
          }, 60);

        if (isCompliance)
        {
            return ruleSetKey?.FirstOrDefault(b => b.Operation.Equals(operation) && b.Level.Equals(userLevel))?.ComplianceRuleSetKey;
        }

        return ruleSetKey?.FirstOrDefault(b => b.Operation.Equals(operation) && b.Level.Equals(userLevel))?.RuleSetKey;
    }
}
