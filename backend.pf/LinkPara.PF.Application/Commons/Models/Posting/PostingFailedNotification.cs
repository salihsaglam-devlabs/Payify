namespace LinkPara.PF.Application.Commons.Models.Posting;

public class PostingFailedNotification
{
    public List<string> Development { get; set; }
    public List<string> Tenant { get; set; }
    public bool IsNotificationActiveForTenant { get; set; }
    public bool IsNotificationActiveForDevelopment { get; set; }
}