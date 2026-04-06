using LinkPara.HttpProviders.KKB.Models;

namespace LinkPara.HttpProviders.KKB
{
    public interface IKKBService
    {
        Task<ValidateIbanResponse> ValidateIban(ValidateIbanRequest request);
    }
}
