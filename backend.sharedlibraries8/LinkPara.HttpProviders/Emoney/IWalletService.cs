using LinkPara.HttpProviders.Emoney.Models;

namespace LinkPara.HttpProviders.Emoney;
public interface IWalletService
{
    Task<UpdateBalanceResponse> UpdateBalanceAsync(UpdateBalanceRequest request);
    Task<ValidateWalletServiceResponse> ValidateWalletAsync(ValidateWalletRequest request);
}
