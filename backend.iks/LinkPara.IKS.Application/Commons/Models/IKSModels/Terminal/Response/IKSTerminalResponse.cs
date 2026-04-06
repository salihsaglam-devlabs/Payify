namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Response
{
    public class IKSTerminalResponse
    {
        public IKSTerminal terminal { get; set; }
    }
    public class IKSTerminal
    {
        public string referenceCode { get; set; }
        public string globalMerchantId { get; set; }
        public string pspMerchantId { get; set; }
        public string terminalId { get; set; }
        public string statusCode { get; set; }
        public string type { get; set; }
        public string brandCode { get; set; }
        public string model { get; set; }
        public string serialNo { get; set; }
        public int ownerPspNo { get; set; }
        public string ownerTerminalId { get; set; }
        public string brandSharing { get; set; }
        public string pinPad { get; set; }
        public string contactless { get; set; }
        public string connectionType { get; set; }
        public string virtualPosUrl { get; set; }
        public string hostingTaxNo { get; set; }
        public string hostingTradeName { get; set; }
        public string hostingUrl { get; set; }
        public string paymentGwTaxNo { get; set; }
        public string paymentGwTradeName { get; set; }
        public string paymentGwUrl { get; set; }
        public int serviceProviderPspNo { get; set; }
        public string fiscalNo { get; set; }
        public string techPos { get; set; }
        public string serviceProviderPspMerchantId { get; set; }
        public string pfMainMerchantId { get; set; }
        public string responseCode { get; set; }
        public string responseCodeExplanation { get; set; }
    }
}
