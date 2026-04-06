using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.HttpProviders.PF.Models.Response;

namespace LinkPara.HttpProviders.PF;

public interface IBankAccountService
{
    Task<MerchantBankAccountDto> GetBankAccountByMerchantId(GetBankAccountRequest request);
}
