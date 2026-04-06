using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class GetTierLevelsRequest
    {
        public string CurrencyCode { get; set; }
        public RecordStatus? RecordStatus { get; set; }
        public bool IncludeCustoms { get; set; }
    }
}
