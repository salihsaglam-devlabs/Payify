using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.OpenBankingConfiguration;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Emoney.Infrastructure.Services.Secrets;

public class SecretService : ISecretService
{
    private readonly IVaultClient _vaultClient;
    public SecretService(IVaultClient vaultClient)
    {
        _vaultClient = vaultClient;
    }

    private OpenBankingHhsSettings openBankingHhsSettings { get; set; }
    private OpenBankingYosSettings openBankingYosSettings { get; set; }

    public OpenBankingHhsSettings OpenBankingHhsSettings
    {
        get
        {
            if (openBankingHhsSettings == null)
            {
                openBankingHhsSettings = _vaultClient.GetSecretValue<OpenBankingHhsSettings>("SharedSecrets", "OpenBankingHhsSettings");
            }
            return openBankingHhsSettings;
        }
    }
    public OpenBankingYosSettings OpenBankingYosSettings
    {
        get
        {
            if (openBankingYosSettings == null)
            {
                openBankingYosSettings = _vaultClient.GetSecretValue<OpenBankingYosSettings>("SharedSecrets", "OpenBankingYosSettings");
            }
            return openBankingYosSettings;
        }
    }
}
