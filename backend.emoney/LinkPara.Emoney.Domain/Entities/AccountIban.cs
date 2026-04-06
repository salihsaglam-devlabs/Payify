using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class AccountIban : AuditEntity
{
    public string IdentityNo { get; set; }
    public string Iban { get; set; }
}