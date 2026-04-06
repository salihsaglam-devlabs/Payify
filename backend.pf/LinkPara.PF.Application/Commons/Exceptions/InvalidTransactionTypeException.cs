using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidTransactionTypeException : ApiException
{
    public InvalidTransactionTypeException()
        : base(ApiErrorCode.InvalidTransactionType, "InvalidTransactionType")
    {
    }
}