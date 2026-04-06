namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
public class GetUserLoginActivityResponse
{
    public DateTime? LastSucceededLogin { get; set; }
    public DateTime? LastFailedLogin { get; set; }
    public List<LoginActivityDto> LoginActivities { get; set; }
    public int FailedLoginCount { get; set; }
    public string LastFailedLoginIPAddress { get; set; }
}
