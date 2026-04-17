using System.Text.Json.Serialization;
using LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Converters;
using LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Enums;

namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Models;

public class FileIngestionAndReconciliationJobRequest
{
    [JsonConverter(typeof(FlexibleEnumJsonConverter<JobRequestType>))]
    public JobRequestType Type { get; set; }

    public string InitiatedByUserId { get; init; }

    public FileIngestionRequest IngestionRequest { get; init; }

    public EvaluateRequest EvaluateRequest { get; init; }

    public ExecuteRequest ExecuteRequest { get; init; }
}