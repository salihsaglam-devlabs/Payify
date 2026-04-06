namespace LinkPara.SharedModels.BusModels.Commands.Notification;

public class SendSms
{
    public string TemplateName { get; set; }
    public Dictionary<string, string> TemplateParameters { get; set; }
    public string To { get; set; }
}