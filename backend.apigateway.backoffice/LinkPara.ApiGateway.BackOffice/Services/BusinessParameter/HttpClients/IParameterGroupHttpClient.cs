using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients
{
    public interface IParameterGroupHttpClient
    {
        Task SaveAsync(SaveParameterGroupDto request);
        Task<PaginatedList<ParameterGroupDto>> GetParameterGroupsAsync(GetAllParameterGroupRequest request);
        Task<ParameterGroupDto> GetParameterGroupByIdAsync(Guid id);
        Task<ParameterGroupDto> UpdateAsync(UpdateParameterGroupRequest request);
        Task DeleteParameterGroupAsync(Guid id);
    }
}
