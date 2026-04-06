using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class AccountKycChange : AuditEntity
{
    public Guid AccountId { get; set; }
    public AccountTierValidation ValidationType { get; set; }
    public AccountKycLevel OldKycLevel { get; set; }
    public AccountKycLevel NewKycLevel { get; set; }
    public bool IsUpgraded { get; set; }
    public string ErrorMessage { get; set; }
}