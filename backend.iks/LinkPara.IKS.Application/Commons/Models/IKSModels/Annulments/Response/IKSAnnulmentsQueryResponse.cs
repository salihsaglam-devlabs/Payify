

namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Response
{
    public class IKSAnnulmentsQueryResponse
    {
        public AnnulmentsResponse[] annulments { get; set; }
        public int totalCount { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }  
    }
    public class AnnulmentsResponse
    {
        public string annulmentId { get; set; }
        public string globalMerchantId { get; set; }
        public string code { get; set; }
        public string date { get; set; }
        public string activityType { get; set; }
        public string informType { get; set; }
        public string ownerIdentityNo { get; set; }
        public string createDate { get; set; }
        public string updateDate { get; set; }
        public bool ownAnnulment { get; set; }
        public string taxNo { get; set; }
    }
}
