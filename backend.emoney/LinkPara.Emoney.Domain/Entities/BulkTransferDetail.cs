using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class BulkTransferDetail : AuditEntity
{
    public Guid BulkTransferId { get; set; }
    public string FullName { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string Description { get; set; }
    public string Receiver { get; set; }
    public BulkTransferDetailStatus BulkTransferDetailStatus { get; set; }
    public Guid? TransactionId { get; set; }
    public virtual BulkTransfer BulkTransfer { get; set; }
    public virtual Transaction Transaction { get; set; }
    public string ExceptionMessage { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvAmount { get; set; }

}
