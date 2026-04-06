using LinkPara.ApiGateway.Merchant.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Emoney.Models.Requests
{
    public class CurrenciesFilterRequest
    {
        public string Code { get; set; }
        public CurrencyType CurrencyType { get; set; }
    }
}
