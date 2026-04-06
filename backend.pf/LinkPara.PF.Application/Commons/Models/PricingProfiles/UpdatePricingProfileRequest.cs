using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.PricingProfiles;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Commons.Models.PricingProfiles;

public class UpdatePricingProfileRequest : IMapFrom<PricingProfile>
{
    public string Name { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal PerTransactionFee { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public List<PricingProfileItemDto> PricingProfileItems { get; set; }
}
