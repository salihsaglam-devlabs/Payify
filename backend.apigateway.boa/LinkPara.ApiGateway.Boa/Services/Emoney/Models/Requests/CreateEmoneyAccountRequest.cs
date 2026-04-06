using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class CreateEmoneyAccountRequest
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public AccountType AccountType { get; set; }
    public AccountKycLevel AccountKycLevel { get; set; }
    public Guid IdentityUserId { get; set; }
    public Guid ParentAccountId { get; set; }
    public string IdentityNumber { get; set; }
    public string Profession { get; set; }
}
