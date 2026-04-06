using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class InvalidIbanException : ApiException
{
    public InvalidIbanException()
        : base(ApiErrorCode.InvalidIban, "InvalidIban")
    {
    }
    
    protected InvalidIbanException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
