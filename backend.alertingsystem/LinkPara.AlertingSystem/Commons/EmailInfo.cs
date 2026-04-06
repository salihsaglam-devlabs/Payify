namespace LinkPara.AlertingSystem.Commons;

public class EmailInfo
{
    public string Subject { get; set; }
    public List<string> ToEmailAddresses { get; set; }
    public string Body { get; set; }
}