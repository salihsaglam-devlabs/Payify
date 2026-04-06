namespace LinkPara.HttpProviders.Notification.Models;

public class AdvancedEmailRequest
{
    public Guid ReceiverId { get; set; }
    public string[] ToEmail { get; set; }
    public string TemplateName { get; set; }
    public string EventName { get; set; }
    public string PreferredLanguage { get; set; }
    public Dictionary<string, string> TemplateParameters { get; set; }
}