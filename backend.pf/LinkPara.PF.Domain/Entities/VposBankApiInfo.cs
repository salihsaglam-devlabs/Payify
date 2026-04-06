using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;
public class VposBankApiInfo : AuditEntity
{
    public Guid VposId { get; set; }
    public Vpos Vpos { get; set; }
    public Guid KeyId { get; set; }
    public BankApiKey Key { get; set; }
    public string Value { get; set; }
}