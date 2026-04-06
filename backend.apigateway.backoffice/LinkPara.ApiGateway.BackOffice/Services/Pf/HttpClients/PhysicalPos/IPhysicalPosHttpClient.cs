using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;

public interface IPhysicalPosHttpClient
{
    Task<PaginatedList<PhysicalPosDto>> GetAllAsync(GetAllPhysicalPosRequest request);
    Task<PhysicalPosDto> GetByIdAsync(Guid id);
    Task SaveAsync(SavePhysicalPosRequest request);
    Task UpdateAsync(UpdatePhysicalPosRequest request);
    Task DeletePhysicalPosAsync(Guid id);
    Task<MerchantPhysicalPosDto> GetByReferenceNumberAsync(string bkmReferenceNumber);
}
