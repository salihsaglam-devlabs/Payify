namespace LinkPara.HttpProviders.IKS.Models.Request
{
    public class IKSUpdateMerchantRequest : IKSSaveMerchantRequest
    {
        public string GlobalMerchantId { get; set; }
        public string StatusCode { get; set; }
    }
}
