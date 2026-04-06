using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

///<summary>
/// Identity Api Error Codes
///</summary>
public static class ApiErrorCode
{
    private const string _prefix = ExceptionPrefix.Identity;

    public const string InvalidNewPassword = _prefix + "001";
    public const string InvalidOtp = _prefix + "002";
    public const string InvalidRegister = _prefix + "004";
    public const string InvalidNewEmail = _prefix + "005";
    public const string LockedOut = _prefix + "006";
    public const string LoginFailed = _prefix + "007";
    public const string PasswordExpired = _prefix + "008";
    public const string PasswordsNotMatched = _prefix + "009";
    public const string PasswordHistoryRequirement = _prefix + "010";
    public const string PasswordContent = _prefix + "011";
    public const string PasswordLength = _prefix + "012";
    public const string PasswordRepetitiveCharacter = _prefix + "013";
    public const string PasswordSuccessiveCharacter = _prefix + "014";
    public const string RoleHasUsersException = _prefix + "015";
    public const string ForbiddenAgreementDocOperation = _prefix + "016";
    public const string BirthdateOutOfRange = _prefix + "017";
    public const string AlreadyDeactivated = _prefix + "018";
    public const string PasswordContainsBirthdate = _prefix + "019";
    public const string UserNotFound = _prefix + "020";
    public const string PilotModeLoginFailed = _prefix + "021";
    public const string SessionExpired = _prefix + "022";
    public const string NewSessionOpened = _prefix + "023";
    public const string RemoveUserLock = _prefix + "024";
    public const string OtpActivationFailed = _prefix + "025";
    public const string NewEmailAddressIsSameEmailException = _prefix + "026";
    public const string SuspendedUserLogin = _prefix + "027";
    public const string ExpireResetPasswordToken = _prefix + "028";
    public const string WrongUsernamePassword = _prefix + "029";
    public const string InvalidRoleUpdate = _prefix + "030";
    public const string PasswordContainsBirthDate = _prefix + "031";
    public const string PermanentLockedOut = _prefix + "032";
    public const string UserSecurityQuestionAnswer = _prefix + "033";
    public const string InvalidNewPhoneNumber = _prefix + "034";
    public const string UserSecurityQuestionAnswerMissmatch = _prefix + "035";    
    public const string InvalidInput = _prefix + "036";    
}