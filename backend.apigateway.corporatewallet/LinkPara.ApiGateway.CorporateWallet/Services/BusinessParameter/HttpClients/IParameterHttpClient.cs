using LinkPara.ApiGateway.CorporateWallet.Services.BusinessParameter.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.BusinessParameter.HttpClients;

public interface IParameterHttpClient
{
    Task<List<ParameterDto>> GetParametersAsync(string groupCode);
}
