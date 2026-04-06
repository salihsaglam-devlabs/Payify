using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class CardLoyalty : AuditEntity
{
    public string Name { get; set; }
    public int BankCode { get; set; }
    public Bank Bank { get; set; }
}