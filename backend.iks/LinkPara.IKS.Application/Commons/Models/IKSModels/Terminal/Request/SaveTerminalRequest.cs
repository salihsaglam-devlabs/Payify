using LinkPara.IKS.Application.Commons.Mappings;
using LinkPara.IKS.Application.Features.Terminal.Command.SaveTerminal;

namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Request
{
    public class SaveTerminalRequest : IMapFrom<SaveTerminalCommand>
    {
        public Guid MerchantId { get; set; }
        public string GlobalMerchantId { get; set; }
        public string PspMerchantId { get; set; }
        public string TerminalId { get; set; }
        public string StatusCode { get; set; }
        public string Type { get; set; }
        public int OwnerPspNo { get; set; }
        public string OwnerTerminalId { get; set; }
        public string BrandSharing { get; set; }
        public string VirtualPosUrl { get; set; }
        public string HostingTaxNo { get; set; }
        public string PaymentGwTaxNo { get; set; }
        public int ServiceProviderPspNo { get; set; }
        public int TechPos { get; set; }
    }
}
