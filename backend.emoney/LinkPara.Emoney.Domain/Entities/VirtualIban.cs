using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class VirtualIban : AuditEntity
{
    public string Iban { get; set; }
    public int BankCode { get; set; }
    public bool Available { get; set;}
}
