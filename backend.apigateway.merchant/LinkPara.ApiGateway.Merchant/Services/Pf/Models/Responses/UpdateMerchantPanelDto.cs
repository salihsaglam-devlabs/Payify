namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses
{
    public class UpdateMerchantPanelDto
    {
        public string WebSiteUrl { get; set; }
        public string Iban { get; set; }
        public int BankCode { get; set; }
        public string CompanyEmail { get; set; }
    }
}
