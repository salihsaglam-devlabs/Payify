using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Commons.Models.SubMerchants;

public class SubMerchantLimitDto : IMapFrom<SubMerchantLimit>
{
    public Guid Id { get; set; }
    public TransactionLimitType TransactionLimitType { get; set; }
    public Period Period { get; set; }
    public LimitType LimitType { get; set; }
    public int? MaxPiece { get; set; }
    public decimal? MaxAmount { get; set; }
    public string Currency { get; set; }
    public Guid SubMerchantId { get; set; }
    public string CreatedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
