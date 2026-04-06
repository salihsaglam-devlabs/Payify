using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses
{
    public class MerchantHistoryDto
    {
        public Guid MerchantId { get; set; }
        public PermissionOperationType PermissionOperationType { get; set; }
        public string NewData { get; set; }
        public string OldData { get; set; }
        public string Detail { get; set; }
        public DateTime CreateDate { get; set; }
        public string MerchantName { get; set; }
        public string CreatedNameBy { get; set; }
    }
}
