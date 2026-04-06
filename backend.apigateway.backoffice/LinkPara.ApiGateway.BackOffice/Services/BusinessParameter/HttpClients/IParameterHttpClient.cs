using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients;

public interface IParameterHttpClient
{
    Task<List<ParameterDto>> GetParametersAsync(string groupCode);
    Task SaveAsync(SaveParameterDto request);
    Task<PaginatedList<ParameterDto>> GetAllParameterAsync(GetAllParameterRequest request);
    Task<ParameterDto> GetParameterByIdAsync(Guid id);
    Task DeleteParameterAsync(Guid id);
    Task<ParameterDto> UpdateAsync(UpdateParameterRequest request);
}
