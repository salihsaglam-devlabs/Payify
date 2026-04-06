using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class UpdateMerchantLimitRequest : SaveMerchantLimitRequest
    {
        public Guid Id { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
