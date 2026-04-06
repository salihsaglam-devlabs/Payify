namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class UserFilterRequest
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public string UserName { get; set; }
}