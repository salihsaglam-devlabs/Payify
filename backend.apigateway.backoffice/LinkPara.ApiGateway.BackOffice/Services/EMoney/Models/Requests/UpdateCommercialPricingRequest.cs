using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class UpdateCommercialPricingRequest
{
    public Guid Id { get; set; }
    public int MaxDistinctSenderCount { get; set; }
    public int MaxDistinctSenderCountWithAmount { get; set; }
    public decimal MaxDistinctSenderAmount { get; set; }
    public PricingCommercialType PricingCommercialType { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal CommissionRate { get; set; }
    public string CurrencyCode { get; set; }
}