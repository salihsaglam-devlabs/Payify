using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;

internal interface IContextBuilder
{
    Task<IReadOnlyDictionary<Guid, EvaluationContext>> BuildManyAsync(
        IReadOnlyList<IngestionFileLine> rows,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default);
}
