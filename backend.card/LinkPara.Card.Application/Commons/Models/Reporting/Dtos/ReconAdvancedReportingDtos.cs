using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Domain.Enums.Reporting;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Dtos;

public class FileReconSummaryDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; }
    public FileType FileType { get; set; }
    public FileContentType ContentType { get; set; }
    public FileStatus FileStatus { get; set; }
    public DateTime FileCreatedAt { get; set; }
    public DataScope DataScope { get; set; }

    public long TotalLineCount { get; set; }
    public long MatchedLineCount { get; set; }
    public long UnmatchedLineCount { get; set; }
    public decimal MatchRatePct { get; set; }

    public decimal? TotalOriginalAmount { get; set; }
    public decimal? MatchedAmount { get; set; }
    public decimal? UnmatchedAmount { get; set; }
    public decimal? TotalSettlementAmount { get; set; }

    public long ReconReadyCount { get; set; }
    public long ReconSuccessCount { get; set; }
    public long ReconFailedCount { get; set; }
    public long ReconNotApplicableCount { get; set; }
}

public class ReconMatchRateTrendDto
{
    public DateTime ReportDate { get; set; }
    public DataScope DataScope { get; set; }
    public FileContentType Network { get; set; }
    public ReconSide Side { get; set; }
    public long TotalLineCount { get; set; }
    public long MatchedCount { get; set; }
    public long UnmatchedCount { get; set; }
    public decimal MatchRatePct { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? MatchedAmount { get; set; }
    public decimal? UnmatchedAmount { get; set; }
}

public class ReconGapAnalysisDto
{
    public DateTime ReportDate { get; set; }
    public DataScope DataScope { get; set; }
    public FileContentType Network { get; set; }

    public long CardLineCount { get; set; }
    public long ClearingLineCount { get; set; }
    public long LineCountDifference { get; set; }

    public long CardMatchedCount { get; set; }
    public long ClearingMatchedCount { get; set; }

    public decimal? CardTotalAmount { get; set; }
    public decimal? ClearingTotalAmount { get; set; }
    public decimal? AmountDifference { get; set; }

    public decimal CardMatchRatePct { get; set; }
    public decimal ClearingMatchRatePct { get; set; }
}

public class UnmatchedTransactionAgingDto
{
    public string AgeBucket { get; set; }
    public DataScope DataScope { get; set; }
    public FileContentType Network { get; set; }
    public ReconSide Side { get; set; }
    public long UnmatchedCount { get; set; }
    public decimal? UnmatchedAmount { get; set; }
    public decimal PctOfTotalUnmatched { get; set; }
}

public class NetworkReconScorecardDto
{
    public DataScope DataScope { get; set; }
    public FileContentType Network { get; set; }

    public long TotalFileCount { get; set; }
    public long TotalCardLineCount { get; set; }
    public long TotalClearingLineCount { get; set; }
    public long TotalMatchedCount { get; set; }
    public long TotalUnmatchedCount { get; set; }
    public decimal OverallMatchRatePct { get; set; }

    public decimal? TotalCardAmount { get; set; }
    public decimal? TotalClearingAmount { get; set; }
    public decimal? NetAmountDifference { get; set; }

    public decimal? AvgCardOriginalAmount { get; set; }
    public decimal? AvgClearingSourceAmount { get; set; }

    public long ReconSuccessLineCount { get; set; }
    public long ReconFailedLineCount { get; set; }
    public long ReconPendingLineCount { get; set; }
    public decimal ReconSuccessRatePct { get; set; }

    public DateTime? FirstFileDate { get; set; }
    public DateTime? LastFileDate { get; set; }
}

