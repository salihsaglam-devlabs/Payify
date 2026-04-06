namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Models
{
    public class FinansPosInfo : IPosInfo
    {
        public string MbrId { get; set; }
        public string MerchantId { get; set; }
        public string UserCode { get; set; }
        public string UserPass { get; set; }
        public string NonSecureUrl { get; set; }
        public string MerchantPass { get; set; }
        public string PaymentFacilitatorId { get; set; }
        public string PostUrl { get; set; }
    }
}
