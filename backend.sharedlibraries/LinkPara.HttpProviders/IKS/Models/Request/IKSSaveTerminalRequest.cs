

namespace LinkPara.HttpProviders.IKS.Models.Request
{
    public class IKSSaveTerminalRequest
    {
        public Guid MerchantId { get; set; }
        public string GlobalMerchantId { get; set; }
        public string PspMerchantId { get; set; }
        public string TerminalId { get; set; }
        public string OwnerTerminalId { get; set; }
        public string BrandSharing { get; set; }
        public int OwnerPspNo { get; set; }
        public string VirtualPosUrl { get; set; }
        public string HostingTaxNo { get; set; }
        public string PaymentGwTaxNo { get; set; }
        public int ServiceProviderPspNo { get; set; }
    }
}
