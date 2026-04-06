namespace LinkPara.ApiGateway.OpenBanking.Commons.OpenBankingConfiguration
{
    public class OpenBankingTokenSettings
    {
        public string Authority { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public bool ValidateAudience { get; set; }
    }
}
