
namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Models
{
    public class PosnetPosInfo : IPosInfo
    {
        public string MerchantId { get; set; }
        public string TerminalId { get; set; }
        public string PosnetId { get; set; }
        public string MrcPfId { get; set; }
        public string XmlServiceUrl { get; set; }
        public string EncryptionKey { get; set; }
    }
}
