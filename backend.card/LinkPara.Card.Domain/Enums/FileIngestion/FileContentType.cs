using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.FileIngestion;

public enum FileContentType
{
    [Description("File content in BKM format.")]
    Bkm = 1,
    [Description("File content in Mastercard (MSC) format.")]
    Msc = 2,
    [Description("File content in VISA format.")]
    Visa = 3
}
