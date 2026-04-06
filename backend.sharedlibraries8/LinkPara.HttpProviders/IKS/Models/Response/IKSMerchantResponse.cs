namespace LinkPara.HttpProviders.IKS.Models.Response
{
    public class IKSMerchantResponse
    {
        public string GlobalMerchantId { get; set; }
        public string PspMerchantId { get; set; }
        public Additionalinfo[] AdditionalInfo { get; set; }
    }

    public class Additionalinfo
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
