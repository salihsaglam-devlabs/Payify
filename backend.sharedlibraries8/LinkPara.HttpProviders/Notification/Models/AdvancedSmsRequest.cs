namespace LinkPara.HttpProviders.Notification.Models;

public class AdvancedSmsRequest
{
    public Guid ReceiverId { get; set; }
    public string[] ToPhoneNumber { get; set; }
    public string TemplateName { get; set; }
    public string EventName { get; set; }
    public string PreferredLanguage { get; set; }
    public Dictionary<string, string> TemplateParameters { get; set; }
}