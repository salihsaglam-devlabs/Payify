

namespace LinkPara.HttpProviders.IKS.Models.Response
{
    public class AnnulmentsQueryResponse
    {
        public List<AnnulmentDto> Annulments { get; set; }
    }
    public class AnnulmentDto
    {
        public string AnnulmentId { get; set; }
        public string GlobalMerchantId { get; set; }
        public string Code { get; set; }
        public string Date { get; set; }
        public string ActivityType { get; set; }
        public string InformType { get; set; }
        public string OwnerIdentityNo { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }
        public bool OwnAnnulment { get; set; }
        public string TaxNo { get; set; }
        public string AnnulmentCodeDescription { get; set; }
        public string OwnAnnulmentDescription { get; set; }
    }
}
