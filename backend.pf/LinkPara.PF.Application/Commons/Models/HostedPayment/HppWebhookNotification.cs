namespace LinkPara.PF.Application.Commons.Models.HostedPayment;

public class HppWebhookNotification
{
    public string TrackingId { get; set; }
    public Guid MerchantId { get; set; }
}