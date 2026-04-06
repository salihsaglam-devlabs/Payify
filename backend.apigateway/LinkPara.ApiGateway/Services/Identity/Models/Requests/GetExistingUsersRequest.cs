namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class GetExistingUsersRequest
{
    public string Email { get; set; }
    public string UserName { get; set; }
}