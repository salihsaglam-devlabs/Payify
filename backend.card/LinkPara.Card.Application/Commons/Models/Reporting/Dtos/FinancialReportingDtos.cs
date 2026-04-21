using LinkPara.Card.Domain.Enums.Reporting;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Dtos;

public class DailyTransactionVolumeDto
{
    public DataScope DataScope { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string Network { get; set; }
    public string Currency { get; set; }
    public string FinancialType { get; set; }
    public string TxnEffect { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? DebitAmount { get; set; }
    public decimal? CreditAmount { get; set; }
    public decimal? NetFlowAmount { get; set; }
    public decimal? AvgAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string VolumeFlag { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

public class MccRevenueConcentrationDto
{
    public DataScope DataScope { get; set; }
    public string Network { get; set; }
    public string Mcc { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? TotalTaxAmount { get; set; }
    public decimal? TotalSurchargeAmount { get; set; }
    public decimal? TotalCashbackAmount { get; set; }
    public decimal? VolumeSharePct { get; set; }
    public string ConcentrationRisk { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

public class MerchantRiskHotspotDto
{
    public DataScope DataScope { get; set; }
    public string Network { get; set; }
    public string MerchantId { get; set; }
    public string MerchantName { get; set; }
    public string MerchantCountry { get; set; }
    public long TransactionCount { get; set; }
    public long DeclinedCount { get; set; }
    public long UnmatchedCount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? UnmatchedAmount { get; set; }
    public decimal DeclineRatePct { get; set; }
    public decimal UnmatchedRatePct { get; set; }
    public string RiskFlag { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

public class CountryCrossBorderExposureDto
{
    public DataScope DataScope { get; set; }
    public string Network { get; set; }
    public string MerchantCountry { get; set; }
    public string FxPattern { get; set; }
    public string OriginalCurrency { get; set; }
    public string SettlementCurrency { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalOriginalAmount { get; set; }
    public string ExposureFlag { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

public class ResponseCodeDeclineHealthDto
{
    public DataScope DataScope { get; set; }
    public string Network { get; set; }
    public string ResponseCode { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalAmount { get; set; }
    public long SuccessfulCount { get; set; }
    public long FailedCount { get; set; }
    public decimal FailureRatePct { get; set; }
    public decimal? NetworkSharePct { get; set; }
    public string HealthFlag { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

public class SettlementLagAnalysisDto
{
    public DataScope DataScope { get; set; }
    public string Network { get; set; }
    public DateTime? TransactionDate { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? AvgLagToEodDays { get; set; }
    public decimal? AvgLagToValueDays { get; set; }
    public decimal? AvgLagToIngestDays { get; set; }
    public int? MaxLagToIngestDays { get; set; }
    public long LateIngestCount { get; set; }
    public string LagHealth { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

public class CurrencyFxDriftDto
{
    public DataScope DataScope { get; set; }
    public string Network { get; set; }
    public string OriginalCurrency { get; set; }
    public string SettlementCurrency { get; set; }
    public string BillingCurrency { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalOriginalAmount { get; set; }
    public decimal? TotalSettlementAmount { get; set; }
    public decimal? TotalBillingAmount { get; set; }
    public decimal? SettlementDrift { get; set; }
    public decimal? BillingDrift { get; set; }
    public string FxDriftSeverity { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

public class InstallmentPortfolioSummaryDto
{
    public DataScope DataScope { get; set; }
    public string Network { get; set; }
    public string InstallmentBucket { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? AvgAmount { get; set; }
    public decimal? VolumeSharePct { get; set; }
    public decimal? AmountSharePct { get; set; }
    public string PortfolioFlag { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

public class LoyaltyPointsEconomyDto
{
    public DataScope DataScope { get; set; }
    public string Network { get; set; }
    public DateTime? TransactionDate { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalOriginalAmount { get; set; }
    public decimal? TotalBcPointAmount { get; set; }
    public decimal? TotalMcPointAmount { get; set; }
    public decimal? TotalCcPointAmount { get; set; }
    public decimal? TotalLoyaltyAmount { get; set; }
    public decimal LoyaltyToAmountRatioPct { get; set; }
    public string LoyaltyIntensity { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

public class ClearingDisputeSummaryDto
{
    public DataScope DataScope { get; set; }
    public string Network { get; set; }
    public string DisputeCode { get; set; }
    public string ReasonCode { get; set; }
    public string ControlStat { get; set; }
    public long TransactionCount { get; set; }
    public decimal? TotalSourceAmount { get; set; }
    public decimal? TotalReimbursementAmount { get; set; }
    public DateTime? FirstTxnDate { get; set; }
    public DateTime? LastTxnDate { get; set; }
    public string DisputeFlag { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

public class ClearingIoImbalanceDto
{
    public DataScope DataScope { get; set; }
    public string Network { get; set; }
    public DateTime? TxnDate { get; set; }
    public long TransactionCount { get; set; }
    public long IncomingCount { get; set; }
    public long OutgoingCount { get; set; }
    public decimal? IncomingAmount { get; set; }
    public decimal? OutgoingAmount { get; set; }
    public decimal? NetFlowAmount { get; set; }
    public string ImbalanceFlag { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

public class HighValueUnmatchedTransactionDto
{
    public DataScope DataScope { get; set; }
    public string Network { get; set; }
    public Guid DetailId { get; set; }
    public Guid FileLineId { get; set; }
    public DateTime? TransactionDate { get; set; }
    public decimal OriginalAmount { get; set; }
    public string Currency { get; set; }
    public string MerchantName { get; set; }
    public string MerchantCountry { get; set; }
    public string CardMask { get; set; }
    public string RiskFlag { get; set; }
    public string Urgency { get; set; }
    public string RecommendedAction { get; set; }
}

