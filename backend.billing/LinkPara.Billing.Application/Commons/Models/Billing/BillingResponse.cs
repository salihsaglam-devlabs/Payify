using LinkPara.Billing.Application.Commons.Exceptions;

namespace LinkPara.Billing.Application.Commons.Models.Billing;

public class BillingResponse<T>
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public T Response { get; set; }
}