using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests
{
    public class GetTierLevelsRequest
    {
        public string CurrencyCode { get; set; }
        public RecordStatus? RecordStatus { get; set; }
        public bool IncludeCustoms { get; set; }
    }
}
