using LinkPara.HttpProviders.PF.Models.Enums;

namespace LinkPara.HttpProviders.PF.Models.Request
{
    public class UpdateMerchantIKSModel
    {
        public Guid Id { get; set; }
        public string GlobalMerchantId { get; set; }
        public MerchantStatus MerchantStatus { get; set; }
    }
}
