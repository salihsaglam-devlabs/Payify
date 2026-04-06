using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantPreApplicationHistory : AuditEntity
{
    public Guid MerchantPreApplicationId { get; set; }
    public MerchantPreApplication MerchantPreApplication { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public ApplicationOperationType OperationType { get; set; }
    public DateTime OperationDate { get; set; }
    public string OperationNote { get; set; }
}