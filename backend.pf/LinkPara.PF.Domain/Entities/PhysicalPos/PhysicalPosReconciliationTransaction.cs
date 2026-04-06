using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities.PhysicalPos;

public class PhysicalPosReconciliationTransaction : AuditEntity
{
    public string PaymentId { get; set; }
    public string BatchId { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string Currency { get; set; }
    public string MerchantId { get; set; }
    public string TerminalId { get; set; }
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public int Installment { get; set; }
    public string MaskedCardNo { get; set; }
    public string BinNumber { get; set; }
    public string ProvisionNo { get; set; }
    public string AcquirerResponseCode { get; set; }
    public int InstitutionId { get; set; }
    public string Vendor { get; set; }
    public string Rrn { get; set; }
    public string Stan { get; set; }
    public string PosEntryMode { get; set; }
    public string PinEntryInfo { get; set; }
    public string BankRef { get; set; }
    public string OriginalRef { get; set; }
    public Guid PfMerchantId { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string SerialNumber { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
    public Guid MerchantTransactionId { get; set; }
    public Guid UnacceptableTransactionId { get; set; }
    public Guid PhysicalPosEodId { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}