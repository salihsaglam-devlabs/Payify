using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Enums;

namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels;

public class OtpResult
{
    public OtpResult() { }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorMessageDetails { get; set; }
    public string Exception { get; set; }
    public List<string> Params { get; set; }
    public Severity Severity { get; set; }
    public bool IsFriendly { get; set; }
}