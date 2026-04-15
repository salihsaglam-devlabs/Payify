using LinkPara.Card.Domain.Enums.Reporting;

namespace LinkPara.Card.Application.Commons.Helpers.Reporting;

public static class ReportingEnumMapper
{
    public static string? ToDbValue(ReportingMatchStatus? status)
    {
        return status switch
        {
            ReportingMatchStatus.Matched => "MATCHED",
            ReportingMatchStatus.UnmatchedCard => "UNMATCHED_CARD",
            _ => null
        };
    }

    public static string? ToDbValue(ReportingNetwork? network)
    {
        return network switch
        {
            ReportingNetwork.BKM => "BKM",
            ReportingNetwork.VISA => "VISA",
            ReportingNetwork.MSC => "MSC",
            _ => null
        };
    }

    public static string? ToDbValue(ReportingDuplicateStatus? status)
    {
        return status switch
        {
            ReportingDuplicateStatus.Unique => "Unique",
            ReportingDuplicateStatus.Primary => "Primary",
            ReportingDuplicateStatus.Secondary => "Secondary",
            ReportingDuplicateStatus.Conflict => "Conflict",
            _ => null
        };
    }
}

