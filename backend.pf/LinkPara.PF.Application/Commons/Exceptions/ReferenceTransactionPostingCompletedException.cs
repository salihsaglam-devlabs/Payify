using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class ReferenceTransactionPostingCompletedException : ApiException
{
    public ReferenceTransactionPostingCompletedException()
        : base(ApiErrorCode.ReferenceTransactionPostingCompleted, "ReferenceTransactionPostingCompleted")
    {
    }
}