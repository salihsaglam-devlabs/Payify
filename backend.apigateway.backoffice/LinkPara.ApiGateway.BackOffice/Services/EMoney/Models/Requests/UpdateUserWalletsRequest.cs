

using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class UpdateUserWalletsRequest
    {
        public Guid UserId { get; set; }
        public bool IsBlockage { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
