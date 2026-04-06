using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

/// <summary>
///  Billing Api Error Codes
/// </summary>
public static class ApiErrorCode
{
    private const string _prefix = ExceptionPrefix.Billing;

    public const string InvalidInstitutionMapping = _prefix + "001";
    public const string InvalidSectorMapping = _prefix + "002";
    public const string ExternalApiError = _prefix + "003";
    public const string InvalidBillStatus = _prefix + "004";
    public const string InvalidExternalInstitution = _prefix + "005";
    public const string InvalidExternalSector = _prefix + "006";
    public const string InvalidInput = _prefix + "007";
    public const string InvalidCancelRequest = _prefix + "008";
    public const string ErrorCancellingProvision = _prefix + "009";
    public const string ReconciliationJobError = _prefix + "010";
    public const string PreviewProvisionError = _prefix + "011";
    public const string TimeoutError = _prefix + "012";
    public const string ResponseParseError = _prefix + "013";
    public const string TechnicalError = _prefix + "014";
    public const string InvalidPaymentError = _prefix + "015";
    public const string InvalidDebtError = _prefix + "016";
    public const string NoErrorReponse = _prefix + "017";
    public const string UnexpectedError = _prefix + "018";
    public const string NoBillFoundError = _prefix + "020";
    public const string BillAccountingCancelException = _prefix + "021";
    public const string ReconciliationSummaryException = _prefix + "022";
    public const string ReconciliationDetailException = _prefix + "023";
    public const string ReconciliationCloseException = _prefix + "024";
    public const string InsufficientBalanceError = _prefix + "025";
    public const string InvalidDSubscriptionOrDebtException = _prefix + "026";
    public const string NotPaidException = _prefix + "027";
    public const string AmountIntervalIsAlreadyInUseException = _prefix + "028";
}