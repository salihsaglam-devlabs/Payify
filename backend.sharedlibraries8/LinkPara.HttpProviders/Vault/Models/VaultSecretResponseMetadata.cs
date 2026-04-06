namespace LinkPara.HttpProviders.Vault.Models;
public class VaultSecretResponseMetadata
{
    public string created_time { get; set; }
    public object custom_metadata { get; set; }
    public string deletion_time { get; set; }
    public bool destroyed { get; set; }
    public int version { get; set; }
}