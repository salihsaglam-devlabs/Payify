using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class ChargebackDocument : AuditEntity
{
    public Guid ChargebackId { get; set; }
    public Guid TransactionId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string Description { get; set; }
    public string FileName { get; set; }
}

 