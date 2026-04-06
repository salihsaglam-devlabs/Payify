using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class TransactionTypeNotValidForDeductionException : ApiException
{
    public TransactionTypeNotValidForDeductionException() 
        : base(ApiErrorCode.TransactionTypeNotValidForDeduction, "TransactionTypeNotValidForDeduction")
    {
    }
}