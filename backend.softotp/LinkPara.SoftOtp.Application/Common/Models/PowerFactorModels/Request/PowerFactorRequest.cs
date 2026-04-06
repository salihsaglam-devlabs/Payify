namespace LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;

public class PowerFactorRequest
{
    public string SecretKey { get; set; }
    public string CipherText { get; set; }
    public string VerificationSignature { get; set; }
    public string HeaderVerificationSignature { get; set; }
    public bool IsMutualAuthenticationRequired { get; set; }
    public string MethodName { get; set; }
}