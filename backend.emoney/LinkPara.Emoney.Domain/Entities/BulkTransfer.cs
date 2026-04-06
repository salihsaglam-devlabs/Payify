using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MassTransit;

namespace LinkPara.Emoney.Domain.Entities;

public class BulkTransfer : AuditEntity
{
    public BulkTransferStatus BulkTransferStatus { get; set; }
    public string FileName { get; set; }
    public int ReferenceNumber { get; set; }
    public Guid? ActionUser { get; set; }
    public string ActionUserName { get; set; }
    public DateTime ActionDate { get; set; }
    public string SenderWalletNumber { get; set; }
    public BulkTransferType BulkTransferType { get; set; }
    public Guid AccountId { get; set; }
    public virtual List<BulkTransferDetail> BulkTransferDetails { get; set; }
}
