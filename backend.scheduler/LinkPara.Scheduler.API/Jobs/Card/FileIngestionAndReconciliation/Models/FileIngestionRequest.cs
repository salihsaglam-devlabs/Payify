using System.Text.Json.Serialization;
using LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Converters;
using LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Enums;

namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Models;

public class FileIngestionRequest
{
    [JsonConverter(typeof(FlexibleEnumJsonConverter<FileSourceType>))]
    public FileSourceType FileSourceType { get; set; }

    [JsonConverter(typeof(FlexibleEnumJsonConverter<FileType>))]
    public FileType FileType { get; set; }

    [JsonConverter(typeof(FlexibleEnumJsonConverter<FileContentType>))]
    public FileContentType FileContentType { get; set; }

    public string FilePath { get; set; }
}