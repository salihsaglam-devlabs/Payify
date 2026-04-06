using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Approval.Application.Commons.Exceptions;

public class MakerAndCheckerAreSameException : ApiException
{
    public MakerAndCheckerAreSameException()
       : base(ApiErrorCode.MakerAndCheckerAreSame, "MakerAndCheckerAreSame")
    {
    }
}
