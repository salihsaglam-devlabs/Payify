using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.FileIngestion;

public enum FileRowStatus
{
    [Description("Row is currently being processed.")]
    Processing = 1,

    [Description("Row processing failed.")]
    Failed = 2,

    [Description("Row processed successfully.")]
    Success = 3
}