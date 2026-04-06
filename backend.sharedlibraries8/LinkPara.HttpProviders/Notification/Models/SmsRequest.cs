namespace LinkPara.HttpProviders.Notification.Models;

public class SmsRequest
{
    public string TemplateName  { get; set; }
    public Dictionary<string, string> TemplateParameters { get; set; }
    public string[] To { get; set; }
    public bool IsOtp { get; set; }
}