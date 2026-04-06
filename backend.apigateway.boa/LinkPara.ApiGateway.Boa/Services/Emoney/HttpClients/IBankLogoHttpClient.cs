using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;

public interface IBankLogoHttpClient
{
    Task<BankLogoDto> GetBankLogoAsync(Guid bankId);
}