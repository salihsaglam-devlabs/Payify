namespace LinkPara.Card.Domain.Entities.FileIngestion.Schemas;

public interface IClearingVisa
{
    IClearingVisaHeader Header { get; set; }
    IClearingVisaDetail Detail { get; set; }
    IClearingVisaFooter Footer { get; set; }
}

public interface IClearingVisaHeader
{
    ClearingVisaHeaderCode HeaderCode { get; set; }
    string FileDate { get; set; }
    long FileNo { get; set; }
    string FileVersionNumber { get; set; }
}

public interface IClearingVisaDetail
{
    ClearingVisaTxnType TxnType { get; set; }
    string IoDate { get; set; }
    ClearingVisaIoFlag IoFlag { get; set; }
    long OceanTxnGuid { get; set; }
    long ClrNo { get; set; }
    string Rrn { get; set; }
    string Arn { get; set; }
    string ReasonCode { get; set; }
    string Reserved { get; set; }
    string ProvisionCode { get; set; }
    string CardNo { get; set; }
    ClearingVisaCardDci? CardDci { get; set; }
    string MccCode { get; set; }
    string Tc { get; set; }
    string UsageCode { get; set; }
    ClearingVisaDisputeCode DisputeCode { get; set; }
    ClearingVisaControlStat ControlStat { get; set; }
    decimal SourceAmount { get; set; }
    int SourceCurrency { get; set; }
    decimal DestinationAmount { get; set; }
    int DestinationCurrency { get; set; }
    decimal CashbackAmount { get; set; }
    decimal ReimbursementAmount { get; set; }
    string ReimbursementAttribute { get; set; }
    int MicrofilmNumber { get; set; }
    string MerchantCity { get; set; }
    string MerchantName { get; set; }
    string CardAcceptorId { get; set; }
    int TxnDate { get; set; }
    int TxnTime { get; set; }
    string FileId { get; set; }
}

public interface IClearingVisaFooter
{
    ClearingVisaFooterCode FooterCode { get; set; }
    string FileDate { get; set; }
    long TxnCount { get; set; }
}

public interface IClearingMsc
{
    IClearingMscHeader Header { get; set; }
    IClearingMscDetail Detail { get; set; }
    IClearingMscFooter Footer { get; set; }
}

public interface IClearingMscHeader
{
    ClearingMscHeaderCode HeaderCode { get; set; }
    string FileDate { get; set; }
    long FileNo { get; set; }
    string FileVersionNumber { get; set; }
}

public interface IClearingMscDetail
{
    ClearingMscTxnType TxnType { get; set; }
    string IoDate { get; set; }
    ClearingMscIoFlag IoFlag { get; set; }
    long OceanTxnGuid { get; set; }
    long ClrNo { get; set; }
    string Rrn { get; set; }
    string Arn { get; set; }
    string ReasonCode { get; set; }
    string Reserved { get; set; }
    string ProvisionCode { get; set; }
    string CardNo { get; set; }
    ClearingMscCardDci? CardDci { get; set; }
    string MccCode { get; set; }
    string Mtid { get; set; }
    string FunctionCode { get; set; }
    string ProcessCode { get; set; }
    string ReversalIndicator { get; set; }
    ClearingMscDisputeCode DisputeCode { get; set; }
    ClearingMscControlStat ControlStat { get; set; }
    decimal SourceAmount { get; set; }
    int SourceCurrency { get; set; }
    decimal DestinationAmount { get; set; }
    int DestinationCurrency { get; set; }
    decimal CashbackAmount { get; set; }
    decimal ReimbursementAmount { get; set; }
    string ReimbursementAttribute { get; set; }
    string AncillaryTransactionCode { get; set; }
    string AncillaryTransactionAmount { get; set; }
    int MicrofilmNumber { get; set; }
    string MerchantCity { get; set; }
    string MerchantName { get; set; }
    string CardAcceptorId { get; set; }
    int TxnDate { get; set; }
    int TxnTime { get; set; }
    string FileId { get; set; }
}

public interface IClearingMscFooter
{
    ClearingMscFooterCode FooterCode { get; set; }
    string FileDate { get; set; }
    long TxnCount { get; set; }
}

public interface IClearingBkm
{
    IClearingBkmHeader Header { get; set; }
    IClearingBkmDetail Detail { get; set; }
    IClearingBkmFooter Footer { get; set; }
}

public interface IClearingBkmHeader
{
    ClearingBkmHeaderCode HeaderCode { get; set; }
    string FileDate { get; set; }
    long FileNo { get; set; }
    string FileVersionNumber { get; set; }
}

public interface IClearingBkmDetail
{
    ClearingBkmTxnType TxnType { get; set; }
    string IoDate { get; set; }
    ClearingBkmIoFlag IoFlag { get; set; }
    long OceanTxnGuid { get; set; }
    long ClrNo { get; set; }
    string Rrn { get; set; }
    string Arn { get; set; }
    string ReasonCode { get; set; }
    string Reserved { get; set; }
    string ProvisionCode { get; set; }
    string CardNo { get; set; }
    ClearingBkmCardDci? CardDci { get; set; }
    string MccCode { get; set; }
    string Mtid { get; set; }
    string FunctionCode { get; set; }
    string ProcessCode { get; set; }
    ClearingBkmDisputeCode DisputeCode { get; set; }
    ClearingBkmControlStat ControlStat { get; set; }
    decimal SourceAmount { get; set; }
    int SourceCurrency { get; set; }
    decimal DestinationAmount { get; set; }
    int DestinationCurrency { get; set; }
    decimal CashbackAmount { get; set; }
    decimal ReimbursementAmount { get; set; }
    string ReimbursementAttribute { get; set; }
    int MicrofilmNumber { get; set; }
    string MerchantCity { get; set; }
    string MerchantName { get; set; }
    string CardAcceptorId { get; set; }
    int TxnDate { get; set; }
    int TxnTime { get; set; }
    string FileId { get; set; }
}

public interface IClearingBkmFooter
{
    ClearingBkmFooterCode FooterCode { get; set; }
    string FileDate { get; set; }
    long TxnCount { get; set; }
}

public interface ICardVisa
{
    ICardVisaHeader Header { get; set; }
    ICardVisaDetail Detail { get; set; }
    ICardVisaFooter Footer { get; set; }
}

public interface ICardVisaHeader
{
    CardVisaHeaderCode HeaderCode { get; set; }
    string FileDate { get; set; }
    string FileNo { get; set; }
    string FileVersionNumber { get; set; }
}

public interface ICardVisaDetail
{
    int TransactionDate { get; set; }
    int TransactionTime { get; set; }
    int ValueDate { get; set; }
    int EndOfDayDate { get; set; }
    string CardNo { get; set; }
    long OceanTxnGuid { get; set; }
    long OceanMainTxnGuid { get; set; }
    string BranchId { get; set; }
    string Rrn { get; set; }
    string Arn { get; set; }
    string ProvisionCode { get; set; }
    int Stan { get; set; }
    string MemberRefNo { get; set; }
    long TraceId { get; set; }
    int Otc { get; set; }
    int Ots { get; set; }
    CardVisaTxnInstallType TxnInstallType { get; set; }
    string BankingTxnCode { get; set; }
    string TxnDescription { get; set; }
    string MerchantName { get; set; }
    string MerchantCity { get; set; }
    string MerchantState { get; set; }
    string MerchantCountry { get; set; }
    CardVisaFinancialType FinancialType { get; set; }
    CardVisaTxnEffect TxnEffect { get; set; }
    CardVisaTxnSource TxnSource { get; set; }
    CardVisaTxnRegion TxnRegion { get; set; }
    CardVisaTerminalType TerminalType { get; set; }
    CardVisaChannelCode ChannelCode { get; set; }
    string TerminalId { get; set; }
    string MerchantId { get; set; }
    int Mcc { get; set; }
    int AcquirerId { get; set; }
    int SecurityLevelIndicator { get; set; }
    CardVisaIsTxnSettle IsTxnSettle { get; set; }
    CardVisaTxnStat TxnStat { get; set; }
    string ResponseCode { get; set; }
    CardVisaIsSuccessfulTxn IsSuccessfulTxn { get; set; }
    CardVisaTxnOrigin TxnOrigin { get; set; }
    int InstallCount { get; set; }
    int InstallOrder { get; set; }
    string OperatorCode { get; set; }
    decimal OriginalAmount { get; set; }
    int OriginalCurrency { get; set; }
    decimal SettlementAmount { get; set; }
    int SettlementCurrency { get; set; }
    decimal CardHolderBillingAmount { get; set; }
    int CardHolderBillingCurrency { get; set; }
    decimal BillingAmount { get; set; }
    int BillingCurrency { get; set; }
    decimal Tax1 { get; set; }
    decimal Tax2 { get; set; }
    decimal CashbackAmount { get; set; }
    decimal SurchargeAmount { get; set; }
    string PointType { get; set; }
    decimal BcPoint { get; set; }
    decimal McPoint { get; set; }
    decimal CcPoint { get; set; }
    decimal BcPointAmount { get; set; }
    decimal McPointAmount { get; set; }
    decimal CcPointAmount { get; set; }
}

public interface ICardVisaFooter
{
    CardVisaFooterCode FooterCode { get; set; }
    string FileDate { get; set; }
    long TxnCount { get; set; }
}

public interface ICardMsc
{
    ICardMscHeader Header { get; set; }
    ICardMscDetail Detail { get; set; }
    ICardMscFooter Footer { get; set; }
}

public interface ICardMscHeader
{
    CardMscHeaderCode HeaderCode { get; set; }
    string FileDate { get; set; }
    string FileNo { get; set; }
    string FileVersionNumber { get; set; }
}

public interface ICardMscDetail
{
    int TransactionDate { get; set; }
    int TransactionTime { get; set; }
    int ValueDate { get; set; }
    int EndOfDayDate { get; set; }
    string CardNo { get; set; }
    long OceanTxnGuid { get; set; }
    long OceanMainTxnGuid { get; set; }
    string BranchId { get; set; }
    string Rrn { get; set; }
    string Arn { get; set; }
    string ProvisionCode { get; set; }
    int Stan { get; set; }
    string MemberRefNo { get; set; }
    long TraceId { get; set; }
    int Otc { get; set; }
    int Ots { get; set; }
    CardMscTxnInstallType TxnInstallType { get; set; }
    string BankingTxnCode { get; set; }
    string TxnDescription { get; set; }
    string MerchantName { get; set; }
    string MerchantCity { get; set; }
    string MerchantState { get; set; }
    string MerchantCountry { get; set; }
    CardMscFinancialType FinancialType { get; set; }
    CardMscTxnEffect TxnEffect { get; set; }
    CardMscTxnSource TxnSource { get; set; }
    CardMscTxnRegion TxnRegion { get; set; }
    CardMscTerminalType TerminalType { get; set; }
    CardMscChannelCode ChannelCode { get; set; }
    string TerminalId { get; set; }
    string MerchantId { get; set; }
    int Mcc { get; set; }
    int AcquirerId { get; set; }
    int SecurityLevelIndicator { get; set; }
    CardMscIsTxnSettle IsTxnSettle { get; set; }
    CardMscTxnStat TxnStat { get; set; }
    string ResponseCode { get; set; }
    CardMscIsSuccessfulTxn IsSuccessfulTxn { get; set; }
    CardMscTxnOrigin TxnOrigin { get; set; }
    int InstallCount { get; set; }
    int InstallOrder { get; set; }
    string OperatorCode { get; set; }
    decimal OriginalAmount { get; set; }
    int OriginalCurrency { get; set; }
    decimal SettlementAmount { get; set; }
    int SettlementCurrency { get; set; }
    decimal CardHolderBillingAmount { get; set; }
    int CardHolderBillingCurrency { get; set; }
    decimal BillingAmount { get; set; }
    int BillingCurrency { get; set; }
    decimal Tax1 { get; set; }
    decimal Tax2 { get; set; }
    decimal CashbackAmount { get; set; }
    decimal SurchargeAmount { get; set; }
    string PointType { get; set; }
    decimal BcPoint { get; set; }
    decimal McPoint { get; set; }
    decimal CcPoint { get; set; }
    decimal BcPointAmount { get; set; }
    decimal McPointAmount { get; set; }
    decimal CcPointAmount { get; set; }
}

public interface ICardMscFooter
{
    CardMscFooterCode FooterCode { get; set; }
    string FileDate { get; set; }
    long TxnCount { get; set; }
}

public interface ICardBkm
{
    ICardBkmHeader Header { get; set; }
    ICardBkmDetail Detail { get; set; }
    ICardBkmFooter Footer { get; set; }
}

public interface ICardBkmHeader
{
    CardBkmHeaderCode HeaderCode { get; set; }
    string FileDate { get; set; }
    string FileNo { get; set; }
    string FileVersionNumber { get; set; }
}

public interface ICardBkmDetail
{
    int TransactionDate { get; set; }
    int TransactionTime { get; set; }
    int ValueDate { get; set; }
    int EndOfDayDate { get; set; }
    string CardNo { get; set; }
    long OceanTxnGuid { get; set; }
    long OceanMainTxnGuid { get; set; }
    string BranchId { get; set; }
    string Rrn { get; set; }
    string Arn { get; set; }
    string ProvisionCode { get; set; }
    int Stan { get; set; }
    string MemberRefNo { get; set; }
    long TraceId { get; set; }
    int Otc { get; set; }
    int Ots { get; set; }
    CardBkmTxnInstallType TxnInstallType { get; set; }
    string BankingTxnCode { get; set; }
    string TxnDescription { get; set; }
    string MerchantName { get; set; }
    string MerchantCity { get; set; }
    string MerchantState { get; set; }
    string MerchantCountry { get; set; }
    CardBkmFinancialType FinancialType { get; set; }
    CardBkmTxnEffect TxnEffect { get; set; }
    CardBkmTxnSource TxnSource { get; set; }
    CardBkmTxnRegion TxnRegion { get; set; }
    CardBkmTerminalType TerminalType { get; set; }
    CardBkmChannelCode ChannelCode { get; set; }
    string TerminalId { get; set; }
    string MerchantId { get; set; }
    int Mcc { get; set; }
    int AcquirerId { get; set; }
    int SecurityLevelIndicator { get; set; }
    CardBkmIsTxnSettle IsTxnSettle { get; set; }
    CardBkmTxnStat TxnStat { get; set; }
    string ResponseCode { get; set; }
    CardBkmIsSuccessfulTxn IsSuccessfulTxn { get; set; }
    CardBkmTxnOrigin TxnOrigin { get; set; }
    int InstallCount { get; set; }
    int InstallOrder { get; set; }
    string OperatorCode { get; set; }
    decimal OriginalAmount { get; set; }
    int OriginalCurrency { get; set; }
    decimal SettlementAmount { get; set; }
    int SettlementCurrency { get; set; }
    decimal CardHolderBillingAmount { get; set; }
    int CardHolderBillingCurrency { get; set; }
    decimal BillingAmount { get; set; }
    int BillingCurrency { get; set; }
    decimal Tax1 { get; set; }
    decimal Tax2 { get; set; }
    decimal CashbackAmount { get; set; }
    decimal SurchargeAmount { get; set; }
    string PointType { get; set; }
    decimal BcPoint { get; set; }
    decimal McPoint { get; set; }
    decimal CcPoint { get; set; }
    decimal BcPointAmount { get; set; }
    decimal McPointAmount { get; set; }
    decimal CcPointAmount { get; set; }
}

public interface ICardBkmFooter
{
    CardBkmFooterCode FooterCode { get; set; }
    string FileDate { get; set; }
    long TxnCount { get; set; }
}