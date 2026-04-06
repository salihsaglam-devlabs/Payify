using LinkPara.ApiGateway.Services.KKB.Models.Request;
using LinkPara.ApiGateway.Services.KKB.Models.Response;
using LinkPara.HttpProviders.KKB.Models;

namespace LinkPara.ApiGateway.Services.KKB.HttpClients
{
    public interface IKKBHttpClient
    {
        Task<ValidateIbanResponse> ValidateIbanAsync(ValidateIbanRequest request);
        Task<InquireIbanResponse> IbanInquireAsync(InquireIbanRequest request);
    }
}
