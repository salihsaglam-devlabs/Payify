
using LinkPara.ApiGateway.Services.Emoney.Models;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.ApiGateway.Services.Emoney;

public class PaymentOtpRequirementService : IPaymentOtpRequirementService
{
    private readonly IVaultClient _vaultClient;

    public PaymentOtpRequirementService(IVaultClient vaultClient)
    {
        _vaultClient = vaultClient;
    }

    private async Task<PaymentOtpRequirementDto> GetRequirements()
    {
        try
        {
            return await _vaultClient.GetSecretValueAsync<PaymentOtpRequirementDto>
                ("SharedSecrets", "TransferOtpRequirementSettings");
        }
        catch
        {
            return new PaymentOtpRequirementDto
            {
                IsActive = false,
                RequirementAmount = 0
            };
        }

    }

    public async Task<bool> IsRequireOtp(decimal amount)
    {
        var settings = await GetRequirements();

        if (settings.IsActive)
        {
            return amount >= settings.RequirementAmount;
        }

        return false;
    }
}
