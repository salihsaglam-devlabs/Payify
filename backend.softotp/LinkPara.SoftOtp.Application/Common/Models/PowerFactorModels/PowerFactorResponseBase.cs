using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Enums;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Enums;
using Severity = LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Enums.Severity;

namespace LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels;

public class PowerFactorResponseBase
{
    private List<Result> _Results;
    public string VerificationSignature { get; set; }
    public string SecretKey { get; set; }
    public string CipherText { get; set; }
    public string Content { get; set; }
    public List<Result> Results
    {
        get
        {
            if (_Results == null)
                _Results = new List<Result>();
            return _Results;
        }
        set
        {
            _Results = value;
        }
    }

    public bool Success
    {
        get
        {
            var suc = from s in Results
                where s.Severity == Severity.Error ||
                      s.Severity == Severity.Warning ||
                      s.Severity == Severity.Fraud
                select s;
            return !(suc != null && suc.Count() > 0);
        }
        set { }
    }
}