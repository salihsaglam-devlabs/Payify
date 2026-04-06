using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class ActionMerchantReturnPoolRequest
    {
        public Guid MerchantReturnPoolId { get; set; }
        public ReturnStatus ReturnStatus { get; set; }
        public string RejectDescription { get; set; }
        public string RejectReason { get; set; }
    }
}