using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.Emoney.Application.Features.Limits
{
    public class AccountCustomTierDto : IMapFrom<AccountCustomTier>
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string AccountName { get; set; }
        public Guid TierLevelId { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public AccountType AccountType { get; set; }
    }
}
