
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class CallCenterNotificationLog : AuditEntity
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public CallCenterConfirmationType ConfirmationType { get; set; }
    public CallCenterNotificationStatus Status { get; set; }
    public DateTime ExpireDate { get; set; }
    public string ErrorMessage { get; set; }
}
