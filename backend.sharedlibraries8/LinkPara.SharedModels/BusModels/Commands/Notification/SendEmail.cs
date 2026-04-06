namespace LinkPara.SharedModels.BusModels.Commands.Notification;

public class SendEmail
{
    public string ToEmail { get; set; }
    public string TemplateName { get; set; }
    public Dictionary<string, string> DynamicTemplateData { get; set; }
    public List<Dictionary<string, string>> AttachmentList { get; set; }
    public byte[] Attachment { get; set; }
}