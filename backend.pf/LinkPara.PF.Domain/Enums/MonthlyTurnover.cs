using System.ComponentModel;

namespace LinkPara.PF.Domain.Enums;

public enum MonthlyTurnover
{
    Undefined,
    
    [Description("0-10k")]
    Range_0_10k,

    [Description("10k-50k")]
    Range_10k_50k,

    [Description("50k-100k")]
    Range_50k_100k,

    [Description("100k+")]
    Range_100k_Plus
}