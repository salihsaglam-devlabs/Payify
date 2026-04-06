using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.UrlModel;
using Microsoft.Extensions.Configuration;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class EMoneyService : IEmoneyService
{
    private readonly IProvisionService _provisionService;
    private readonly HttpClient _client;
    private readonly IVaultClient _vaultClient;
    public EMoneyService(IProvisionService provisionService, IVaultClient vaultClient)
    {
        _provisionService = provisionService;
        _client = new HttpClient();
        _vaultClient = vaultClient;
        _client.BaseAddress = new Uri(_vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Emoney"));
        
    }

    public async Task<ProvisionResponse> CancelProvisionAsync(string provisionReferenceId)
    {
        try
        {
            var provisionResponse = await _provisionService.CancelProvisionAsync(new ProvisionCancelRequest
            {
                ConversationId = provisionReferenceId
            });

            return provisionResponse;
        }
        catch (Exception exception)
        {
            return new ProvisionResponse
            {
                IsSucceed = false,
                ReferenceNumber = provisionReferenceId,
                ErrorMessage = $"ErrorCancellingProvisionMaxTimes: {exception.Message}"
            };
        }
    }

    public async Task<ProvisionCashbackResponse> CancelProvisionCashbackAsync(CancelProvisionCashbackRequest request)
    {
        return await _provisionService.CancelProvisionCashbackAsync(request);
    }

    public async Task<ProvisionResponse> CreateProvisionAsync(ProvisionRequest request)
    {
        return await _provisionService.ProvisionAsync(request);
    }

    public async Task<ProvisionPreviewResponse> PreviewProvisionAsync(ProvisionPreviewRequest request)
    {
        return await _provisionService.ProvisionPreviewAsync(request);
    }

    public async Task<ProvisionCashbackResponse> ProvisionCashbackAsync(ProvisionCashbackRequest request)
    {
        return await _provisionService.ProvisionCashbackAsync(request);
    }

    public async Task<ProvisionResponse> ReturnProvisionAsync(ProvisionReturnRequest request)
    {
        return await _provisionService.ReturnProvisionAsync(request);
    }
}
