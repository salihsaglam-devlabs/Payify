using LinkPara.SharedModels.Pagination;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public interface IDueProfileHttpClient
    {
        Task<PaginatedList<DueProfileDto>> GetFilterListAsync(GetFilterDueProfileRequest request);
        Task<DueProfileDto> GetByIdAsync(Guid id);
        Task UpdateAsync(UpdateDueProfileRequest request);
        Task CreateAsync(CreateDueProfileRequest request);
        Task DeleteAsync(Guid id);
    }
}
