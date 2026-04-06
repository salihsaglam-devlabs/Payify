using LinkPara.Epin.Application.Commons.Exceptions;
using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.HttpProviders.Utility;
using LinkPara.HttpProviders.Vault;
using Microsoft.Extensions.Configuration;

namespace LinkPara.Epin.Infrastructure.EMoneyServices;

public class EMoneyService : IEmoneyService
{
    private readonly IProvisionService _provisionService; 
    private readonly HttpClient _client;
    private readonly IVaultClient _vaultClient;

    public EMoneyService(IProvisionService walletService, IVaultClient vaultClient)
    {
        _provisionService = walletService;
        _vaultClient = vaultClient;
        _client = new HttpClient();
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
                ErrorCode = ApiErrorCode.ErrorCancellingProvision,
                ErrorMessage = $"ErrorCancellingProvisionMaxTimes: {exception.Message}"
            };
        }
    }

    public async Task<ProvisionResponse> CreateProvisionAsync(ProvisionRequest request)
    {
        return await _provisionService.ProvisionAsync(request);
    }

    public async Task<ProvisionPreviewResponse> PreviewProvisionAsync(ProvisionPreviewRequest request)
    {
        return await _provisionService.ProvisionPreviewAsync(request);
    }
}
