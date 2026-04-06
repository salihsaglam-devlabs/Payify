using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantLimit : AuditEntity, ITrackChange
{
    public TransactionLimitType TransactionLimitType { get; set; }
    public Period Period { get; set; }
    public LimitType LimitType { get; set; }
    public int? MaxPiece { get; set; }
    public decimal? MaxAmount { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public string Currency { get; set; }
}
