using LinkPara.ApiGateway.Merchant.Services.BusinessParameter.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.BusinessParameter.HttpClients;

public interface IParameterHttpClient
{
    Task<List<ParameterDto>> GetParametersAsync(string groupCode);
}
