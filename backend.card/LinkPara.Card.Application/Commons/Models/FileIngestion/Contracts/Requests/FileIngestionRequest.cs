using System.Text.Json.Serialization;
using LinkPara.Card.Application.Commons.Helpers.Shared;
using LinkPara.Card.Domain.Enums.FileIngestion;

namespace LinkPara.Card.Application.Commons.Models.FileIngestion.Contracts.Requests;

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
