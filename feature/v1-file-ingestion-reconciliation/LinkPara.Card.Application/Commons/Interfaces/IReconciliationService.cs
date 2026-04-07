using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IReconciliationService
{
    Task<ReconciliationProcessSummary> ProcessImportedFileAsync(
        Guid importedFileId,
        string actor,
        ReconciliationSummaryOptions options = null,
        CancellationToken cancellationToken = default);

    Task<ReconciliationProcessSummary> RegenerateOperationsAsync(
        string actor,
        int take = 1000,
        Guid? importedFileId = null,
        int? lookbackDays = null,
        ReconciliationSummaryOptions options = null,
        CancellationToken cancellationToken = default);
}
