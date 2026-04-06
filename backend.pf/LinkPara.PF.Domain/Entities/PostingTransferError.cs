using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;
public class PostingTransferError : AuditEntity
{
    public DateTime PostingDate { get; set; }
    public Guid MerchantTransactionId { get; set; }
    public MerchantTransaction MerchantTransaction { get; set; }
    public Guid? MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public TransferErrorCategory TransferErrorCategory { get; set; }
    public string ErrorMessage { get; set; }
    public string StackTrace { get; set; }
}

