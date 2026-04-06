using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.DeleteParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.DeleteParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.SaveParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.UpdateParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetAllParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetParameterTemplateValueById;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.BusinessParameter.Application.Commons.Interfaces;

public interface IParameterTemplateValueService
{
    Task<List<ParameterTemplateValueDto>> GetAllParameterTemplateValuesByGroupCodeAsync(string groupCode, string parameterCode);
    Task<PaginatedList<ParameterTemplateValueDto>> GetAllParameterTemplateValuesAsync(GetAllParameterTemplateValueQuery request);
    Task<ParameterTemplateValueDto> GetParameterTemplateValueAsync(string groupCode, string parameterCode, string templateCode);
    Task SaveAsync(SaveParameterTemplateValueCommand request);
    Task<ParameterTemplateValueDto> GetByIdAsync(GetParameterTemplateValueByIdQuery request);
    Task DeleteAsync(DeleteParameterTemplateValueCommand command);
    Task<ParameterTemplateValueDto> UpdateAsync(UpdateParameterTemplateValueCommand command);
}
