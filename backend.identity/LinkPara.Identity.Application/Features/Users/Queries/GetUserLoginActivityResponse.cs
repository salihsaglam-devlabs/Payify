namespace LinkPara.Identity.Application.Features.Users.Queries;

public class GetUserLoginActivityResponse
{
    public DateTime? LastSucceededLogin { get; set; }
    public DateTime? LastFailedLogin { get; set; }
    public string LastFailedLoginIPAddress { get; set; }
    public List<LoginActivityDto> LoginActivities { get; set; }
    public int FailedLoginCount { get; set; }
}
