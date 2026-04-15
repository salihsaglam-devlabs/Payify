using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reporting;

public enum ReportingNetwork
{
    [Description("BKM network.")]
    BKM = 1,

    [Description("VISA network.")]
    VISA = 2,

    [Description("Mastercard (MSC) network.")]
    MSC = 3
}

