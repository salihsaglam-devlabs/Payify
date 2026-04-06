using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class AlreadyInUsePhoneNumberException : ApiException
{
    public AlreadyInUsePhoneNumberException()
        : base(ApiErrorCode.AlreadyInUsePhoneNumber, "AlreadyInUsePhoneNumber") { }

    protected AlreadyInUsePhoneNumberException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
