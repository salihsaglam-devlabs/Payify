using LinkPara.Card.Domain.Entities.FileIngestion.Schemas;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.FileIngestion.Persistence;

public class IngestionCardBkmDetail : AuditEntity, IIngestionTypedDetail,ICardBkmDetail
{
    public Guid FileLineId { get; set; }
    public IngestionFileLine FileLine { get; set; }
    
    public int TransactionDate { get; set; }
    public int TransactionTime { get; set; }
    public int ValueDate { get; set; }
    public int EndOfDayDate { get; set; }
    public string CardNo { get; set; }
    public long OceanTxnGuid { get; set; }
    public long OceanMainTxnGuid { get; set; }
    public string BranchId { get; set; }
    public string Rrn { get; set; }
    public string Arn { get; set; }
    public string ProvisionCode { get; set; }
    public int Stan { get; set; }
    public string MemberRefNo { get; set; }
    public long TraceId { get; set; }
    public int Otc { get; set; }
    public int Ots { get; set; }
    public CardBkmTxnInstallType TxnInstallType { get; set; }
    public string BankingTxnCode { get; set; }
    public string TxnDescription { get; set; }
    public string MerchantName { get; set; }
    public string MerchantCity { get; set; }
    public string MerchantState { get; set; }
    public string MerchantCountry { get; set; }
    public CardBkmFinancialType FinancialType { get; set; }
    public CardBkmTxnEffect TxnEffect { get; set; }
    public CardBkmTxnSource TxnSource { get; set; }
    public CardBkmTxnRegion TxnRegion { get; set; }
    public CardBkmTerminalType TerminalType { get; set; }
    public CardBkmChannelCode ChannelCode { get; set; }
    public string TerminalId { get; set; }
    public string MerchantId { get; set; }
    public int Mcc { get; set; }
    public int AcquirerId { get; set; }
    public int SecurityLevelIndicator { get; set; }
    public CardBkmIsTxnSettle IsTxnSettle { get; set; }
    public CardBkmTxnStat TxnStat { get; set; }
    public string ResponseCode { get; set; }
    public CardBkmIsSuccessfulTxn IsSuccessfulTxn { get; set; }
    public CardBkmTxnOrigin TxnOrigin { get; set; }
    public int InstallCount { get; set; }
    public int InstallOrder { get; set; }
    public string OperatorCode { get; set; }
    public decimal OriginalAmount { get; set; }
    public int OriginalCurrency { get; set; }
    public decimal SettlementAmount { get; set; }
    public int SettlementCurrency { get; set; }
    public decimal CardHolderBillingAmount { get; set; }
    public int CardHolderBillingCurrency { get; set; }
    public decimal BillingAmount { get; set; }
    public int BillingCurrency { get; set; }
    public decimal Tax1 { get; set; }
    public decimal Tax2 { get; set; }
    public decimal CashbackAmount { get; set; }
    public decimal SurchargeAmount { get; set; }
    public string PointType { get; set; }
    public decimal BcPoint { get; set; }
    public decimal McPoint { get; set; }
    public decimal CcPoint { get; set; }
    public decimal BcPointAmount { get; set; }
    public decimal McPointAmount { get; set; }
    public decimal CcPointAmount { get; set; }
}

