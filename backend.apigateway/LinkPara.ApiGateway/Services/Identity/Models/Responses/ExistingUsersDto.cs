namespace LinkPara.ApiGateway.Services.Identity.Models.Responses;

public class ExistingUsersDto
{
    public bool IsExists { get; set; }
    public List<string> UserNames { get; set; }
}