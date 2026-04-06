namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses
{
    public class LinkRequirementResponse
    {
        public bool Is3dRequired { get; set; }
        public List<int> AvailableInstallmentCounts { get; set; }
    }
}
