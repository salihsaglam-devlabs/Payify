using LinkPara.ApiGateway.Boa.Services.BusinessParameter.Models.Responses;

namespace LinkPara.ApiGateway.Boa.Services.BusinessParameter.HttpClients;

public interface IParameterHttpClient
{
    Task<List<ParameterDto>> GetParametersAsync(string groupCode);
}