using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

/// <summary>
///  PF Api Error Codes
/// </summary>
public static class ApiErrorCode
{
    private const string _prefix = ExceptionPrefix.PF;

    public const string InvalidInstallment = _prefix + "080";
    public const string CostProfileHasActivePos = _prefix + "081";
    public const string MerchantBusinessPartnerMaxCount = _prefix + "082";
    public const string IKSGeneralError = _prefix + "083";
    public const string InvalidActivationDate = _prefix + "084";
    public const string DuplicateMerchantLimit = _prefix + "085";
    public const string MerchantLimitDailyMaxAmountExceeded = _prefix + "086";
    public const string MerchantLimitMonthlyMaxAmountExceeded = _prefix + "087";
    public const string MerchantCommercialTitleNotMatch = _prefix + "088";
    public const string InvalidCommissionRate = _prefix + "118";
    public const string InvalidMerchantPricingProfile = _prefix + "119";

    //MerchantDeduction
    public const string TransactionTypeNotValidForDeduction = _prefix + "089";
    public const string TransactionCanBeChargebackOrSuspicious = _prefix + "090";
    public const string MerchantTransactionUnavailable = _prefix + "138";
    public const string PaymentIsNotAllowedForMerchant = _prefix + "139";

    //MerchantDue
    public const string DueAlreadyActiveForMerchant = _prefix + "091";
    public const string DueInUse = _prefix + "092";
    public const string CannotDeleteDefaultDueProfile = _prefix + "093";


    // Payment API Error
    public const string InvalidMerchant = _prefix + "001";
    public const string InvalidMerchantStatus = _prefix + "002";
    public const string InvalidTransactionType = _prefix + "003";
    public const string DuplicateMerchantTransaction = _prefix + "004";
    public const string VposNotFound = _prefix + "005";
    public const string BankTransactionNotFound = _prefix + "006";
    public const string MerchantTransactionNotFound = _prefix + "007";
    public const string AcquireBankNotFound = _prefix + "008";
    public const string InvalidAmount = _prefix + "009";
    public const string InvalidPaymentType = _prefix + "010";
    public const string InvalidSessionId = _prefix + "011";
    public const string MonthlyLimitAmountExceeded = _prefix + "012";
    public const string MonthlyLimitCountExceeded = _prefix + "013";
    public const string DailyLimitCountExceeded = _prefix + "014";
    public const string DailyLimitAmountExceeded = _prefix + "015";
    public const string InvalidCardInfo = _prefix + "016";
    public const string MdStatusNotSucceeded = _prefix + "017";
    public const string HalfSecureNotAllowed = _prefix + "018";
    public const string SessionNotFound = _prefix + "019";
    public const string InternationalCardNotAllowed = _prefix + "020";
    public const string ThreeDValidationAmountMismatch = _prefix + "021";
    public const string ThreeDValidationCardTokenMismatch = _prefix + "022";
    public const string ThreeDSessionExpired = _prefix + "023";
    public const string ThreeDValidationCurrencyMismatch = _prefix + "024";
    public const string ThreeDValidationTransactionTypeMismatch = _prefix + "025";
    public const string InvalidToken = _prefix + "026";
    public const string CardTokenExpired = _prefix + "027";
    public const string InstallmentNotAllowed = _prefix + "028";
    public const string NonSecureNotAllowed = _prefix + "029";
    public const string InsurancePaymentNotAllowed = _prefix + "120";
    public const string MultipleVposTypeNotAllowed = _prefix + "121";
    public const string PreAuthorizationNotAllowed = _prefix + "030";
    public const string TransactionAlreadyRefunded = _prefix + "031";
    public const string CardBinNotFound = _prefix + "032";
    public const string InvalidReferenceNumber = _prefix + "033";
    public const string InvalidReturnAmount = _prefix + "034";
    public const string ThreeDVerificationNotFound = _prefix + "035";
    public const string InternationalCardInstallmentNotAllowed = _prefix + "036";
    public const string PointInquiryNotFound = _prefix + "047";
    public const string PricingProfileItemNotFound = _prefix + "048";
    public const string BankHasTimedOut = _prefix + "049";
    public const string NoReturnPaymentAllowed = _prefix + "059";
    public const string NoReversePaymentAllowed = _prefix + "060";
    public const string TransactionNotReversible = _prefix + "061";
    public const string TransactionNotReturnable = _prefix + "062";
    public const string CannotReturnMerchantHasBlockedAmount = _prefix + "065";
    public const string DailyReturnAmountCannotGreaterAuthAmount = _prefix + "067";
    public const string IbanValidationFailed = _prefix + "068";
    public const string MerchantLimitExceeded = _prefix + "069";
    public const string GlobalMerchantIdNotFound = _prefix + "070";
    public const string BankLimitExceeded = _prefix + "071";
    public const string TransactionCannotBeRefundedToday = _prefix + "072";
    public const string IdentityValidationFailed = _prefix + "077";
    public const string PreValidation = _prefix + "079";
    public const string ManuelReturnAllowed = _prefix + "130";

    //Link
    public const string LinkNotFound = _prefix + "037";
    public const string LinkExpired = _prefix + "038";
    public const string LinkMaxLimitCountExceeded = _prefix + "039";
    public const string CustomerAddressRequired = _prefix + "040";
    public const string CustomerNameRequired = _prefix + "041";
    public const string CustomerNoteRequired = _prefix + "042";
    public const string CustomerPhoneNumberRequired = _prefix + "043";
    public const string CustomerEmailRequired = _prefix + "044";
    public const string LinkThreeDSRequired = _prefix + "045";
    public const string CommissionFromCustomerException = _prefix + "046";

    //Posting
    public const string PayingTotalAmountIsLessZero = _prefix + "050";
    public const string PostingUpdateBankNotPaymentWaiting = _prefix + "051";

    //Fraud
    public const string PotentialFraud = _prefix + "052";
    
    //Hpp
    public const string DuplicateOrderId = _prefix + "053";
    public const string IntegrationModeNotAllowed = _prefix + "054";
    public const string HppNotFound = _prefix + "055";
    public const string HppThreeDSRequired = _prefix + "056";
    public const string HppExpired = _prefix + "057";
    public const string InvalidGateway = _prefix + "058";
    public const string CannotTakeCommissionOnAdvance = _prefix + "063";
    public const string CannotSetAmountOnCommissionFromCustomer = _prefix + "064";
    public const string InstallmentsMustBeEmpty = _prefix + "066";
    
    //OnUs
    public const string CallbackUrlRequired = _prefix + "073";
    public const string OnUsExpired = _prefix + "074";
    public const string InvalidOnUsStatus = _prefix + "075";
    public const string OnUsVposAlreadyActive = _prefix + "076";
    public const string EmoneyUserRejected = _prefix + "078";
    
    
    public const string ActiveMerchantWalletRequired = _prefix + "094";
    
    //SubMerchant
    public const string SubMerchantRemove = _prefix + "095";
    public const string SubMerchantIntegrationModeNotAllowed = _prefix + "096";
    public const string SubMerchantInstallmentNotAllowed = _prefix + "097";
    public const string SubMerchantNonSecureNotAllowed = _prefix + "098";
    public const string InvalidSubMerchantStatus = _prefix + "099";
    public const string InvalidSubMerchant = _prefix + "100";
    public const string SubMerchantNoReturnPaymentAllowed = _prefix + "101";
    public const string InvalidMerchantType = _prefix + "102";
    public const string SubMerchantCountError = _prefix + "103";
    public const string SubMerchantUserCountError = _prefix + "104";
    public const string ParentMerchantCommissionMustBeZero = _prefix + "105";
    public const string InvalidPricingProfileType = _prefix + "106";
    
    //Boa Api Response
    public const string MerchantAlreadyActive = _prefix + "107";
    public const string BankCodeNotFound = _prefix + "108";
    public const string MainMerchantNotFound = _prefix + "109";
    public const string MccCodeNotFound = _prefix + "110";
    public const string MerchantIntegratorNotFound = _prefix + "111";
    public const string PricingProfileNotFound = _prefix + "112";
    public const string PricingProfileTypeIsNotValid = _prefix + "113";
    public const string InvalidCurrencyInMerchantLimit = _prefix + "114";
    public const string AdminUserCredentialsAlreadyExists = _prefix + "115";
    public const string MerchantLimitDailyMaxValueExceeded = _prefix + "116";
    public const string MerchantLimitMonthlyMaxValueExceeded = _prefix + "117";
    
    //PhysicalPos
    
    public const string PhysicalDeviceNotFound = _prefix + "122";
    public const string MerchantPhysicalDeviceNotFound = _prefix + "123";
    public const string MerchantPhysicalPosNotFound = _prefix + "124";
    public const string CostProfileNotFound = _prefix + "125";
    public const string PhysicalPosNotFound = _prefix + "126";
    public const string TransactionHasChargeback = _prefix + "127";
    public const string ReferenceTransactionPostingCompleted = _prefix + "128";
    public const string BatchEndOfDayAlreadyCompleted = _prefix + "129";
    public const string InvalidUpdateIks = _prefix + "131";
    public const string InvalidPosType = _prefix + "132";
    public const string ReconciliationAmountMismatch = _prefix + "133";
    public const string ReconciliationNewTransaction = _prefix + "134";
    public const string InvalidUnacceptableTransactionStatus = _prefix + "135";
    public const string CannotChangePosType = _prefix + "136";
    public const string BatchHasUnacceptableTransaction = _prefix + "137";
}

