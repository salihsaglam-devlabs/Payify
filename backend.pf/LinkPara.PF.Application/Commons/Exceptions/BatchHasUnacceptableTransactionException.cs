using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class BatchHasUnacceptableTransactionException : ApiException
{
    public BatchHasUnacceptableTransactionException() 
        : base(ApiErrorCode.BatchHasUnacceptableTransaction, "BatchHasUnacceptableTransaction")
    {
    }
}