using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients
{
    public interface IBankLogoHttpClient
    {
        Task<BankLogoDto> GetBankLogoAsync(Guid bankId);
    }
}