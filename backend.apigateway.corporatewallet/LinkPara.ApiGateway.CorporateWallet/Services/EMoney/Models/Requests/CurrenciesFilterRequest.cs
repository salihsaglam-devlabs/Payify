using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests
{
    public class CurrenciesFilterRequest
    {
        public string Code { get; set; }
        public CurrencyType CurrencyType { get; set; }
    }
}
