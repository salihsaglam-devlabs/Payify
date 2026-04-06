using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class WalletBlockageDocument : AuditEntity
{    
    public Guid WalletId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string Description { get; set; }
    public string FileName { get; set; }
}

 