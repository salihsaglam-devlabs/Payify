using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class PreValidationException : ApiException
{
    public PreValidationException(string code, string message)
        : base(code, message)
    {
    }
}
