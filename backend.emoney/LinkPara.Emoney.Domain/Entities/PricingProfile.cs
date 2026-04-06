using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class PricingProfile : AuditEntity
{
    public string Name { get; set; }
    public PricingProfileStatus Status { get; set; }
    public DateTime ActivationDateStart { get; set; }
    public DateTime? ActivationDateEnd { get; set; }
    public TransferType TransferType { get; set; }
    public int? BankCode { get; set; }
    public Bank Bank { get; set; }
    public string CurrencyCode { get; set; }
    public Currency Currency { get; set; }
    public List<PricingProfileItem> PricingProfileItems { get; set; }
    public CardType CardType { get; set; }
}
