namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses
{
    public class InstallmentsManualPaymentPageResponse
    {
        public bool Is3DRequired { get; set; }
        public List<int> AvailableInstallmentCounts { get; set; }
    }
}
