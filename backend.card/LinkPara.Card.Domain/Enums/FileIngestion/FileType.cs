using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.FileIngestion;

public enum FileType
{
    [Description("Card transaction file.")]
    Card = 1,
    [Description("Clearing/reconciliation file.")]
    Clearing = 2
}
