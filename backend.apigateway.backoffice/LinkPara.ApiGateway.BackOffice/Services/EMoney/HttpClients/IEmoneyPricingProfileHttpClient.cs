using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface IEmoneyPricingProfileHttpClient
{
    Task<EmoneyPricingProfileDto> GetByIdAsync(Guid id);

    Task<PaginatedList<EmoneyPricingProfileDto>> GetListAsync(GetPricingProfileListRequest request);

    Task SaveAsync(EmoneySavePricingProfileRequest request);

    Task UpdateAsync(EmoneyUpdatePricingProfileRequest request);

    Task UpdateProfileItemAsync(PricingProfileItemUpdateModel request);

    Task DeleteAsync(Guid id);
}
