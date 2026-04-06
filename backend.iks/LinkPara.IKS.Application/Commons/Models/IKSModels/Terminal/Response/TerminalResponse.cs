using LinkPara.IKS.Application.Commons.Mappings;

namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Response
{
    public class TerminalResponse : IMapFrom<IKSTerminal>
    {
        public string GlobalMerchantId { get; set; }
        public string PspMerchantId { get; set; }
        public string TerminalId { get; set; }
        public string ReferenceCode { get; set; }
        public string StatusCode { get; set; }
        public string ServiceProviderPspMerchantId { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseCodeExplanation { get; set; }
    }
}
