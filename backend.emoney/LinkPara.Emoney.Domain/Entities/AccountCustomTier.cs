using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class AccountCustomTier : AuditEntity
{
    public Guid AccountId { get; set; }
    public Guid TierLevelId { get; set; }
    public string AccountName { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public TierLevel TierLevel { get; set; }
    public virtual Account Account { get; set; }
}