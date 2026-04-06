using LinkPara.HttpProviders.PF.Models.Request;

namespace LinkPara.HttpProviders.PF
{
    public interface IMerchantService
    {
        Task UpdateMerchantIKSAsync(UpdateMerchantIKSModel request);
    }
}
