using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class WrongUsernamePasswordException : ApiException
{
    public WrongUsernamePasswordException() 
        : base(ApiErrorCode.WrongUsernamePassword, "Wrong username or password!")
    {
        
    }

    protected WrongUsernamePasswordException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}