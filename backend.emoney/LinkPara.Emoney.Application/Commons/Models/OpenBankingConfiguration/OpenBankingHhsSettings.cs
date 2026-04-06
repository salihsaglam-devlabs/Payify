namespace LinkPara.Emoney.Application.Commons.Models.OpenBankingConfiguration;

public class OpenBankingHhsSettings
{
    public string HhsCode { get; set; }
    public string OpenBankingUrl { get; set; }
    public string Authority { get; set; }
    public bool RequireHttpsMetadata { get; set; }
    public bool ValidateAudience { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Scope { get; set; }
    public string GrantType { get; set; }
    public string TenantId { get; set; }
}
