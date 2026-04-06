using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Application.Commons.Models.PerdigitalConfiguration;
using LinkPara.HttpProviders.Vault;
using Microsoft.AspNetCore.DataProtection;

namespace LinkPara.Epin.Infrastructure.Services.Secrets;

public class SecretService
{
    private readonly IVaultClient _vaultClient;
    public SecretService(IVaultClient vaultClient)
    {
        _vaultClient = vaultClient;
    }

    private PerdigitalApiSettings perdigitalApiSettings;
    public PerdigitalApiSettings PerdigitalApiSettings
    {
        get
        {
            if(perdigitalApiSettings == null)
            {
                perdigitalApiSettings = _vaultClient.GetSecretValue<PerdigitalApiSettings>("EpinSecrets", "PerdigitalApiSettings");
            }
            return perdigitalApiSettings;
        }
    }
}
