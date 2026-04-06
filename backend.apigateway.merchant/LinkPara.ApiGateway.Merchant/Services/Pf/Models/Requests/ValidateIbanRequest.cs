using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests
{
    public class ValidateIbanRequest
    {
        public CompanyType CompanyType { get; set; }
        public string Iban { get; set; }
        public string IdentityNo { get; set; }
        public string TaxNumber { get; set; }
    }
}
