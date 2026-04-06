using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class UpdateAccountStatusRequest
{
    public AccountStatus AccountStatus { get; set; }
    public string ChangeReason { get; set; }
}