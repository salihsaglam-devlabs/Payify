using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public interface ITierLevelHttpClient
{
    Task<List<TierLevelDto>> GetTierLevelsAsync();
}