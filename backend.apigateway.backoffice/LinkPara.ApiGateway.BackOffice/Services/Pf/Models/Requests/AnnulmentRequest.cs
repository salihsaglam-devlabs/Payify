namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class AnnulmentRequest
    {
        public Guid MerchantId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string GlobalMerchantId { get; set; }
    }
}
