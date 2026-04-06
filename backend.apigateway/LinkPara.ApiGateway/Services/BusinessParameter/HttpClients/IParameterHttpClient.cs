using LinkPara.ApiGateway.Services.BusinessParameter.Models.Request;
using LinkPara.ApiGateway.Services.BusinessParameter.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.BusinessParameter.HttpClients
{
    public interface IParameterHttpClient
    {
        Task<PaginatedList<ParameterDto>> GetAllParameterAsync(GetAllParameterRequest request);
        Task<List<ParameterDto>> GetParametersAsync(string groupCode);
        Task<List<ParameterDto>> GetProfessionParametersAsync();
        Task<List<ParameterDto>> GetCompanyInfoParametersAsync();
    }
}
