using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class InitiateWithdrawException : ApiException
{
    public InitiateWithdrawException() : base(ApiErrorCode.InitiateWithdrawException,
        "InitiateWithdrawException")
    {
    }
    
    protected InitiateWithdrawException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
