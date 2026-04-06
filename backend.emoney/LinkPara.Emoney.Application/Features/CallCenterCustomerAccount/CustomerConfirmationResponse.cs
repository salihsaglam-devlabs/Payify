
namespace LinkPara.Emoney.Application.Features.CallCenterCustomerAccount;

public class CustomerConfirmationResponse
{
    public Guid AccountId { get; set; }
    public Guid PushNotificationId { get; set; }
    public bool IsSuccess { get; set; }
}
