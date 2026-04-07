using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Infrastructure.Consumers.FileIngestionAndReconciliation;

public class FileIngestionAndReconciliationJobResponse
{
    public FileIngestionAndReconciliationJobType Type { get; set; }

    public bool IsSuccess { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public List<FileIngestionResponse> IngestionResponses { get; set; } = new();

    public EvaluateResponse? EvaluateResponse { get; set; }

    public ExecuteResponse? ExecuteResponse { get; set; }
}