using LinkPara.SharedModels.Exceptions;

namespace LinkPara.SoftOtp.Application.Common.Exceptions;

public class UpdateActivationPINByCustomerIdException : ApiException
{
    public UpdateActivationPINByCustomerIdException()
        : base(ApiErrorCode.UpdateActivationPINByCustomerIdFailed, "UpdateActivationPINByCustomerIdFailed")
    {
    }
}