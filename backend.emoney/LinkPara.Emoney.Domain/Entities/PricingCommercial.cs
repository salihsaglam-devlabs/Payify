using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class PricingCommercial : AuditEntity
{
    public int MaxDistinctSenderCount { get; set; }
    public int MaxDistinctSenderCountWithAmount { get; set; }
    public decimal MaxDistinctSenderAmount { get; set; }
    public PricingCommercialType PricingCommercialType { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal CommissionRate { get; set; }
    public string CurrencyCode { get; set; }
    public PricingCommercialStatus PricingCommercialStatus { get; set; }
}