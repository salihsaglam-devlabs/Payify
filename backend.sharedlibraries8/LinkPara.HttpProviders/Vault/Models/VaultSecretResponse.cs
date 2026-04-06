namespace LinkPara.HttpProviders.Vault.Models;
public class VaultSecretResponse
{
    public string request_id { get; set; }
    public string lease_id { get; set; }
    public bool renewable { get; set; }
    public int lease_duration { get; set; }
    public VaultSecretResponseData data { get; set; }
    public object wrap_info { get; set; }
    public object warnings { get; set; }
    public object auth { get; set; }
}