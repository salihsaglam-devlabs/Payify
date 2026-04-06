using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface INaceCodesHttpClient
{
    Task<NaceDto> GetByIdAsync(Guid id);
    Task<PaginatedList<NaceDto>> GetAllAsync(GetAllNaceCodesRequest request);
}