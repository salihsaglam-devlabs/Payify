using LinkPara.BusinessParameter.Application.Features.ParameterTemplates;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.DeleteParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.SaveParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.UpdateParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetAllParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetParameterTemplateById;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.BusinessParameter.Application.Commons.Interfaces;

public interface IParameterTemplateService
{
    Task<List<ParameterTemplateDto>> GetAllParameterTemplateByGroupCodeAsync(string groupCode);
    Task<ParameterTemplateDto> GetParameterTemplateAsync(string groupCode, string templateCode);
    Task SaveAsync(SaveParameterTemplateCommand request);
    Task<ParameterTemplateDto> GetByIdAsync(GetParameterTemplateByIdQuery request);
    Task DeleteAsync(DeleteParameterTemplateCommand command);
    Task<ParameterTemplateDto> UpdateAsync(UpdateParameterTemplateCommand command);
    Task<PaginatedList<ParameterTemplateDto>> GetAllParameterTemplateAsync(GetAllParameterTemplateQuery request);
}
