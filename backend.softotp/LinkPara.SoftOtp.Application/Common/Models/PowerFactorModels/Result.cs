using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Enums;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Enums;
using Severity = LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Enums.Severity;

namespace LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels;

public class Result
{
    public Result() { }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorMessageDetails { get; set; }
    public string Exception { get; set; }
    public List<string> Params { get; set; }
    public Severity Severity { get; set; }
    public bool IsFriendly { get; set; }
}