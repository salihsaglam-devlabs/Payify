using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class UpdateDueProfileRequest
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public TimeInterval OccurenceInterval { get; set; }
    }
}
