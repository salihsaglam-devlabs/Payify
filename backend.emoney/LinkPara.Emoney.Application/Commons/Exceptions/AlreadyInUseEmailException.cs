using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class AlreadyInUseEmailException : ApiException
{
    public AlreadyInUseEmailException()
        : base(ApiErrorCode.AlreadyInUseEmail, "AlreadyInUseEmail") { }
    
    protected AlreadyInUseEmailException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}

