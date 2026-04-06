namespace LinkPara.HttpProviders.Approval.Models;

public class RequestCashbackDto
{

    public string DisplayName { get; set; }
    public string Body { get; set; }
    public int Status { get; set; }
    public Guid CheckerUserId { get; set; }
    public Guid SecondCheckerUserId { get; set; }
}
