using LinkPara.SharedModels.Persistence;

namespace LinkPara.IKS.Domain.Entities
{
    public class TimeoutIKSTransaction : AuditEntity
    {
        public string Operation { get; set; }
        public string ResponseCode { get; set; }
        public bool IsSuccess { get; set; }
        public Guid MerchantId { get; set; }
        public Dictionary<string, object> RequestDetails { get; set; }
        public Dictionary<string, object> ResponseDetails { get; set; }
        public Dictionary<string, object> TimeoutReturnDetails { get; set; }
    }
}
