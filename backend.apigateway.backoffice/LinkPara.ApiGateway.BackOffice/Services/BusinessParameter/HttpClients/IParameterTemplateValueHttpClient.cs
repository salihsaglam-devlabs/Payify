using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients
{
    public interface IParameterTemplateValueHttpClient
    {
        Task SaveAsync(SaveParameterTemplateValueDto request);
        Task<ParameterTemplateValueDto> GetParameterTemplateValueByIdAsync(Guid id);
        Task<ParameterTemplateValueDto> UpdateAsync(UpdateParameterTemplateValueRequest request);
        Task DeleteParameterTemplateValueAsync(Guid id);
        Task<PaginatedList<ParameterTemplateValueDto>> GetParameterTemplateValuesAsync(GetAllParameterTemplateValueRequest request);
    }
}
