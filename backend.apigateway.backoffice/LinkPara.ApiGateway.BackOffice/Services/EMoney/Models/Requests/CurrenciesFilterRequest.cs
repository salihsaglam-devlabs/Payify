using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class CurrenciesFilterRequest
    {
        public string Code { get; set; }
        public CurrencyType CurrencyType { get; set; }
        public RecordStatus? RecordStatus { get; set; }
    }
}
