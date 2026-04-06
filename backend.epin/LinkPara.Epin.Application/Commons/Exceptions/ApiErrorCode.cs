using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

/// <summary>
///  Epin Api Error Codes
/// </summary>
public class ApiErrorCode
{
    private const string _prefix = ExceptionPrefix.Epin;

    public const string ErrorCancellingProvision = _prefix + "001";
    public const string ProvisionError = _prefix + "002";
    public const string StockNotFound = _prefix + "003";
    public const string OrderServiceError = _prefix + "004";
    public const string AuthorizationFailed = _prefix + "005";
    public const string PublisherList = _prefix + "006";
    public const string BrandList = _prefix + "007";
    public const string ProdcutList = _prefix + "008";
    public const string SendData = _prefix + "009";
    public const string InsufficientBalance = _prefix + "010";
    public const string ProductNotFound = _prefix + "011";
    public const string InsufficientStock = _prefix + "012";
    public const string OrderNotFound = _prefix + "013";
    public const string AuthorizationError = _prefix + "014";
    public const string JoygameUserNotFound = _prefix + "015";
    public const string AlreadyExistsReference = _prefix + "016";
    public const string PerdigitalSystemError = _prefix + "017";
    public const string HttpFailed = _prefix + "018";
    public const string PriceDidNotMatch = _prefix + "019";
    public const string NotMatchReconciliationCount = _prefix + "020";
    public const string NotMatchReconciliationTotal = _prefix + "021";
    public const string InvalidStatusForCancel = _prefix + "022";
    public const string OrdersNotFound = _prefix + "023";
    public const string OrdersNotMatch = _prefix + "024";
    public const string NotEmptyReconciliationResult = _prefix + "025";
}

