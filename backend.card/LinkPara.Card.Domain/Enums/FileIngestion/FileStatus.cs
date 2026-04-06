using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.FileIngestion;

public enum FileStatus
{
    [Description("File is currently being processed.")]
    Processing = 1,

    [Description("File processing failed.")]
    Failed = 2,

    [Description("File processed successfully.")]
    Success = 3
}