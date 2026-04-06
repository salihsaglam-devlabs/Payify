using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class AccountLimitInsufficientException : ApiException
{
    public AccountLimitInsufficientException()
        : base(ApiErrorCode.AccountLimitInsufficient, "AccountLimitInsufficient")
    {
    }

    protected AccountLimitInsufficientException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}