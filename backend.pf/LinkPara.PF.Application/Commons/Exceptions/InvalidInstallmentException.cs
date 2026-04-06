using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidInstallmentException : ApiException
{
    public InvalidInstallmentException()
         : base(ApiErrorCode.InvalidInstallment, "InvalidInstallment")
    {
    }
}
