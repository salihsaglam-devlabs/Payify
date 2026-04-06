using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;
public class PostingItem : AuditEntity
{
    public Guid MerchantId { get; set; }
    public int ErrorCount { get; set; }
    public int TotalCount { get; set; }
    public DateTime PostingDate { get; set; }
    public BatchStatus BatchStatus { get; set; }
}