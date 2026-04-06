using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface IAccountFinancialInfoHttpClient
{
    Task<AccountFinancialInfoDto> GetAccountFinancialInfoAsync(Guid accountId);
}
