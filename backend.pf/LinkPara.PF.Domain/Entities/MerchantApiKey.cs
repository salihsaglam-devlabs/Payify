using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantApiKey : AuditEntity, ITrackChange
{
    public string PublicKey { get; set; }
    public string PrivateKeyEncrypted { get; set; }
    
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
}