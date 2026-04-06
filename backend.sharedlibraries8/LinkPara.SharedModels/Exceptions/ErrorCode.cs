namespace LinkPara.SharedModels.Exceptions;

/// <summary>
/// Generic Error Codes
/// </summary>
public static class ErrorCode
{
    private const string _prefix = ExceptionPrefix.Shared;

    public const string InternalError = _prefix + "001";
    public const string ServiceForbidden = _prefix + "002";
    public const string InvalidParameters = _prefix + "003";
    public const string NotFound = _prefix + "004";
    public const string ValidationError = _prefix + "005";
    public const string InvalidOtp = _prefix + "006";
    public const string InvalidPermissions = _prefix + "007";
    public const string DuplicateRecord = _prefix + "008";
    public const string AlreadyInUse = _prefix + "009";
    public const string InvalidAuthorizationHeader = _prefix + "010";
    public const string AuthorizationSignatureMismatch = _prefix + "011";
    public const string AuthorizationTimestampExpired = _prefix + "012";
    public const string AuditableMissingInfo = _prefix + "013";
    public const string UserInBlacklist = _prefix + "014";
    public const string PotentialFraud = _prefix + "015";
    public const string BirthdateOutOfRange = _prefix + "016";
    public const string InvalidImageFormat = _prefix + "017";
    public const string Unauthorized = _prefix + "018";
    public const string BadRequest = _prefix + "019";
    public const string InvalidRecaptcha = _prefix + "020";
    public const string InvalidFileExtension = _prefix + "021";
}