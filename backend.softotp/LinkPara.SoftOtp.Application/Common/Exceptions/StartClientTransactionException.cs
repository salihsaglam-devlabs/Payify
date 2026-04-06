using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.SoftOtp.Application.Common.Exceptions;

public class StartClientTransactionException : ApiException
{
    public StartClientTransactionException() 
        : base(ApiErrorCode.StartClientTransactionException, "StartClientTransactionException")
    {
    }

    protected StartClientTransactionException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}