namespace LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels;

public class PowerfactorConfig
{
    public string ServerIP { get; set; }
    public string ServicePath { get; set; }
    public string BackOfficeIP { get; set; }
    public string BackOfficePath { get; set; }
    public string SSLEnabled { get; set; }
    public string IsMutualAuthenticationRequired { get; set; }
    public string PrivateKey { get; set; }
    public string PublicKey { get; set; }
    public string FirebaseServerKey { get; set; }
    public string ApplicationName { get; set; }
    public string TransactionDefinitionKey { get; set; }
}