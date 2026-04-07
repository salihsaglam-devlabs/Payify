using LinkPara.Card.Application.Commons.Models.Logging;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IBulkServiceOperationLogPublisher
{
    Task PublishAsync(BulkServiceOperationLog log, CancellationToken cancellationToken = default);
}
