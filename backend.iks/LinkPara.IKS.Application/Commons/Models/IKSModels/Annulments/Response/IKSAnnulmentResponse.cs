namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Response
{
    public class IKSAnnulmentResponse
    {
        public IKSAnnulment annulment { get; set; }
    }
    public class IKSAnnulment
    {
        public string annulmentId { get; set; }
        public string globalMerchantId { get; set; }
        public string code { get; set; }
        public string date { get; set; }
        public string activityType { get; set; }
        public string informType { get; set; }
        public string explanation { get; set; }
        public string personnelName { get; set; }
        public string ownerIdentityNo { get; set; }
        public string partner2IdentityNo { get; set; }
        public string partner3IdentityNo { get; set; }
        public string partner4IdentityNo { get; set; }
        public string partner5IdentityNo { get; set; }
    }
}
