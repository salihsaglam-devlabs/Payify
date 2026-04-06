namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class CustomerConfirmationResponse
{
    public Guid AccountId { get; set; }
    public Guid PushNotificationId { get; set; }
    public bool IsSuccess { get; set; }
}
