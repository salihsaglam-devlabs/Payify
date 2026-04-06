using LinkPara.IKS.Application.Commons.Mappings;
using LinkPara.IKS.Application.Features.Annuments.Command.SaveAnnulment;

namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Request
{
    public class SaveAnnulmentRequest : IMapFrom<SaveAnnulmentCommand>
    {
        public Guid MerchantId { get; set; }
        public string GlobalMerchantId { get; set; }
        public string Code { get; set; }
        public string Date { get; set; }
        public string ActivityType { get; set; }
        public string InformType { get; set; }
        public string Explanation { get; set; }
        public string PersonnelName { get; set; }
        public string OwnerIdentityNo { get; set; }
        public string Partner2IdentityNo { get; set; }
        public string Partner3IdentityNo { get; set; }
        public string Partner4IdentityNo { get; set; }
        public string Partner5IdentityNo { get; set; }
    }
}
