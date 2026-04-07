namespace LinkPara.Card.Application.Commons.Constants;

public static class FileIngestionValues
{
    public const string CardFileType = "CARD_TRANSACTIONS";
    public const string ClearingFileType = "CLEARING_FILE";
    public const string UnknownFileType = "UNKNOWN";
    public const string DefaultEncoding = "UTF-8";
}

public static class FileNameMarkers
{
    public const string CardTransaction = "CARD_TRANSACTION";
    public const string Clearing = "CLEARING";
    public const string BkmAcc = "BKMACC";
    public const string Bkm = "BKM";
    public const string Msc = "MSC";
    public const string Master = "MASTER";
    public const string Visa = "VISA";
}

public static class FileParsingRuleKeys
{
    public const string CardTransactions = "CARD_TRANSACTIONS";
    public const string ClearingBkm = "CLEARING_BKM";
    public const string ClearingMsc = "CLEARING_MSC";
    public const string ClearingVisa = "CLEARING_VISA";
}

public static class FixedWidthRecordTypes
{
    public const string Header = "H";
    public const string Detail = "D";
    public const string Footer = "F";
}

public static class FileIngestionErrorCodes
{
    public const string NoFilesFound = "NO_FILES_FOUND";
    public const string PartialOrFullFailure = "PARTIAL_OR_FULL_FAILURE";
}

public static class FileIngestionResultStatuses
{
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";
    public const string Skipped = "SKIPPED";
}

public static class FileIngestionSourceTypes
{
    public const string Local = "LOCAL";
    public const string Ftp = "FTP";
    public const string LocalCardTransactions = "LOCAL_CARD_TRANSACTIONS";
    public const string LocalClearing = "LOCAL_CLEARING";
    public const string FtpCardTransactions = "FTP_CARD_TRANSACTIONS";
    public const string FtpClearing = "FTP_CLEARING";
}

public static class FileIngestionAlarmCodes
{
    public const string FileImportFailed = "FILE_IMPORT_FAILED";
    public const string ReconciliationFailedAfterImport = "RECON_FAILED_AFTER_IMPORT";
    public const string ReconciliationAutoActionTriggerFailed = "RECON_AUTO_ACTION_TRIGGER_FAILED";
    public const string BkmClrNoDuplicateInFile = "BKM_CLRNO_DUPLICATE_IN_FILE";
    public const string BkmClrNoDuplicateDb = "BKM_CLRNO_DUPLICATE_DB";
}
