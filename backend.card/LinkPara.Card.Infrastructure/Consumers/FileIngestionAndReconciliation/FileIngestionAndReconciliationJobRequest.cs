using System.Text.Json.Serialization;
using LinkPara.Card.Application.Commons.Helpers.Shared;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;

namespace LinkPara.Card.Infrastructure.Consumers.FileIngestionAndReconciliation;

public class FileIngestionAndReconciliationJobRequest
{
    [JsonConverter(typeof(FlexibleEnumJsonConverter<FileIngestionAndReconciliationJobType>))]
    public FileIngestionAndReconciliationJobType Type { get; set; }
    public string? InitiatedByUserId { get; init; }
    public FileIngestionRequest? IngestionRequest { get; init; }
    public EvaluateRequest? EvaluateRequest { get; init; }
    public ExecuteRequest? ExecuteRequest { get; init; }
}
