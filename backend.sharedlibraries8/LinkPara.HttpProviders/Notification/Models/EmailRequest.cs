namespace LinkPara.HttpProviders.Notification.Models;

public class EmailRequest
{
    public string ToEmail { get; set; }
    public string TemplateName { get; set; }
    public Dictionary<string, string> DynamicTemplateData { get; set; }
    public List<Dictionary<string, string>> AttachmentList { get; set; }
}