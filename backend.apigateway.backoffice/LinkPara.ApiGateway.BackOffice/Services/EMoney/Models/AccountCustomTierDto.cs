using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models
{
    public class AccountCustomTierDto
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid TierLevelId { get; set; }
        public string AccountName { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public AccountType AccountType { get; set; }
    }
}
