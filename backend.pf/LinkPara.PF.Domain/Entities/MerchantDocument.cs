using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.PF.Domain.Entities;

public class MerchantDocument : AuditEntity
{
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentName { get; set; }
    
    public Guid MerchantId { get; set; }
    public Guid? MerchantTransactionId { get; set; }
    public Merchant Merchant { get; set; }
}