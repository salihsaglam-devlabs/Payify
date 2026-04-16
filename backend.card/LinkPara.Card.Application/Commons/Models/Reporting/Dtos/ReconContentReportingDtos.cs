using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Domain.Enums.Reporting;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Dtos;

public class ReconCardContentDailyDto
{
    public DateTime ReportDate { get; set; }
    public DataScope DataScope { get; set; }
    public FileContentType Network { get; set; }
    public FileRowStatus LineStatus { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
    public string FinancialType { get; set; }
    public string TxnEffect { get; set; }
    public string TxnSource { get; set; }
    public string TxnRegion { get; set; }
    public string TerminalType { get; set; }
    public string ChannelCode { get; set; }
    public string IsTxnSettle { get; set; }
    public string TxnStat { get; set; }
    public string ResponseCode { get; set; }
    public string IsSuccessfulTxn { get; set; }
    public int OriginalCurrency { get; set; }
    public long TransactionCount { get; set; }
    public long DistinctFileCount { get; set; }
    public decimal? TotalCardOriginalAmount { get; set; }
    public decimal? TotalCardSettlementAmount { get; set; }
    public decimal? TotalCardBillingAmount { get; set; }
    public decimal? AvgCardOriginalAmount { get; set; }
    public long MatchedCount { get; set; }
    public long UnmatchedCount { get; set; }
}

public class ReconClearingContentDailyDto
{
    public DateTime ReportDate { get; set; }
    public DataScope DataScope { get; set; }
    public FileContentType Network { get; set; }
    public FileRowStatus LineStatus { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
    public string TxnType { get; set; }
    public string IoFlag { get; set; }
    public string ControlStat { get; set; }
    public int SourceCurrency { get; set; }
    public long TransactionCount { get; set; }
    public long DistinctFileCount { get; set; }
    public decimal? TotalClearingSourceAmount { get; set; }
    public decimal? TotalClearingDestinationAmount { get; set; }
    public decimal? AvgClearingSourceAmount { get; set; }
    public long MatchedCount { get; set; }
    public long UnmatchedCount { get; set; }
}

public class ReconContentDailyDto
{
    public DateTime ReportDate { get; set; }
    public DataScope DataScope { get; set; }
    public FileContentType Network { get; set; }
    public ReconSide Side { get; set; }
    public FileRowStatus LineStatus { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
    public long TransactionCount { get; set; }
    public long DistinctFileCount { get; set; }
    public long MatchedCount { get; set; }
    public long UnmatchedCount { get; set; }
    public decimal? TotalCardOriginalAmount { get; set; }
    public decimal? TotalCardSettlementAmount { get; set; }
    public decimal? TotalCardBillingAmount { get; set; }
    public decimal? TotalClearingSourceAmount { get; set; }
    public decimal? TotalClearingDestinationAmount { get; set; }
}

public class ReconClearingControlStatAnalysisDto
{
    public DataScope DataScope { get; set; }
    public FileContentType Network { get; set; }
    public FileRowStatus LineStatus { get; set; }
    public string ControlStat { get; set; }
    public string IoFlag { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalClearingSourceAmount { get; set; }
    public long MatchedCount { get; set; }
    public long UnmatchedCount { get; set; }
    public decimal UnmatchedRatePct { get; set; }
}

public class ReconFinancialSummaryDto
{
    public DataScope DataScope { get; set; }
    public FileContentType Network { get; set; }
    public FileRowStatus LineStatus { get; set; }
    public string FinancialType { get; set; }
    public string TxnEffect { get; set; }
    public int OriginalCurrency { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalCardOriginalAmount { get; set; }
    public decimal? TotalCardSettlementAmount { get; set; }
    public decimal? TotalCardBillingAmount { get; set; }
    public long SettledCount { get; set; }
    public long UnsettledCount { get; set; }
    public decimal? DebitAmount { get; set; }
    public decimal? CreditAmount { get; set; }
    public long MatchedCount { get; set; }
    public long UnmatchedCount { get; set; }
}

public class ReconResponseStatusAnalysisDto
{
    public DataScope DataScope { get; set; }
    public FileContentType Network { get; set; }
    public FileRowStatus LineStatus { get; set; }
    public string ResponseCode { get; set; }
    public string TxnStat { get; set; }
    public string IsSuccessfulTxn { get; set; }
    public string IsTxnSettle { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalCardOriginalAmount { get; set; }
    public long MatchedCount { get; set; }
    public long UnmatchedCount { get; set; }
}
