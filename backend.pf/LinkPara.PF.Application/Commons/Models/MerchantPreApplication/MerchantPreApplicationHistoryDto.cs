using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.MerchantPreApplication;

public class MerchantPreApplicationHistoryDto : IMapFrom<MerchantPreApplicationHistory>
{
    public Guid Id { get; set; }
    public Guid MerchantPreApplicationId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public ApplicationOperationType OperationType { get; set; }
    public DateTime OperationDate { get; set; }
    public string OperationNote { get; set; }
}