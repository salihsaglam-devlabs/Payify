namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetHppTransactionByTrackingIdRequest
{
    public string TrackingId { get; set; }
    public Guid MerchantId { get; set; }
}