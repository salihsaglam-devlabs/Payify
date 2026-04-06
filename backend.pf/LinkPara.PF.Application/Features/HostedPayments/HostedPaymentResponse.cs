using LinkPara.PF.Application.Commons.Models.Payments.Response;

namespace LinkPara.PF.Application.Features.HostedPayments;

public class HostedPaymentResponse : ResponseBase
{
    public string ConversationId { get; set; }
    public string TrackingId { get; set; }
    public string OrderId { get; set; }
    public string OriginalOrderId { get; set; }
}