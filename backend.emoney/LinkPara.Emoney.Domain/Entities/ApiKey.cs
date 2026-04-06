using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Emoney.Domain.Entities;

public class ApiKey: AuditEntity
{
    public Guid PartnerId { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
    public Partner Partner { get; set; }
}
