using LinkPara.HttpProviders.KKB.Models;

namespace LinkPara.ApiGateway.BackOffice.Services.KKB.HttpClients
{
    public interface IKKBHttpClient
    {
        Task<ValidateIbanResponse> ValidateIbanAsync(ValidateIbanRequest request);
    }
}
