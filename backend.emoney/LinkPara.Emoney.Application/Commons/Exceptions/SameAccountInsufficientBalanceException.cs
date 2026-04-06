using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class SameAccountInsufficientBalanceException : ApiException
{
    public SameAccountInsufficientBalanceException()
        : base(ApiErrorCode.SameAccountInsufficientBalance, "SameAccountInsufficientBalance")
    {
    }

    protected SameAccountInsufficientBalanceException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
