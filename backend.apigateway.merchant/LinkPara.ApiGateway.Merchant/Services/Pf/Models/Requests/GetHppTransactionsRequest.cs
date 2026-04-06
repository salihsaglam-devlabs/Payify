using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetHppTransactionsRequest : SearchQueryParams
{
    public Guid MerchantId { get; set; }
    public string TrackingId { get; set; }
    public string OrderId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public ChannelStatus? HppStatus { get; set; }
    public ChannelPaymentStatus? HppPaymentStatus { get; set; }
    public WebhookStatus? WebhookStatus { get; set; }
    public HppPageViewType? PageViewType { get; set; }
    public DateTime? ExpiryDateStart { get; set; }
    public DateTime? ExpiryDateEnd { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}