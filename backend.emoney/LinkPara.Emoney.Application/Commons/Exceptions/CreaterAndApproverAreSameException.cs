using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class CreaterAndApproverAreSameException : ApiException
{
    public CreaterAndApproverAreSameException()
        : base(ApiErrorCode.CreaterAndApproverSame, "CreaterAndApproverSame") { }

    protected CreaterAndApproverAreSameException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}