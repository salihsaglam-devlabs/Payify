using LinkPara.Audit.Models;
using LinkPara.LogConsumers.Commons.Entities;
using LinkPara.LogConsumers.Commons.Interfaces;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;

namespace LinkPara.LogConsumers.Consumers
{
    public class ChangeTrackerConsumer : IConsumer<EntityChangeLogModel>
    {
        private readonly IGenericRepository<EntityChangeLog> _entityChangeRepository;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IElasticSearchService _elasticSearchService;

        public ChangeTrackerConsumer(IGenericRepository<EntityChangeLog> entityChangeRepository,
            IApplicationUserService applicationUserService,
            IElasticSearchService elasticSearchService)
        {
            _entityChangeRepository = entityChangeRepository;
            _applicationUserService = applicationUserService;
            _elasticSearchService = elasticSearchService;
        }

        public async Task Consume(ConsumeContext<EntityChangeLogModel> context)
        {
            var entityChange = context.Message;

            if (entityChange == null)
            {
                return;
            }

            await _entityChangeRepository.AddAsync(
                new EntityChangeLog
                {
                    SchemaName = entityChange.ShemaName,
                    TableName = entityChange.TableName,
                    CrudOperationType = entityChange.CrudOperationType,
                    UserId = !string.IsNullOrWhiteSpace(entityChange.UserId) 
                        ? entityChange.UserId 
                        : _applicationUserService.ApplicationUserId.ToString(),
                    ClientIpAddress = entityChange.ClientIpAddress,
                    KeyValues = entityChange.KeyValues,
                    OldValues = entityChange.OldValues,
                    NewValues = entityChange.NewValues,
                    AffectedColumns = entityChange.AffectedColumns,
                    ServiceName = entityChange.ServiceName,
                    CreatedBy = !string.IsNullOrWhiteSpace(entityChange.UserId) 
                        ? entityChange.UserId 
                        : _applicationUserService.ApplicationUserId.ToString(),
                    CorrelationId = entityChange.CorrelationId
                }
                );

            var indexName = $"entitychange-log-{DateTime.Now.ToString("yyyy.MM.dd")}";

            await _elasticSearchService.
                    InsertDocumentAsync(indexName, context.Message);
        }

    }
}
