namespace LinkPara.Identity.Application.Features.Users.Queries;

public class ExistingUsersDto
{
    public bool IsExists { get; set; }
    public List<string> UserNames { get; set; }
}