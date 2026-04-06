using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class PricingProfile : AuditEntity, ITrackChange 
{
    public string Name { get; set; }
    public string PricingProfileNumber { get; set; }
    public DateTime ActivationDate { get; set; }
    public ProfileStatus ProfileStatus { get; set; }
    public ProfileType ProfileType { get; set; }
    public string CurrencyCode { get; set; }
    public Currency Currency { get; set; }
    public decimal PerTransactionFee { get; set; }
    public bool IsPaymentToMainMerchant { get; set; }
    public virtual List<PricingProfileItem> PricingProfileItems { get; set; }
}