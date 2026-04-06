using LinkPara.IKS.Application.Commons.Mappings;


namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Response
{
    public class MerchantResponse : IMapFrom<IKSMerchant>
    {
        public string GlobalMerchantId { get; set; }
        public string PspMerchantId { get; set; }
        public Additionalinfo[] AdditionalInfo { get; set; }
    }
}
