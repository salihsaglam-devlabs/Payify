using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Commons.Models.DueProfiles;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.MerchantDues;

public class MerchantDueDto : IMapFrom<MerchantDue>
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid DueProfileId { get; set; }
    public DueProfileDto DueProfile { get; set; }
    public int TotalExecutionCount { get; set; }
    public DateTime LastExecutionDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public DateTime CreateDate { get; set; }
}