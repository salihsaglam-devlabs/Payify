using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class JoygameUserNotFoundException : ApiException
{
    public JoygameUserNotFoundException(string message)
        : base(ApiErrorCode.JoygameUserNotFound, message)
    {
    }
}
