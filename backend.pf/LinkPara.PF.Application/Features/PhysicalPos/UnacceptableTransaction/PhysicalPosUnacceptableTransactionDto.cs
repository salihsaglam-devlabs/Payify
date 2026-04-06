using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;

namespace LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction;

public class PhysicalPosUnacceptableTransactionDto : IMapFrom<PhysicalPosUnacceptableTransaction>
{
    public Guid Id { get; set; }
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
    public string Gateway { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public UnacceptableTransactionStatus CurrentStatus {get; set;}
    public Guid PhysicalPosEodId { get; set; }
    public EndOfDayStatus EndOfDayStatus { get; set; }
}