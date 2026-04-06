using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Accounting.Application.Commons.Exceptions;

/// <summary>
///  Accounting Api Error Codes
/// </summary>
public static class ApiErrorCode
{
    private const string _prefix = ExceptionPrefix.Accounting;

    public const string CanNotRetrySuccessPayment = _prefix + "001";
    public const string CanNotRetrySuccessCustomer = _prefix + "002";
    public const string CanNotCancelNotSuccessPayment = _prefix + "003";
    public const string RetryFailed = _prefix + "004";
}

