using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class InvalidAmountException : ApiException
{
    public InvalidAmountException()
        : base(ApiErrorCode.InvalidAmount, "InvalidAmount") { }
    
    protected InvalidAmountException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
