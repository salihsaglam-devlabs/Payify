using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Commons.Models.DueProfiles
{
    public class DueProfileDto : IMapFrom<DueProfile>
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DueType DueType { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public TimeInterval OccurenceInterval { get; set; }
        public bool IsDefault { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
