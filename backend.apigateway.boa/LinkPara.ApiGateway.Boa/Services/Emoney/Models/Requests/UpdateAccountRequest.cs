using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class UpdateAccountRequest
{
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public string ChangeReason { get; set; }
}
