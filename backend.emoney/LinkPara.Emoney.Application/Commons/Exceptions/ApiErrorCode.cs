using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

/// <summary>
///  E-Money Api Error Codes
/// </summary>
public static class ApiErrorCode
{
    private const string _prefix = ExceptionPrefix.Emoney;

    public const string MaxWalletLimitExceeded = _prefix + "001";
    public const string InsufficientBalance = _prefix + "002";
    public const string InvalidWalletStatus = _prefix + "003";
    public const string CurrencyCodeMismatch = _prefix + "004";
    public const string InvalidIban = _prefix + "005";
    public const string InvalidTransactionStatus = _prefix + "006";
    public const string MainWalletAlreadyExistError = _prefix + "007";
    public const string MoneyTransferOutsideEftHoursException = _prefix + "008";
    public const string WalletHasBalanceException = _prefix + "009";
    public const string InvalidTierLevel = _prefix + "010";
    public const string InvalidTierType = _prefix + "011";
    public const string LimitExceeded = _prefix + "012";
    public const string AlreadyDeactivated = _prefix + "013";
    public const string CanNotCloseMainWallet = _prefix + "014";
    public const string WalletBlocked = _prefix + "015";
    public const string ProfileIsPassive = _prefix + "016";
    public const string InvalidProfileItem = _prefix + "017";
    public const string AmountIntervalIsAlreadyInUse = _prefix + "018";
    public const string InvalidKycLevel = _prefix + "019";
    public const string DuplicateWallet = _prefix + "020";
    public const string InvalidAmount = _prefix + "021";
    public const string AmountCanNotBeEmpty = _prefix + "022";
    public const string GreaterThanProvisionAmount = _prefix + "023";
    public const string InitiateWithdrawException = _prefix + "024";
    public const string AlreadyInUseEmail = _prefix + "025";
    public const string CommercialPricingStatusInvalid = _prefix + "026";
    public const string NotFirstDayOfMonthException = _prefix + "027";
    public const string InvalidTierPermission = _prefix + "028";
    public const string KPSValidationFailed = _prefix + "029";
    public const string KPSInformationsNotMatch = _prefix + "030";
    public const string CheckTransactionApprovalFailed = _prefix + "031";
    public const string AlreadyInUsePhoneNumber = _prefix + "032";
    public const string IbanBlacklisted = _prefix + "033";
    public const string RequiredDocumentType = _prefix + "034";
    public const string StatusNotAcceptable = _prefix + "035";
    public const string Timeout = _prefix + "036";
    public const string AlreadyProcessed = _prefix + "037";
    public const string CreaterAndApproverSame = _prefix + "038";
    public const string DuplicateBulkTransferDetail = _prefix + "039";
    public const string SameAccountInsufficientBalance = _prefix + "040";
    public const string AccountLimitInsufficient = _prefix + "041";
    public const string DuplicateDocument = _prefix + "042";
    public const string MasterpassUnauthorizedAccess = _prefix + "043";
    public const string CustomizedWalletNotFound = _prefix + "044";
    public const string WalletHasPendingApprovalRequest = _prefix + "045";
    public const string WalletBlockageAmountIsGreaterThanAvailabeBalance = _prefix + "046";
    public const string DescriptionLengthIsShort = _prefix + "047";
    public const string PermissionDenied = _prefix + "048";
    public const string ReturnAmountExceeded = _prefix + "049";
    public const string MaxDocumentLimitExceeded = _prefix + "050";
    public const string MaxDocumentSizeExceeded = _prefix + "051";
}
