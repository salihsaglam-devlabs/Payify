using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.PF.Application.Features.MerchantHistory
{
    public class MerchantHistoryDto : IMapFrom<LinkPara.PF.Domain.Entities.MerchantHistory>
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
