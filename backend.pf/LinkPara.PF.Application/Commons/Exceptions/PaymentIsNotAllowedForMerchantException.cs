using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class PaymentIsNotAllowedForMerchantException : ApiException
{
    public PaymentIsNotAllowedForMerchantException()
        : base(ApiErrorCode.PaymentIsNotAllowedForMerchant, "PaymentIsNotAllowedForMerchant")
    {
    }
}