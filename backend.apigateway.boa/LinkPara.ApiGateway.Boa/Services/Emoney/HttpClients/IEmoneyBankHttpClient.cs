using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;

public interface IEmoneyBankHttpClient
{
    Task<List<EmoneyBankDto>> GetBanksAsync(string iban = null);
}
