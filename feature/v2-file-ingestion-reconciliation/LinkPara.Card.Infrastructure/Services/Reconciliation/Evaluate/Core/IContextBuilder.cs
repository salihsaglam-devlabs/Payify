using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Entities.FileIngestion;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;

internal interface IContextBuilder
{
    Task<IReadOnlyDictionary<Guid, EvaluationContext>> BuildManyAsync(
        IReadOnlyList<IngestionFileLine> rows,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default);
}
