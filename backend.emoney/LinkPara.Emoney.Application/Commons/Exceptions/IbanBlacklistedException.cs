using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class IbanBlacklistedException : ApiException
{
    public IbanBlacklistedException(string message) :
        base(ApiErrorCode.IbanBlacklisted, message)
    {
    }

    protected IbanBlacklistedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}