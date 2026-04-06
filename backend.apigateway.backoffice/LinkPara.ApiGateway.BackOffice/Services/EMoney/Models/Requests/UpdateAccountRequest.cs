using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class UpdateAccountRequest
{
    public AccountKycLevel AccountKycLevel { get; set; }
    public string IdentityNumber { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string Name { get; set; }
    public AccountType AccountType { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public string ChangeReason { get; set; }
    public Guid ParentAccountId { get; set; }
}
