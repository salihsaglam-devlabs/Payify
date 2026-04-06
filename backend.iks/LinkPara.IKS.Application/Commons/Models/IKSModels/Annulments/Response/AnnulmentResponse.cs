using LinkPara.IKS.Application.Commons.Mappings;

namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Response
{
    public class AnnulmentResponse : IMapFrom<IKSAnnulment>
    {
        public string AnnulmentId { get; set; }
        public string GlobalMerchantId { get; set; }
        public string Code { get; set; }
        public string Date { get; set; }
        public string OwnerIdentityNo { get; set; }
    }
}
