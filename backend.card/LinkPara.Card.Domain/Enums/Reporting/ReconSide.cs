using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reporting;

public enum ReconSide
{
    [Description("Card transaction side.")]
    Card = 1,

    [Description("Clearing transaction side.")]
    Clearing = 2
}

