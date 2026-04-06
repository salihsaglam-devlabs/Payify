using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients
{
    public interface IParameterTemplateHttpClient
    {
        Task SaveAsync(SaveParameterTemplateDto request);
        Task<ParameterTemplateDto> GetParameterTemplateByIdAsync(Guid id);
        Task<ParameterTemplateDto> UpdateAsync(UpdateParameterTemplateRequest request);
        Task DeleteParameterTemplateAsync(Guid id);
        Task<PaginatedList<ParameterTemplateDto>> GetParameterTemplatesAsync(GetAllParameterTemplateRequest request);
        Task<List<ParameterTemplateDto>> GetParameterTemplatesByGroupCodeAsync(string groupCode);
    }
}