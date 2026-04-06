using LinkPara.HttpProviders.BusinessParameter.Models;

namespace LinkPara.HttpProviders.BusinessParameter;

public interface IParameterService
{
    Task<ParameterDto> GetParameterAsync(string groupCode, string parameterCode);
    Task<List<ParameterDto>> GetParametersAsync(string groupCode);
    Task<ParameterTemplateDto> GetParameterTemplateAsync(string groupCode, string templateCode);
    Task<ParameterTemplateValueDto> GetParameterTemplateValueAsync(string groupCode, string parameterCode, string templateCode);
    Task<List<ParameterTemplateValueDto>> GetAllParameterTemplateValuesAsync(string groupCode, string parameterCode);
}
