using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models;

public class UserSimBlockageDto
{
    public string PhoneNumber { get; set; }
    public bool IsSendOtp { get; set; }
    public DateTime ExpiryDate { get; set; }
    public Guid? DocumentId { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
