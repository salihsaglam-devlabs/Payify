using System.Text.Json;

namespace LinkPara.HttpProviders.Vault.Models;
public class VaultSecretResponseData
{
    public JsonElement data { get; set; }
    public VaultSecretResponseMetadata metadata { get; set; }
}