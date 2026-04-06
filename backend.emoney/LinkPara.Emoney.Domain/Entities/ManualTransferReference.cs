using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class ManualTransferReference : AuditEntity
{
    public Guid TransactionId { get; set; }
    public Guid ApprovalRequestId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public DocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }
}