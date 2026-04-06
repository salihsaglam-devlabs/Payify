namespace LinkPara.HttpProviders.Vault.Models;
public class VaultLoginResponse
{
    public string request_id { get; set; }
    public string lease_id { get; set; }
    public bool renewable { get; set; }
    public int lease_duration { get; set; }
    public object data { get; set; }
    public object wrap_info { get; set; }
    public object warnings { get; set; }
    public VaultLoginResponseAuth auth { get; set; }
}