using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;
public class ThreeDVerification : AuditEntity
{
    public TransactionType TransactionType { get; set; }
    public string OrderId { get; set; }
    public string CardToken { get; set; }
    public int InstallmentCount { get; set; }
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public VerificationStep CurrentStep { get; set; }
    public string CallbackUrl { get; set; }
    public Guid MerchantId { get; set; }
    public int IssuerBankCode { get; set; }
    public int AcquireBankCode { get; set; }
    public string MerchantCode { get; set; }
    public string SubMerchantCode { get; set; }
    public string SubMerchantTerminalNo { get; set; }
    public string BinNumber { get; set; }
    public int Currency { get; set; }
    public DateTime SessionExpiryDate { get; set; }
    public decimal BankCommissionAmount { get; set; }
    public decimal BankCommissionRate { get; set; }
    public int BankBlockedDayNumber { get; set; }
    public string Md { get; set; }
    public string MdStatus { get; set; }
    public string MdErrorMessage { get; set; }
    public string Xid { get; set; }
    public string Eci { get; set; }
    public string Cavv { get; set; }
    public string PayerTxnId { get; set; }
    public string TxnStat { get; set; }
    public string ThreeDStatus { get; set; }
    public string HashKey { get; set; }
    public DateTime BankTransactionDate { get; set; }
    public string BankResponseCode { get; set; }
    public string BankResponseDescription { get; set; }
    public string ConversationId { get; set; }
    public string BankPacket { get; set; }
    public bool IsPerInstallment { get; set; }
    public Guid CostProfileItemId { get; set; }
    public Guid VposId { get; set; }
    public Vpos Vpos { get; set; }
}
