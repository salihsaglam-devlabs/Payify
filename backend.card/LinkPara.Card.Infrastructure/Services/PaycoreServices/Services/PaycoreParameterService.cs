using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.ParameterModels;
using LinkPara.Card.Application.Features.PaycoreServices.ParameterServices.Queries.GetProductsQuery;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models;
using LinkPara.HttpProviders.Vault;
using Microsoft.Extensions.Configuration;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Services;

public class PaycoreParameterService : IPaycoreParameterService
{
    private readonly PaycoreClientService _clientService;
    private readonly IConfiguration _configuration;
    private readonly IVaultClient _vaultClient;
    private readonly PaycoreSettings _paycoreSettings;

    public PaycoreParameterService(PaycoreClientService clientService, 
        IConfiguration configuration, 
        IVaultClient vaultClient)
    {
        _clientService = clientService;
        _configuration = configuration;
        _vaultClient = vaultClient;
        _paycoreSettings = new PaycoreSettings();
        _configuration.GetSection(nameof(PaycoreSettings)).Bind(_paycoreSettings);
        _paycoreSettings.VaultSettings = _vaultClient.GetSecretValue<PaycoreVaultSettings>("CardSecrets", "PaycoreSettings");
    }

    public async Task<GetProductsResponse> GetProductsAsync(GetProductsQuery query)
    {
        try
        {
            var productResponse = await _clientService.ExecuteAsync<PaycoreProduct[]>(
                  $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetProducts}",
                  PaycoreRequestType.Get);

            return new GetProductsResponse
            {
                IsSuccess = productResponse.IsSuccess,
                Result = productResponse.Result
            };
        }
        catch (Exception ex)
        {
            return new GetProductsResponse
            {
                IsSuccess = false,
                Result = null
            };
        }
    }
}
