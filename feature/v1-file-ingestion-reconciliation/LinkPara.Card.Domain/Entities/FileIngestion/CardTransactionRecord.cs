using LinkPara.SharedModels.Persistence;
using LinkPara.Card.Domain.Enums;

namespace LinkPara.Card.Domain.Entities.FileIngestion;

public class CardTransactionRecord : AuditEntity
{
    public Guid ImportedFileRowId { get; set; }

    public DateOnly? TransactionDate { get; set; }
    public TimeOnly? TransactionTime { get; set; }
    public DateOnly? ValueDate { get; set; }
    public DateOnly? EndOfDayDate { get; set; }
    public string CardNo { get; set; }
    public string OceanTxnGuid { get; set; }
    public string OceanMainTxnGuid { get; set; }
    public string BranchId { get; set; }
    public string Rrn { get; set; }
    public string Arn { get; set; }
    public string ProvisionCode { get; set; }
    public string Stan { get; set; }
    public string MemberRefNo { get; set; }
    public string TraceId { get; set; }
    public string Otc { get; set; }
    public string Ots { get; set; }
    public string TxnInstallType { get; set; }
    public string BankingTxnCode { get; set; }
    public string TxnDescription { get; set; }
    public string MerchantName { get; set; }
    public string MerchantCity { get; set; }
    public string MerchantState { get; set; }
    public string MerchantCountry { get; set; }
    public string FinancialType { get; set; }
    public string TxnEffect { get; set; }
    public string TxnSource { get; set; }
    public string TxnRegion { get; set; }
    public string TerminalType { get; set; }
    public string ChannelCode { get; set; }
    public string TerminalId { get; set; }
    public string MerchantId { get; set; }
    public string Mcc { get; set; }
    public string AcquirerId { get; set; }
    public string SecurityLevelIndicator { get; set; }
    public string IsTxnSettle { get; set; }
    public string TxnStat { get; set; }
    public string ResponseCode { get; set; }
    public string IsSuccessfulTxn { get; set; }
    public int? TxnOrigin { get; set; }
    public int? InstallCount { get; set; }
    public int? InstallOrder { get; set; }
    public string OperatorCode { get; set; }
    public decimal? OriginalAmount { get; set; }
    public string OriginalCurrency { get; set; }
    public decimal? SettlementAmount { get; set; }
    public string SettlementCurrency { get; set; }
    public decimal? CardHolderBillingAmount { get; set; }
    public string CardHolderBillingCurrency { get; set; }
    public decimal? BillingAmount { get; set; }
    public string BillingCurrency { get; set; }
    public decimal? Tax1 { get; set; }
    public decimal? Tax2 { get; set; }
    public decimal? CashbackAmount { get; set; }
    public decimal? SurchargeAmount { get; set; }
    public string PointType { get; set; }
    public decimal? BcPoint { get; set; }
    public decimal? McPoint { get; set; }
    public decimal? CcPoint { get; set; }
    public decimal? BcPointAmount { get; set; }
    public decimal? McPointAmount { get; set; }
    public decimal? CcPointAmount { get; set; }
    public string CorrelationKey { get; set; }
    public CardReconciliationState ReconciliationState { get; set; }
    public DateTime? ReconciliationStateUpdatedAt { get; set; }
    public Guid? LastReconciliationRunId { get; set; }
    public Guid? LastReconciliationExecutionGroupId { get; set; }
    public string ReconciliationStateReason { get; set; }

    public ImportedFileRow ImportedFileRow { get; set; }
}
