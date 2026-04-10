using System.Runtime.Serialization;

namespace LinkPara.Card.Domain.Entities.FileIngestion.Schemas;

#region CLEARING VISA

public partial class ClearingVisa
{
    public ClearingVisaHeader Header { get; set; } = new();
    public ClearingVisaDetail Detail { get; set; } = new();
    public ClearingVisaFooter Footer { get; set; } = new();
}

public partial class ClearingVisaHeader
{
    public ClearingVisaHeaderCode HeaderCode { get; set; }
    public string FileDate { get; set; } = default!;
    public long FileNo { get; set; }
    public string FileVersionNumber { get; set; } = default!;
}

public partial class ClearingVisaDetail
{
    public ClearingVisaTxnType TxnType { get; set; }
    public string IoDate { get; set; } = default!;
    public ClearingVisaIoFlag IoFlag { get; set; }
    public long OceanTxnGuid { get; set; }
    public long ClrNo { get; set; }
    public string Rrn { get; set; } = default!;
    public string Arn { get; set; } = default!;
    public string ReasonCode { get; set; } = default!;
    public string Reserved { get; set; } = default!;
    public string ProvisionCode { get; set; } = default!;
    public string CardNo { get; set; } = default!;
    public ClearingVisaCardDci? CardDci { get; set; }
    public string MccCode { get; set; } = default!;
    public string Tc { get; set; } = default!;
    public string UsageCode { get; set; } = default!;
    public ClearingVisaDisputeCode DisputeCode { get; set; }
    public ClearingVisaControlStat ControlStat { get; set; }
    public decimal SourceAmount { get; set; }
    public int SourceCurrency { get; set; }
    public decimal DestinationAmount { get; set; }
    public int DestinationCurrency { get; set; }
    public decimal CashbackAmount { get; set; }
    public decimal ReimbursementAmount { get; set; }
    public string ReimbursementAttribute { get; set; } = default!;
    public int MicrofilmNumber { get; set; }
    public string MerchantCity { get; set; } = default!;
    public string MerchantName { get; set; } = default!;
    public string CardAcceptorId { get; set; } = default!;
    public int TxnDate { get; set; }
    public int TxnTime { get; set; }
    public string FileId { get; set; } = default!;
}

public partial class ClearingVisaFooter
{
    public ClearingVisaFooterCode FooterCode { get; set; }
    public string FileDate { get; set; } = default!;
    public long TxnCount { get; set; }
}

public enum ClearingVisaHeaderCode
{
    [EnumMember(Value = "H")]
    Header
}

public enum ClearingVisaFooterCode
{
    [EnumMember(Value = "F")]
    Footer
}

public enum ClearingVisaTxnType
{
    [EnumMember(Value = "I")]
    Issuer,
    [EnumMember(Value = "A")]
    Acquirer,
    [EnumMember(Value = "D")]
    Document,
    [EnumMember(Value = "F")]
    Fee,
    [EnumMember(Value = "R")]
    Fraud
}

public enum ClearingVisaIoFlag
{
    [EnumMember(Value = "I")]
    Incoming,
    [EnumMember(Value = "O")]
    Outgoing
}

public enum ClearingVisaCardDci
{
    [EnumMember(Value = "D")]
    Debit,
    [EnumMember(Value = "P")]
    Prepaid,
    [EnumMember(Value = "C")]
    Credit
}

public enum ClearingVisaDisputeCode
{
    [EnumMember(Value = "H")]
    FeeRepClosing,
    [EnumMember(Value = "M")]
    OutCbCardholderCredit,
    [EnumMember(Value = "U")]
    MerchantCbCardholderCredit,
    [EnumMember(Value = "B")]
    Loss,
    [EnumMember(Value = "E")]
    AccountingCure,
    [EnumMember(Value = "C")]
    RepCardholderCure,
    [EnumMember(Value = "G")]
    OutCbTemporaryAccount,
    [EnumMember(Value = "F")]
    MerchantCbTemporaryAccount,
    [EnumMember(Value = "Z")]
    ExpiryDifferenceAccountingCure,
    [EnumMember(Value = "-")]
    None,
    [EnumMember(Value = "X")]
    FraudTemporaryAccount,
    [EnumMember(Value = "Y")]
    NegativeExchangeRateDifferences,
    [EnumMember(Value = "R")]
    PlusExchangeRateDifferences,
    [EnumMember(Value = "A")]
    OpenMerchantAccounting,
    [EnumMember(Value = "S")]
    Insurance,
    [EnumMember(Value = "T")]
    RejectedExpenseSecurity,
    [EnumMember(Value = "J")]
    PurchasingInsurance,
    [EnumMember(Value = "I")]
    AcceptedLoss,
    [EnumMember(Value = "D")]
    FraudCommission,
    [EnumMember(Value = "V")]
    AcceptedLossSecurity,
    [EnumMember(Value = "P")]
    FraudTemporarily,
    [EnumMember(Value = "N")]
    OutCbWithFeeChCharge,
    [EnumMember(Value = "L")]
    OutCbCbFeeWithSuspenseAccount
}

public enum ClearingVisaControlStat
{
    [EnumMember(Value = "N")]
    Normal,
    [EnumMember(Value = "P")]
    Problem,
    [EnumMember(Value = "D")]
    ChargebackClosing,
    [EnumMember(Value = "X")]
    ProblemToNormal,
    [EnumMember(Value = "Y")]
    DisputeEnd
}

#endregion

#region CLEARING MSC

public partial class ClearingMsc
{
    public ClearingMscHeader Header { get; set; } = new();
    public ClearingMscDetail Detail { get; set; } = new();
    public ClearingMscFooter Footer { get; set; } = new();
}

public partial class ClearingMscHeader
{
    public ClearingMscHeaderCode HeaderCode { get; set; }
    public string FileDate { get; set; } = default!;
    public long FileNo { get; set; }
    public string FileVersionNumber { get; set; } = default!;
}

public partial class ClearingMscDetail
{
    public ClearingMscTxnType TxnType { get; set; }
    public string IoDate { get; set; } = default!;
    public ClearingMscIoFlag IoFlag { get; set; }
    public long OceanTxnGuid { get; set; }
    public long ClrNo { get; set; }
    public string Rrn { get; set; } = default!;
    public string Arn { get; set; } = default!;
    public string ReasonCode { get; set; } = default!;
    public string Reserved { get; set; } = default!;
    public string ProvisionCode { get; set; } = default!;
    public string CardNo { get; set; } = default!;
    public ClearingMscCardDci? CardDci { get; set; }
    public string MccCode { get; set; } = default!;
    public string Mtid { get; set; } = default!;
    public string FunctionCode { get; set; } = default!;
    public string ProcessCode { get; set; } = default!;
    public string ReversalIndicator { get; set; } = default!;
    public ClearingMscDisputeCode DisputeCode { get; set; }
    public ClearingMscControlStat ControlStat { get; set; }
    public decimal SourceAmount { get; set; }
    public int SourceCurrency { get; set; }
    public decimal DestinationAmount { get; set; }
    public int DestinationCurrency { get; set; }
    public decimal CashbackAmount { get; set; }
    public decimal ReimbursementAmount { get; set; }
    public string ReimbursementAttribute { get; set; } = default!;
    public string AncillaryTransactionCode { get; set; } = default!;
    public string AncillaryTransactionAmount { get; set; } = default!;
    public int MicrofilmNumber { get; set; }
    public string MerchantCity { get; set; } = default!;
    public string MerchantName { get; set; } = default!;
    public string CardAcceptorId { get; set; } = default!;
    public int TxnDate { get; set; }
    public int TxnTime { get; set; }
    public string FileId { get; set; } = default!;
}

public partial class ClearingMscFooter
{
    public ClearingMscFooterCode FooterCode { get; set; }
    public string FileDate { get; set; } = default!;
    public long TxnCount { get; set; }
}

public enum ClearingMscHeaderCode
{
    [EnumMember(Value = "H")]
    Header
}

public enum ClearingMscFooterCode
{
    [EnumMember(Value = "F")]
    Footer
}

public enum ClearingMscTxnType
{
    [EnumMember(Value = "I")]
    Issuer,
    [EnumMember(Value = "A")]
    Acquirer,
    [EnumMember(Value = "D")]
    Document,
    [EnumMember(Value = "F")]
    Fee,
    [EnumMember(Value = "R")]
    Fraud
}

public enum ClearingMscIoFlag
{
    [EnumMember(Value = "I")]
    Incoming,
    [EnumMember(Value = "O")]
    Outgoing
}

public enum ClearingMscCardDci
{
    [EnumMember(Value = "D")]
    Debit,
    [EnumMember(Value = "P")]
    Prepaid,
    [EnumMember(Value = "C")]
    Credit
}

public enum ClearingMscDisputeCode
{
    [EnumMember(Value = "H")]
    FeeRepClosing,
    [EnumMember(Value = "M")]
    OutCbCardholderCredit,
    [EnumMember(Value = "U")]
    MerchantCbCardholderCredit,
    [EnumMember(Value = "B")]
    Loss,
    [EnumMember(Value = "E")]
    AccountingCure,
    [EnumMember(Value = "C")]
    RepCardholderCure,
    [EnumMember(Value = "G")]
    OutCbTemporaryAccount,
    [EnumMember(Value = "F")]
    MerchantCbTemporaryAccount,
    [EnumMember(Value = "Z")]
    ExpiryDifferenceAccountingCure,
    [EnumMember(Value = "-")]
    None,
    [EnumMember(Value = "X")]
    FraudTemporaryAccount,
    [EnumMember(Value = "Y")]
    NegativeExchangeRateDifferences,
    [EnumMember(Value = "R")]
    PlusExchangeRateDifferences,
    [EnumMember(Value = "A")]
    OpenMerchantAccounting,
    [EnumMember(Value = "S")]
    Insurance,
    [EnumMember(Value = "T")]
    RejectedExpenseSecurity,
    [EnumMember(Value = "J")]
    PurchasingInsurance,
    [EnumMember(Value = "I")]
    AcceptedLoss,
    [EnumMember(Value = "D")]
    FraudCommission,
    [EnumMember(Value = "V")]
    AcceptedLossSecurity,
    [EnumMember(Value = "P")]
    FraudTemporarily,
    [EnumMember(Value = "N")]
    OutCbWithFeeChCharge,
    [EnumMember(Value = "L")]
    OutCbCbFeeWithSuspenseAccount
}

public enum ClearingMscControlStat
{
    [EnumMember(Value = "N")]
    Normal,
    [EnumMember(Value = "P")]
    Problem,
    [EnumMember(Value = "D")]
    ChargebackClosing,
    [EnumMember(Value = "X")]
    ProblemToNormal,
    [EnumMember(Value = "Y")]
    DisputeEnd
}

#endregion

#region CLEARING BKM

public partial class ClearingBkm
{
    public ClearingBkmHeader Header { get; set; } = new();
    public ClearingBkmDetail Detail { get; set; } = new();
    public ClearingBkmFooter Footer { get; set; } = new();
}

public partial class ClearingBkmHeader
{
    public ClearingBkmHeaderCode HeaderCode { get; set; }
    public string FileDate { get; set; } = default!;
    public long FileNo { get; set; }
    public string FileVersionNumber { get; set; } = default!;
}

public partial class ClearingBkmDetail
{
    public ClearingBkmTxnType TxnType { get; set; }
    public string IoDate { get; set; } = default!;
    public ClearingBkmIoFlag IoFlag { get; set; }
    public long OceanTxnGuid { get; set; }
    public long ClrNo { get; set; }
    public string Rrn { get; set; } = default!;
    public string Arn { get; set; } = default!;
    public string ReasonCode { get; set; } = default!;
    public string Reserved { get; set; } = default!;
    public string ProvisionCode { get; set; } = default!;
    public string CardNo { get; set; } = default!;
    public ClearingBkmCardDci? CardDci { get; set; }
    public string MccCode { get; set; } = default!;
    public string Mtid { get; set; } = default!;
    public string FunctionCode { get; set; } = default!;
    public string ProcessCode { get; set; } = default!;
    public ClearingBkmDisputeCode DisputeCode { get; set; }
    public ClearingBkmControlStat ControlStat { get; set; }
    public decimal SourceAmount { get; set; }
    public int SourceCurrency { get; set; }
    public decimal DestinationAmount { get; set; }
    public int DestinationCurrency { get; set; }
    public decimal CashbackAmount { get; set; }
    public decimal ReimbursementAmount { get; set; }
    public string ReimbursementAttribute { get; set; } = default!;
    public int MicrofilmNumber { get; set; }
    public string MerchantCity { get; set; } = default!;
    public string MerchantName { get; set; } = default!;
    public string CardAcceptorId { get; set; } = default!;
    public int TxnDate { get; set; }
    public int TxnTime { get; set; }
    public string FileId { get; set; } = default!;
}

public partial class ClearingBkmFooter
{
    public ClearingBkmFooterCode FooterCode { get; set; }
    public string FileDate { get; set; } = default!;
    public long TxnCount { get; set; }
}

public enum ClearingBkmHeaderCode
{
    [EnumMember(Value = "H")]
    Header
}

public enum ClearingBkmFooterCode
{
    [EnumMember(Value = "F")]
    Footer
}

public enum ClearingBkmTxnType
{
    [EnumMember(Value = "I")]
    Issuer,
    [EnumMember(Value = "A")]
    Acquirer,
    [EnumMember(Value = "D")]
    Document,
    [EnumMember(Value = "F")]
    Fee,
    [EnumMember(Value = "R")]
    Fraud,
    [EnumMember(Value = "M")]
    ServiceFee
}

public enum ClearingBkmIoFlag
{
    [EnumMember(Value = "I")]
    Incoming,
    [EnumMember(Value = "O")]
    Outgoing
}

public enum ClearingBkmCardDci
{
    [EnumMember(Value = "D")]
    Debit,
    [EnumMember(Value = "P")]
    Prepaid,
    [EnumMember(Value = "C")]
    Credit
}

public enum ClearingBkmDisputeCode
{
    [EnumMember(Value = "H")]
    FeeRepClosing,
    [EnumMember(Value = "M")]
    OutCbCardholderCredit,
    [EnumMember(Value = "U")]
    MerchantCbCardholderCredit,
    [EnumMember(Value = "B")]
    Loss,
    [EnumMember(Value = "E")]
    AccountingCure,
    [EnumMember(Value = "C")]
    RepCardholderCure,
    [EnumMember(Value = "G")]
    OutCbTemporaryAccount,
    [EnumMember(Value = "F")]
    MerchantCbTemporaryAccount,
    [EnumMember(Value = "Z")]
    ExpiryDifferenceAccountingCure,
    [EnumMember(Value = "-")]
    None,
    [EnumMember(Value = "X")]
    FraudTemporaryAccount,
    [EnumMember(Value = "Y")]
    NegativeExchangeRateDifferences,
    [EnumMember(Value = "R")]
    PlusExchangeRateDifferences,
    [EnumMember(Value = "A")]
    OpenMerchantAccounting,
    [EnumMember(Value = "S")]
    Insurance,
    [EnumMember(Value = "T")]
    RejectedExpenseSecurity,
    [EnumMember(Value = "J")]
    PurchasingInsurance,
    [EnumMember(Value = "I")]
    AcceptedLoss,
    [EnumMember(Value = "D")]
    FraudCommission,
    [EnumMember(Value = "V")]
    AcceptedLossSecurity,
    [EnumMember(Value = "P")]
    FraudTemporarily,
    [EnumMember(Value = "N")]
    OutCbWithFeeChCharge,
    [EnumMember(Value = "L")]
    OutCbCbFeeWithSuspenseAccount
}

public enum ClearingBkmControlStat
{
    [EnumMember(Value = "N")]
    Normal,
    [EnumMember(Value = "P")]
    Problem,
    [EnumMember(Value = "D")]
    ChargebackClosing,
    [EnumMember(Value = "X")]
    ProblemToNormal,
    [EnumMember(Value = "Y")]
    DisputeEnd
}

#endregion

#region CARD VISA

public partial class CardVisa
{
    public CardVisaHeader Header { get; set; } = new();
    public CardVisaDetail Detail { get; set; } = new();
    public CardVisaFooter Footer { get; set; } = new();
}

public partial class CardVisaHeader
{
    public CardVisaHeaderCode HeaderCode { get; set; }
    public string FileDate { get; set; } = default!;
    public string FileNo { get; set; } = default!;
    public string FileVersionNumber { get; set; } = default!;
}

public partial class CardVisaDetail
{
    public int TransactionDate { get; set; }
    public int TransactionTime { get; set; }
    public int ValueDate { get; set; }
    public int EndOfDayDate { get; set; }
    public string CardNo { get; set; } = default!;
    public long OceanTxnGuid { get; set; }
    public long OceanMainTxnGuid { get; set; }
    public string BranchId { get; set; } = default!;
    public string Rrn { get; set; } = default!;
    public string Arn { get; set; } = default!;
    public string ProvisionCode { get; set; } = default!;
    public int Stan { get; set; }
    public string MemberRefNo { get; set; } = default!;
    public long TraceId { get; set; }
    public int Otc { get; set; }
    public int Ots { get; set; }
    public CardVisaTxnInstallType TxnInstallType { get; set; }
    public string BankingTxnCode { get; set; } = default!;
    public string TxnDescription { get; set; } = default!;
    public string MerchantName { get; set; } = default!;
    public string MerchantCity { get; set; } = default!;
    public string MerchantState { get; set; } = default!;
    public string MerchantCountry { get; set; } = default!;
    public CardVisaFinancialType FinancialType { get; set; }
    public CardVisaTxnEffect TxnEffect { get; set; }
    public CardVisaTxnSource TxnSource { get; set; }
    public CardVisaTxnRegion TxnRegion { get; set; }
    public CardVisaTerminalType TerminalType { get; set; }
    public CardVisaChannelCode ChannelCode { get; set; }
    public string TerminalId { get; set; } = default!;
    public string MerchantId { get; set; } = default!;
    public int Mcc { get; set; }
    public int AcquirerId { get; set; }
    public int SecurityLevelIndicator { get; set; }
    public CardVisaIsTxnSettle IsTxnSettle { get; set; }
    public CardVisaTxnStat TxnStat { get; set; }
    public string ResponseCode { get; set; } = default!;
    public CardVisaIsSuccessfulTxn IsSuccessfulTxn { get; set; }
    public CardVisaTxnOrigin TxnOrigin { get; set; }
    public int InstallCount { get; set; }
    public int InstallOrder { get; set; }
    public string OperatorCode { get; set; } = default!;
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
    public string PointType { get; set; } = default!;
    public decimal BcPoint { get; set; }
    public decimal McPoint { get; set; }
    public decimal CcPoint { get; set; }
    public decimal BcPointAmount { get; set; }
    public decimal McPointAmount { get; set; }
    public decimal CcPointAmount { get; set; }
}

public partial class CardVisaFooter
{
    public CardVisaFooterCode FooterCode { get; set; }
    public string FileDate { get; set; } = default!;
    public long TxnCount { get; set; }
}

public enum CardVisaHeaderCode
{
    [EnumMember(Value = "H")]
    Header
}

public enum CardVisaFooterCode
{
    [EnumMember(Value = "F")]
    Footer
}

public enum CardVisaTxnInstallType
{
    [EnumMember(Value = "N")]
    Normal,
    [EnumMember(Value = "Y")]
    Installment,
    [EnumMember(Value = "I")]
    InstallmentWithInterest,
    [EnumMember(Value = "T")]
    InstallmentWithInstructionOrCashAdvance
}

public enum CardVisaFinancialType
{
    [EnumMember(Value = "C")]
    Capital,
    [EnumMember(Value = "F")]
    Fee,
    [EnumMember(Value = "I")]
    Interest,
    [EnumMember(Value = "B")]
    Tax1,
    [EnumMember(Value = "K")]
    Tax2,
    [EnumMember(Value = "M")]
    Payment,
    [EnumMember(Value = "P")]
    Point
}

public enum CardVisaTxnEffect
{
    [EnumMember(Value = "D")]
    Debit,
    [EnumMember(Value = "C")]
    Credit,
    [EnumMember(Value = "P")]
    CreditPoint,
    [EnumMember(Value = "M")]
    DebitPoint
}

public enum CardVisaTxnSource
{
    [EnumMember(Value = "O")]
    Onus,
    [EnumMember(Value = "N")]
    Domestic,
    [EnumMember(Value = "V")]
    Visa,
    [EnumMember(Value = "M")]
    Mastercard
}

public enum CardVisaTxnRegion
{
    [EnumMember(Value = "O")]
    Onus,
    [EnumMember(Value = "D")]
    Domestic,
    [EnumMember(Value = "I")]
    International,
    [EnumMember(Value = "R")]
    IntraRegional
}

public enum CardVisaTerminalType
{
    [EnumMember(Value = "PO")]
    Pos,
    [EnumMember(Value = "AT")]
    Atm,
    [EnumMember(Value = "EP")]
    Epos,
    [EnumMember(Value = "IN")]
    InternetBanking,
    [EnumMember(Value = "IV")]
    Ivr,
    [EnumMember(Value = "VP")]
    VirtualPos,
    [EnumMember(Value = "CT")]
    Crt,
    [EnumMember(Value = "BR")]
    BranchScreen,
    [EnumMember(Value = "UA")]
    UnattendedPos,
    [EnumMember(Value = "VL")]
    Validator,
    [EnumMember(Value = "KI")]
    Kiosk
}

public enum CardVisaChannelCode
{
    [EnumMember(Value = "SYS")]
    System,
    [EnumMember(Value = "OCN")]
    OceanFrontend
}

public enum CardVisaIsTxnSettle
{
    [EnumMember(Value = "Y")]
    Settled,
    [EnumMember(Value = "N")]
    Unsettled
}

public enum CardVisaTxnStat
{
    [EnumMember(Value = "N")]
    Normal,
    [EnumMember(Value = "R")]
    Reverse,
    [EnumMember(Value = "V")]
    Void,
    [EnumMember(Value = "E")]
    Expired
}

public enum CardVisaIsSuccessfulTxn
{
    [EnumMember(Value = "Y")]
    Successful,
    [EnumMember(Value = "N")]
    Unsuccessful
}

public enum CardVisaTxnOrigin
{
    [EnumMember(Value = "0")]
    Authorization,
    [EnumMember(Value = "1")]
    UserGenerated,
    [EnumMember(Value = "2")]
    SystemGenerated,
    [EnumMember(Value = "3")]
    ChannelGenerated
}

#endregion

#region CARD MSC

public partial class CardMsc
{
    public CardMscHeader Header { get; set; } = new();
    public CardMscDetail Detail { get; set; } = new();
    public CardMscFooter Footer { get; set; } = new();
}

public partial class CardMscHeader
{
    public CardMscHeaderCode HeaderCode { get; set; }
    public string FileDate { get; set; } = default!;
    public string FileNo { get; set; } = default!;
    public string FileVersionNumber { get; set; } = default!;
}

public partial class CardMscDetail
{
    public int TransactionDate { get; set; }
    public int TransactionTime { get; set; }
    public int ValueDate { get; set; }
    public int EndOfDayDate { get; set; }
    public string CardNo { get; set; } = default!;
    public long OceanTxnGuid { get; set; }
    public long OceanMainTxnGuid { get; set; }
    public string BranchId { get; set; } = default!;
    public string Rrn { get; set; } = default!;
    public string Arn { get; set; } = default!;
    public string ProvisionCode { get; set; } = default!;
    public int Stan { get; set; }
    public string MemberRefNo { get; set; } = default!;
    public long TraceId { get; set; }
    public int Otc { get; set; }
    public int Ots { get; set; }
    public CardMscTxnInstallType TxnInstallType { get; set; }
    public string BankingTxnCode { get; set; } = default!;
    public string TxnDescription { get; set; } = default!;
    public string MerchantName { get; set; } = default!;
    public string MerchantCity { get; set; } = default!;
    public string MerchantState { get; set; } = default!;
    public string MerchantCountry { get; set; } = default!;
    public CardMscFinancialType FinancialType { get; set; }
    public CardMscTxnEffect TxnEffect { get; set; }
    public CardMscTxnSource TxnSource { get; set; }
    public CardMscTxnRegion TxnRegion { get; set; }
    public CardMscTerminalType TerminalType { get; set; }
    public CardMscChannelCode ChannelCode { get; set; }
    public string TerminalId { get; set; } = default!;
    public string MerchantId { get; set; } = default!;
    public int Mcc { get; set; }
    public int AcquirerId { get; set; }
    public int SecurityLevelIndicator { get; set; }
    public CardMscIsTxnSettle IsTxnSettle { get; set; }
    public CardMscTxnStat TxnStat { get; set; }
    public string ResponseCode { get; set; } = default!;
    public CardMscIsSuccessfulTxn IsSuccessfulTxn { get; set; }
    public CardMscTxnOrigin TxnOrigin { get; set; }
    public int InstallCount { get; set; }
    public int InstallOrder { get; set; }
    public string OperatorCode { get; set; } = default!;
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
    public string PointType { get; set; } = default!;
    public decimal BcPoint { get; set; }
    public decimal McPoint { get; set; }
    public decimal CcPoint { get; set; }
    public decimal BcPointAmount { get; set; }
    public decimal McPointAmount { get; set; }
    public decimal CcPointAmount { get; set; }
}

public partial class CardMscFooter
{
    public CardMscFooterCode FooterCode { get; set; }
    public string FileDate { get; set; } = default!;
    public long TxnCount { get; set; }
}

public enum CardMscHeaderCode
{
    [EnumMember(Value = "H")]
    Header
}

public enum CardMscFooterCode
{
    [EnumMember(Value = "F")]
    Footer
}

public enum CardMscTxnInstallType
{
    [EnumMember(Value = "N")]
    Normal,
    [EnumMember(Value = "Y")]
    Installment,
    [EnumMember(Value = "I")]
    InstallmentWithInterest,
    [EnumMember(Value = "T")]
    InstallmentWithInstructionOrCashAdvance
}

public enum CardMscFinancialType
{
    [EnumMember(Value = "C")]
    Capital,
    [EnumMember(Value = "F")]
    Fee,
    [EnumMember(Value = "I")]
    Interest,
    [EnumMember(Value = "B")]
    Tax1,
    [EnumMember(Value = "K")]
    Tax2,
    [EnumMember(Value = "M")]
    Payment,
    [EnumMember(Value = "P")]
    Point
}

public enum CardMscTxnEffect
{
    [EnumMember(Value = "D")]
    Debit,
    [EnumMember(Value = "C")]
    Credit,
    [EnumMember(Value = "P")]
    CreditPoint,
    [EnumMember(Value = "M")]
    DebitPoint
}

public enum CardMscTxnSource
{
    [EnumMember(Value = "O")]
    Onus,
    [EnumMember(Value = "N")]
    Domestic,
    [EnumMember(Value = "V")]
    Visa,
    [EnumMember(Value = "M")]
    Mastercard
}

public enum CardMscTxnRegion
{
    [EnumMember(Value = "O")]
    Onus,
    [EnumMember(Value = "D")]
    Domestic,
    [EnumMember(Value = "I")]
    International,
    [EnumMember(Value = "R")]
    IntraRegional
}

public enum CardMscTerminalType
{
    [EnumMember(Value = "PO")]
    Pos,
    [EnumMember(Value = "AT")]
    Atm,
    [EnumMember(Value = "EP")]
    Epos,
    [EnumMember(Value = "IN")]
    InternetBanking,
    [EnumMember(Value = "IV")]
    Ivr,
    [EnumMember(Value = "VP")]
    VirtualPos,
    [EnumMember(Value = "CT")]
    Crt,
    [EnumMember(Value = "BR")]
    BranchScreen,
    [EnumMember(Value = "UA")]
    UnattendedPos,
    [EnumMember(Value = "VL")]
    Validator,
    [EnumMember(Value = "KI")]
    Kiosk
}

public enum CardMscChannelCode
{
    [EnumMember(Value = "SYS")]
    System,
    [EnumMember(Value = "OCN")]
    OceanFrontend
}

public enum CardMscIsTxnSettle
{
    [EnumMember(Value = "Y")]
    Settled,
    [EnumMember(Value = "N")]
    Unsettled
}

public enum CardMscTxnStat
{
    [EnumMember(Value = "N")]
    Normal,
    [EnumMember(Value = "R")]
    Reverse,
    [EnumMember(Value = "V")]
    Void,
    [EnumMember(Value = "E")]
    Expired
}

public enum CardMscIsSuccessfulTxn
{
    [EnumMember(Value = "Y")]
    Successful,
    [EnumMember(Value = "N")]
    Unsuccessful
}

public enum CardMscTxnOrigin
{
    [EnumMember(Value = "0")]
    Authorization,
    [EnumMember(Value = "1")]
    UserGenerated,
    [EnumMember(Value = "2")]
    SystemGenerated,
    [EnumMember(Value = "3")]
    ChannelGenerated
}

#endregion

#region CARD BKM

public partial class CardBkm
{
    public CardBkmHeader Header { get; set; } = new();
    public CardBkmDetail Detail { get; set; } = new();
    public CardBkmFooter Footer { get; set; } = new();
}

public partial class CardBkmHeader
{
    public CardBkmHeaderCode HeaderCode { get; set; }
    public string FileDate { get; set; } = default!;
    public string FileNo { get; set; } = default!;
    public string FileVersionNumber { get; set; } = default!;
}

public partial class CardBkmDetail
{
    public int TransactionDate { get; set; }
    public int TransactionTime { get; set; }
    public int ValueDate { get; set; }
    public int EndOfDayDate { get; set; }
    public string CardNo { get; set; } = default!;
    public long OceanTxnGuid { get; set; }
    public long OceanMainTxnGuid { get; set; }
    public string BranchId { get; set; } = default!;
    public string Rrn { get; set; } = default!;
    public string Arn { get; set; } = default!;
    public string ProvisionCode { get; set; } = default!;
    public int Stan { get; set; }
    public string MemberRefNo { get; set; } = default!;
    public long TraceId { get; set; }
    public int Otc { get; set; }
    public int Ots { get; set; }
    public CardBkmTxnInstallType TxnInstallType { get; set; }
    public string BankingTxnCode { get; set; } = default!;
    public string TxnDescription { get; set; } = default!;
    public string MerchantName { get; set; } = default!;
    public string MerchantCity { get; set; } = default!;
    public string MerchantState { get; set; } = default!;
    public string MerchantCountry { get; set; } = default!;
    public CardBkmFinancialType FinancialType { get; set; }
    public CardBkmTxnEffect TxnEffect { get; set; }
    public CardBkmTxnSource TxnSource { get; set; }
    public CardBkmTxnRegion TxnRegion { get; set; }
    public CardBkmTerminalType TerminalType { get; set; }
    public CardBkmChannelCode ChannelCode { get; set; }
    public string TerminalId { get; set; } = default!;
    public string MerchantId { get; set; } = default!;
    public int Mcc { get; set; }
    public int AcquirerId { get; set; }
    public int SecurityLevelIndicator { get; set; }
    public CardBkmIsTxnSettle IsTxnSettle { get; set; }
    public CardBkmTxnStat TxnStat { get; set; }
    public string ResponseCode { get; set; } = default!;
    public CardBkmIsSuccessfulTxn IsSuccessfulTxn { get; set; }
    public CardBkmTxnOrigin TxnOrigin { get; set; }
    public int InstallCount { get; set; }
    public int InstallOrder { get; set; }
    public string OperatorCode { get; set; } = default!;
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
    public string PointType { get; set; } = default!;
    public decimal BcPoint { get; set; }
    public decimal McPoint { get; set; }
    public decimal CcPoint { get; set; }
    public decimal BcPointAmount { get; set; }
    public decimal McPointAmount { get; set; }
    public decimal CcPointAmount { get; set; }
}

public partial class CardBkmFooter
{
    public CardBkmFooterCode FooterCode { get; set; }
    public string FileDate { get; set; } = default!;
    public long TxnCount { get; set; }
}

public enum CardBkmHeaderCode
{
    [EnumMember(Value = "H")]
    Header
}

public enum CardBkmFooterCode
{
    [EnumMember(Value = "F")]
    Footer
}

public enum CardBkmTxnInstallType
{
    [EnumMember(Value = "N")]
    Normal,
    [EnumMember(Value = "Y")]
    Installment,
    [EnumMember(Value = "I")]
    InstallmentWithInterest,
    [EnumMember(Value = "T")]
    InstallmentWithInstructionOrCashAdvance
}

public enum CardBkmFinancialType
{
    [EnumMember(Value = "C")]
    Capital,
    [EnumMember(Value = "F")]
    Fee,
    [EnumMember(Value = "I")]
    Interest,
    [EnumMember(Value = "B")]
    Tax1,
    [EnumMember(Value = "K")]
    Tax2,
    [EnumMember(Value = "M")]
    Payment,
    [EnumMember(Value = "P")]
    Point
}

public enum CardBkmTxnEffect
{
    [EnumMember(Value = "D")]
    Debit,
    [EnumMember(Value = "C")]
    Credit,
    [EnumMember(Value = "P")]
    CreditPoint,
    [EnumMember(Value = "M")]
    DebitPoint
}

public enum CardBkmTxnSource
{
    [EnumMember(Value = "O")]
    Onus,
    [EnumMember(Value = "N")]
    Domestic,
    [EnumMember(Value = "V")]
    Visa,
    [EnumMember(Value = "M")]
    Mastercard
}

public enum CardBkmTxnRegion
{
    [EnumMember(Value = "O")]
    Onus,
    [EnumMember(Value = "D")]
    Domestic,
    [EnumMember(Value = "I")]
    International,
    [EnumMember(Value = "R")]
    IntraRegional
}

public enum CardBkmTerminalType
{
    [EnumMember(Value = "PO")]
    Pos,
    [EnumMember(Value = "AT")]
    Atm,
    [EnumMember(Value = "EP")]
    Epos,
    [EnumMember(Value = "IN")]
    InternetBanking,
    [EnumMember(Value = "IV")]
    Ivr,
    [EnumMember(Value = "VP")]
    VirtualPos,
    [EnumMember(Value = "CT")]
    Crt,
    [EnumMember(Value = "BR")]
    BranchScreen,
    [EnumMember(Value = "UA")]
    UnattendedPos,
    [EnumMember(Value = "VL")]
    Validator,
    [EnumMember(Value = "KI")]
    Kiosk
}

public enum CardBkmChannelCode
{
    [EnumMember(Value = "SYS")]
    System,
    [EnumMember(Value = "OCN")]
    OceanFrontend
}

public enum CardBkmIsTxnSettle
{
    [EnumMember(Value = "Y")]
    Settled,
    [EnumMember(Value = "N")]
    Unsettled
}

public enum CardBkmTxnStat
{
    [EnumMember(Value = "N")]
    Normal,
    [EnumMember(Value = "R")]
    Reverse,
    [EnumMember(Value = "V")]
    Void,
    [EnumMember(Value = "E")]
    Expired
}

public enum CardBkmIsSuccessfulTxn
{
    [EnumMember(Value = "Y")]
    Successful,
    [EnumMember(Value = "N")]
    Unsuccessful
}

public enum CardBkmTxnOrigin
{
    [EnumMember(Value = "0")]
    Authorization,
    [EnumMember(Value = "1")]
    UserGenerated,
    [EnumMember(Value = "2")]
    SystemGenerated,
    [EnumMember(Value = "3")]
    ChannelGenerated
}

#endregion