namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

public class TemplateAttachmentDto
{
    public byte[] FileContent { get; set; }
    public Guid Id { get; set; }
    public string FilePath { get; set; }
    public string OriginalFileName { get; set; }
    public string MimeType { get; set; }
}