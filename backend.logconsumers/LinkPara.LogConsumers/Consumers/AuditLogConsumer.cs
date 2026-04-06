using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.LogConsumers.Commons.Entities;
using LinkPara.LogConsumers.Commons.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using sharedModel = LinkPara.Audit.Models;

namespace LinkPara.LogConsumers.Consumers;

public class AuditLogConsumer : IConsumer<sharedModel.AuditLog>
{
    private const string GroupCode = "LogConsumerParameters";
    private const string ParameterCode = "SaveAuditLogToDb";

    private readonly IGenericRepository<AuditLog> _auditLogRepository;
    private readonly IElasticSearchService _elasticSearchService;
    private readonly IParameterService _parameterService;
    private readonly ILogger<AuditLogConsumer> _logger;

    public AuditLogConsumer(IGenericRepository<AuditLog> auditLogRepository,
        IElasticSearchService elasticSearchService,
        IParameterService parameterService,
        ILogger<AuditLogConsumer> logger)
    {
        _auditLogRepository = auditLogRepository;
        _elasticSearchService = elasticSearchService;
        _parameterService = parameterService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<sharedModel.AuditLog> context)
    {
        try
        {
            var parameter = await _parameterService.GetParameterAsync(GroupCode, ParameterCode);

            _ = bool.TryParse(parameter.ParameterValue, out var saveAuditLogToDb);

            if (saveAuditLogToDb)
            {
                var auditLog = context.Message;

                await _auditLogRepository.AddAsync(
                    new AuditLog
                        {
                            Details = auditLog.Details,
                            IsSuccess = auditLog.IsSuccess,
                            LogDate = auditLog.LogDate,
                            Operation = auditLog.Operation,
                            Resource = auditLog.Resource,
                            SourceApplication = auditLog.SourceApplication,
                            UserId = auditLog.UserId,
                            UserName = auditLog.UserName,
                            CreatedBy = auditLog.UserId.ToString(),
                            Channel = auditLog.Channel,
                            ClientIpAddress = auditLog.ClientIpAddress,
                            CorrelationId = auditLog.CorrelationId
                    }
                    );
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("AuditLogConsumer - Save To Database Error : {Exception}", exception);
        }

        var indexName = $"audit-log-{DateTime.Now.ToString("yyyy.MM.dd")}";

        await _elasticSearchService.
                InsertDocumentAsync(indexName, context.Message);
    }
}
