using LinkPara.HttpProviders.KPS.Models;

namespace LinkPara.HttpProviders.KPS
{
    public interface IKpsService
    {
        Task<KpsResponse> GetPersonalInformation(KpsServiceRequest request);
        Task<ValidateIdentityResponse> ValidateIdentity (ValidateIdentityRequest request);
    }
}
