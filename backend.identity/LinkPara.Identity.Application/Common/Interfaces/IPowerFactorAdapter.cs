using LinkPara.Identity.Application.Common.Models.PowerFactorModels.Request;
using LinkPara.Identity.Application.Common.Models.PowerFactorModels.Response;

namespace LinkPara.Identity.Application.Common.Interfaces
{
    public interface IPowerFactorAdapter
    {
        public Task<GenerateActivationOtpResponse> GetActivationOtpAsync(GenerateActivationOtpRequest request);
        public Task<VerifyLoginOtpResponse> VerifyLoginOtpAsync(VerifyLoginOtpRequest request);
    }
}