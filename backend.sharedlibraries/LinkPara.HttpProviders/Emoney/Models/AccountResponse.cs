using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.Emoney.Models;

public class AccountResponse
{
    public Guid Id { get; set; }
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
    public DateTime SuspendedDate { get; set; }
    public DateTime KycChangeDate { get; set; }
    public string ChangeReason { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string ParentAccountId { get; set; }
}
