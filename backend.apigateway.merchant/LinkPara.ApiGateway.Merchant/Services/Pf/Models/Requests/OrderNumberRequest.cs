namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests
{
    public class OrderNumberRequest
    {
        public Guid MerchantId { get; set; }
        public string OrderNumber { get; set; }
        public bool IsLinkOrderId { get; set; }
    }
}
