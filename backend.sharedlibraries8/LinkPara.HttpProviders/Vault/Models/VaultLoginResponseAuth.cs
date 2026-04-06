namespace LinkPara.HttpProviders.Vault.Models;
public class VaultLoginResponseAuth
{
    public string client_token { get; set; }
    public string accessor { get; set; }
    public List<string> policies { get; set; }
    public List<string> token_policies { get; set; }
    public VaultLoginResponseMetadata metadata { get; set; }
    public int lease_duration { get; set; }
    public bool renewable { get; set; }
    public string entity_id { get; set; }
    public string token_type { get; set; }
    public bool orphan { get; set; }
    public object mfa_requirement { get; set; }
    public int num_uses { get; set; }
}