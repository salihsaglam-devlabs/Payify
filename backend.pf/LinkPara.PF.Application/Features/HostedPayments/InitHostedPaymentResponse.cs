using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.HostedPayments;

public class InitHostedPaymentResponse : ResponseBase
{
    public ChannelStatus Status { get; set; }
    public string PaymentUrl { get; set; }
    public string TrackingId { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string LanguageCode { get; set; }
    public string ConversationId { get; set; }
    public HppPageViewType PageViewType { get; set; }
}