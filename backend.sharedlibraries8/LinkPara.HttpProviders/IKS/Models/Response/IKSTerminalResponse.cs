

namespace LinkPara.HttpProviders.IKS.Models.Response
{
    public class IKSTerminalResponse
    {
        public string GlobalMerchantId { get; set; }
        public string PspMerchantId { get; set; }
        public string TerminalId { get; set; }
        public string ServiceProviderPspMerchantId { get; set; }
        public string ReferenceCode { get; set; }
        public string StatusCode { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseCodeExplanation { get; set; }
    }
}
