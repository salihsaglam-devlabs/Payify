using LinkPara.SharedModels.Exceptions;

namespace LinkPara.SoftOtp.Application.Common.Exceptions;

public class StartOneTouchTransactionException : ApiException
{
    public StartOneTouchTransactionException() 
        : base(ApiErrorCode.StartOneTouchTransactionFailed, "StartOneTouchTransactionFailed")
    {
    }
}