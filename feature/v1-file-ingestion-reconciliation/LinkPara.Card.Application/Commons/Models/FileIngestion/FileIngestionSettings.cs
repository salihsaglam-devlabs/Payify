using LinkPara.Card.Application.Commons.Constants;

namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class FileIngestionSettings
{
    public FtpIngestionSettings Ftp { get; set; } = new();
    public LocalIngestionSettings Local { get; set; } = new();
    public string TimestampFormat { get; set; } = "yyyyMMdd_HHmmss";
    public int MaxFilesPerRun { get; set; }
    public string FileEncoding { get; set; } = FileIngestionValues.DefaultEncoding;
    public FileDetectionSettings FileDetection { get; set; } = new();
    public ReconciliationProcessingSettings ReconciliationProcessing { get; set; } = new();
    public ReconciliationAlarmSettings Alarm { get; set; } = new();
}

public class FtpIngestionSettings
{
    public RemoteEndpointSettings Source { get; set; } = new();
    public RemoteTargetSettings Target { get; set; } = new();
}

public class LocalIngestionSettings
{
    public string DefaultDriveCode { get; set; } = "C:";
    public bool ArchiveEnabled { get; set; } = true;
    public string CardTransactionsPath { get; set; }
    public string BkmClearingPath { get; set; }
    public string MastercardClearingPath { get; set; }
    public string VisaClearingPath { get; set; }
    public LocalPathDefaults Defaults { get; set; } = new();
}

public class LocalPathDefaults
{
    public LocalOsPathSettings Windows { get; set; } = new();
    public LocalOsPathSettings Linux { get; set; } = new();
    public LocalOsPathSettings MacOS { get; set; } = new();
}

public class LocalOsPathSettings
{
    public string CardTransactionsPath { get; set; }
    public string BkmClearingPath { get; set; }
    public string MastercardClearingPath { get; set; }
    public string VisaClearingPath { get; set; }
}

public class FileDetectionSettings
{
    public string[] CardFileNamePatterns { get; set; } = ["^CARD_TRANSACTIONS_\\d{8}_\\d+$"];
    public string[] ClearingFileNamePatterns { get; set; } = ["^(BKMACC|MSCACC|VISAACC)\\d{8}\\d+$"];
    public string[] SupportedExtensions { get; set; } = [".txt", ".dat"];
}

public class RemoteEndpointSettings
{
    public string Protocol { get; set; } = "FTP";
    public string Host { get; set; }
    public int Port { get; set; } = 21;
    public string Username { get; set; }
    public string Password { get; set; }
    public string PrivateKeyPath { get; set; }
    public string PrivateKeyPassphrase { get; set; }
    public string KnownHostFingerprint { get; set; }
    public string CardTransactionsPath { get; set; } = "/turkonay/PROD/PTS2TURKONAY/PAYCORE_REPORTS";
    public string BkmClearingOutgoingPath { get; set; } = "/turkonay/PROD/PTS2TURKONAY/BKM_REPORTS/OUTGOING";
    public string MastercardClearingOutgoingPath { get; set; } = "/turkonay/PROD/PTS2TURKONAY/MASTERCARD_REPORTS/OUTGOING";
    public string VisaClearingOutgoingPath { get; set; } = "/turkonay/PROD/PTS2TURKONAY/VISA_REPORTS/OUTGOING";
    public bool UsePassive { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
}

public class RemoteTargetSettings : RemoteEndpointSettings
{
    public bool Enabled { get; set; }
    public string RootPath { get; set; }
}

public class ReconciliationProcessingSettings
{
    public bool EnableParallelEvaluation { get; set; } = false;
    public int EvaluationDegreeOfParallelism { get; set; } = 4;
    public bool EnableAutoOperationTrigger { get; set; } = true;
    public int RegenerateCandidateLookbackDays { get; set; } = 2;
    public int ExpireControlStatPLookbackDays { get; set; } = 20;
    public int PayifyLookupLookbackDays { get; set; } = 20;
    public int OriginalTxnMatchingLookbackDays { get; set; } = 10;
}

public class ReconciliationAlarmSettings
{
    public bool Enabled { get; set; } = false;
    public string TemplateName { get; set; } = "CardReconciliationAlarm";
    public string[] ToEmails { get; set; } = [];
}
