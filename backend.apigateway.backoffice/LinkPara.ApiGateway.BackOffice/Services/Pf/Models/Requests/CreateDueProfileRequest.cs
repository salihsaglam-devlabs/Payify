using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class CreateDueProfileRequest
    {
        public string Title { get; set; }
        public DueType DueType { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public TimeInterval OccurenceInterval { get; set; }
    }
}
