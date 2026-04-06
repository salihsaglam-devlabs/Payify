namespace LinkPara.HttpProviders.IKS.Models.Request
{
    public class IKSSaveAnnulmentRequest
    {
        public Guid MerchantId { get; set; }
        public string GlobalMerchantId { get; set; }
        public string Code { get; set; }
        public string CodeDescription { get; set; }
        public string Explanation { get; set; }
        public string OwnerIdentityNo { get; set; }
        public string Partner2IdentityNo { get; set; }
        public string Partner3IdentityNo { get; set; }
        public string Partner4IdentityNo { get; set; }
        public string Partner5IdentityNo { get; set; }
    }
}
