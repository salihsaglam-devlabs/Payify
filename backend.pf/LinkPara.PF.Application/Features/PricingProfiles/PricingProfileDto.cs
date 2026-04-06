using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.PricingProfiles;

public class PricingProfileDto : IMapFrom<PricingProfile>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime ActivationDate { get; set; }
    public ProfileStatus ProfileStatus { get; set; }
    public ProfileType ProfileType { get; set; }
    public string CurrencyCode { get; set; }
    public decimal PerTransactionFee { get; set; }
    public string PricingProfileNumber { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsPaymentToMainMerchant { get; set; }
    public List<PricingProfileItemDto> PricingProfileItems { get; set; } = new List<PricingProfileItemDto>();
}
