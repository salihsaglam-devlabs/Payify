using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.HttpProviders.Emoney.Models
{
    public class PatchAccountDto
    {
        public AccountKycLevel AccountKycLevel { get; set; }
        public string IdentityNumber { get; set; }
        public string Email { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public AccountType AccountType { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public DateTime KycChangeDate { get; set; }
        public string ChangeReason { get; set; }
        public Guid CustomerId { get; set; }
        public bool IsAddressConfirmed { get; set; }
        public string Profession { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
