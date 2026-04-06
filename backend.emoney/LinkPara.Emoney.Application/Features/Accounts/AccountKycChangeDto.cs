using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Application.Features.Accounts;

public class AccountKycChangeDto : IMapFrom<AccountKycChange>
{
    public Guid AccountId { get; set; }
    public AccountTierValidation ValidationType { get; set; }
    public AccountKycLevel OldKycLevel { get; set; }
    public AccountKycLevel NewKycLevel { get; set; }
    public bool IsUpgraded { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
}