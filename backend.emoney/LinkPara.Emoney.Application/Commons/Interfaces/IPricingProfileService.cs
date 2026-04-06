using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Application.Features.PricingProfiles;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.DeletePricingProfile;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.SavePricingProfile;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.UpdatePricingProfile;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.UpdatePricingProfileItem;
using LinkPara.Emoney.Application.Features.PricingProfiles.Queries.GetPricingProfileList;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IPricingProfileService
{
    Task<PricingProfile> GetByIdAsync(Guid id);

    Task<PaginatedList<PricingProfileDto>> GetListAsync(GetPricingProfileListQuery request);

    Task SaveAsync(SavePricingProfileCommand request);

    Task UpdateAsync(UpdatePricingProfileCommand request);

    Task UpdateProfileItemAsync(UpdatePricingProfileItemCommand request);

    Task DeleteAsync(DeletePricingProfileItemCommand request);
    
    Task<CalculatePricingResponse> CalculatePricingAsync(CalculatePricingRequest request);

    Task CheckProfileStatus();

    Task<PricingProfileItemDto> GetCardTopupCommissionAsync();
}
