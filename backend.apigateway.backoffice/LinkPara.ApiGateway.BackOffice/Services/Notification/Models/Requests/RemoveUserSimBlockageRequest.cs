namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class RemoveUserSimBlockageRequest
{
    public string PhoneNumber { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public string OriginalFileName { get; set; }
    public Guid DocumentTypeId { get; set; }
}
