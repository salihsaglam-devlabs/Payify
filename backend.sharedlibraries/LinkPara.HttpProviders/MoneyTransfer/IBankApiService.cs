using LinkPara.HttpProviders.MoneyTransfer.Models;

namespace LinkPara.HttpProviders.MoneyTransfer;

public interface IBankApiService
{
    Task<BankApiDto> GetByBankCodeAsync(int code);
}
