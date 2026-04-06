using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class AccountDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string IdentityNumber { get; set; }
    public Guid CustomerId { get; set; }
    public int CustomerNumber { get; set; }
    public AccountType AccountType { get; set; }
    public AccountKycLevel AccountKycLevel { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public DateTime OpeningDate { get; set; }
    public DateTime ClosingDate { get; set; }
    public DateTime SuspendedDate { get; set; }
    public DateTime KycChangeDate { get; set; }
    public string ChangeReason { get; set; }
    public bool IsCommercial { get; set; }
    public DateTime BirthDate { get; set; }
    public string VirtualIban { get; set; }

    public Guid Id { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public Guid ParentAccountId { get; set; }
    public string Profession { get; set; }
}
