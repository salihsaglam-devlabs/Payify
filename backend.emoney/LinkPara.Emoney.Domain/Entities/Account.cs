using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class Account : AuditEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string IdentityNumber { get; set; }
    public Guid CustomerId { get; set; }
    public AccountType AccountType { get; set; }
    public AccountKycLevel AccountKycLevel { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public DateTime OpeningDate { get; set; }
    public DateTime ClosingDate { get; set; }
    public DateTime ReopeningDate { get; set; }
    public DateTime SuspendedDate { get; set; }
    public DateTime KycChangeDate { get; set; }
    public string ChangeReason { get; set; }
    public bool IsCommercial { get; set; }
    public int CustomerNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public bool IsAddressConfirmed { get; set; }
    public List<AccountUser> AccountUsers { get; set; }
    public List<Wallet> Wallets { get; set; }
    public AccountFinancialInformation AccountFinancialInformation { get; set; }
    public string VirtualIban { get; set; }
    public Guid ParentAccountId { get; set; }
    public virtual CompanyPool CompanyPool { get; set; }
    public bool IsOpenBankingPermit { get; set; }
    public string Profession { get; set; }
    public bool IsNameMaskingEnabled { get; set; }
    public DeclarationStatus DeclarationStatus { get; set; }
}
