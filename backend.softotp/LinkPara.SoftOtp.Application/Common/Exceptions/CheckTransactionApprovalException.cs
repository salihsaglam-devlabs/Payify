using LinkPara.SharedModels.Exceptions;

namespace LinkPara.SoftOtp.Application.Common.Exceptions;

public class CheckTransactionApprovalException : ApiException
{
    public CheckTransactionApprovalException() 
        : base(ApiErrorCode.CheckTransactionApprovalException, "CheckTransactionApprovalException")
    {
    }
}