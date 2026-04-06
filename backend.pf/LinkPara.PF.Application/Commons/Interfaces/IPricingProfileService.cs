using LinkPara.PF.Application.Features.PricingProfiles;
using LinkPara.PF.Application.Features.PricingProfiles.Command.DeletePricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Command.SavePricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Command.UpdatePricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Queries.GetFilterPricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Queries.GetPricingProfileById;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IPricingProfileService
{
    Task SaveAsync(SavePricingProfileCommand request);
    Task DeleteAsync(DeletePricingProfileCommand request);
    Task<PricingProfileDto> GetByIdAsync(GetPricingProfileByIdQuery request);
    Task<PaginatedList<PricingProfileDto>> GetFilterListAsync(GetFilterPricingProfileQuery request);
    Task UpdateAsync(UpdatePricingProfileCommand request);
    void ValidateInstallment(List<PricingProfileItemDto> pricingProfileItems);
}
