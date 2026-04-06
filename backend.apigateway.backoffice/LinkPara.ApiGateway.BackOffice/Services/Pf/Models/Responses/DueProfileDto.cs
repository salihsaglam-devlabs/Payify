using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses
{
    public class DueProfileDto
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
