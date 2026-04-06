using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public interface ITierLevelHttpClient
{
    Task<List<TierLevelDto>> GetTierLevelsAsync();
}