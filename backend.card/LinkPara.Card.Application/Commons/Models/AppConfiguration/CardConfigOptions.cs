using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;

namespace LinkPara.Card.Application.Commons.Models.AppConfiguration;

public class CardConfigOptions
{
    public const string SectionName = "CardConfig";

    public ApplicationSection Application { get; set; }
    public EndpointsSection Endpoints { get; set; }

    public void ValidateAndApplyDefaults()
    {
        Application ??= new ApplicationSection();
        Application.ValidateAndApplyDefaults();

        Endpoints ??= new EndpointsSection();
        Endpoints.ValidateAndApplyDefaults();
    }
    
    public class ApplicationSection
    {
        public AuthBypassSection AuthBypass { get; set; }
        public DatabaseSection Database { get; set; }

        public void ValidateAndApplyDefaults()
        {
            AuthBypass ??= new AuthBypassSection();
            AuthBypass.ValidateAndApplyDefaults();

            Database ??= new DatabaseSection();
            Database.ValidateAndApplyDefaults();
        }
    }

    public class AuthBypassSection
    {
        public const bool DefaultEnabled = false;

        public bool? Enabled { get; set; }
        public string[] Controllers { get; set; }

        public void ValidateAndApplyDefaults()
        {
            Enabled ??= DefaultEnabled;
            Controllers ??= Array.Empty<string>();
        }
    }

    public class DatabaseSection
    {
        public const bool DefaultEnableAutoMigrate = false;

        public bool? EnableAutoMigrate { get; set; }

        public void ValidateAndApplyDefaults()
        {
            EnableAutoMigrate ??= DefaultEnableAutoMigrate;
        }
    }
    
    public class EndpointsSection
    {
        public FileIngestionEndpoints FileIngestion { get; set; }
        public ReconciliationEndpoints Reconciliation { get; set; }
        public ArchiveEndpoints Archive { get; set; }
        public ReportingEndpoints Reporting { get; set; }

        public void ValidateAndApplyDefaults()
        {
            FileIngestion ??= new FileIngestionEndpoints();
            FileIngestion.ValidateAndApplyDefaults();

            Reconciliation ??= new ReconciliationEndpoints();
            Reconciliation.ValidateAndApplyDefaults();

            Archive ??= new ArchiveEndpoints();
            Archive.ValidateAndApplyDefaults();

            Reporting ??= new ReportingEndpoints();
        }
    }
    public class FileIngestionEndpoints
    {
        public IngestEndpoint Ingest { get; set; }

        public void ValidateAndApplyDefaults()
        {
            if (Ingest is null)
                throw new FileIngestionVaultConfigMissingException(
                    "Vault key missing: CardConfig.Endpoints.FileIngestion.Ingest (Connections and Profiles are required).");
            Ingest.ValidateAndApplyDefaults();
        }
    }

    public class IngestEndpoint
    {
        public ConnectionsOptions Connections { get; set; }
        public ProcessingOptions Processing { get; set; }
        public Dictionary<string, ProfileOptions> Profiles { get; set; }

        public void ValidateAndApplyDefaults()
        {
            Processing ??= new ProcessingOptions();
            Processing.ValidateAndApplyDefaults();

            if (Connections is null)
                throw new FileIngestionConfigConnectionsMissingException(
                    "Vault configuration missing: CardConfig.Endpoints.FileIngestion.Ingest.Connections");
            if (Profiles is null)
                throw new FileIngestionConfigProfilesMissingException(
                    "Vault configuration missing: CardConfig.Endpoints.FileIngestion.Ingest.Profiles");

            Connections.Validate();
        }
    }
    
    public class ReconciliationEndpoints
    {
        public EvaluateEndpoint Evaluate { get; set; }
        
        public OperationsExecuteEndpoint OperationsExecute { get; set; }
        
        public AlertsEndpoint Alerts { get; set; }

        public void ValidateAndApplyDefaults()
        {
            Evaluate ??= new EvaluateEndpoint();
            Evaluate.ValidateAndApplyDefaults();

            OperationsExecute ??= new OperationsExecuteEndpoint();
            OperationsExecute.ValidateAndApplyDefaults();

            Alerts ??= new AlertsEndpoint();
            Alerts.ValidateAndApplyDefaults();
        }
    }

    public class EvaluateEndpoint
    {
        public const int DefaultChunkSize = 50_000;
        public const int DefaultClaimTimeoutSeconds = 1_800;
        public const int DefaultClaimRetryCount = 5;
        public const int DefaultOperationMaxRetries = 5;

        public ConsumerSection Consumer { get; set; }
        public int? ChunkSize { get; set; }
        public int? ClaimTimeoutSeconds { get; set; }
        public int? ClaimRetryCount { get; set; }
        public int? OperationMaxRetries { get; set; }

        public void ValidateAndApplyDefaults()
        {
            Consumer ??= new ConsumerSection();
            Consumer.ValidateAndApplyDefaults();

            ChunkSize ??= DefaultChunkSize;
            ClaimTimeoutSeconds ??= DefaultClaimTimeoutSeconds;
            ClaimRetryCount ??= DefaultClaimRetryCount;
            OperationMaxRetries ??= DefaultOperationMaxRetries;

            if (ChunkSize <= 0)
                throw new ReconciliationEvaluateChunkSizeInvalidException(
                    $"Reconciliation.Evaluate.ChunkSize must be positive. Current: {ChunkSize}");
            if (ClaimTimeoutSeconds <= 0)
                throw new ReconciliationEvaluateClaimTimeoutInvalidException(
                    $"Reconciliation.Evaluate.ClaimTimeoutSeconds must be positive. Current: {ClaimTimeoutSeconds}");
            if (ClaimRetryCount <= 0)
                throw new ReconciliationEvaluateClaimRetryInvalidException(
                    $"Reconciliation.Evaluate.ClaimRetryCount must be positive. Current: {ClaimRetryCount}");
            if (OperationMaxRetries < 0)
                throw new ReconciliationEvaluateOperationMaxRetriesInvalidException(
                    $"Reconciliation.Evaluate.OperationMaxRetries must be non-negative. Current: {OperationMaxRetries}");
        }
    }

    public class ConsumerSection
    {
        public const bool DefaultRespondToContext = false;

        public bool? RespondToContext { get; set; }

        public void ValidateAndApplyDefaults()
        {
            RespondToContext ??= DefaultRespondToContext;
        }
    }

    public class OperationsExecuteEndpoint
    {
        public const int DefaultMaxEvaluations = 500_000;
        public const int DefaultLeaseSeconds = 900;
        public const bool DefaultAutoArchiveAfterExecute = false;

        public int? MaxEvaluations { get; set; }
        public int? LeaseSeconds { get; set; }
        
        public bool? AutoArchiveAfterExecute { get; set; }

        public void ValidateAndApplyDefaults()
        {
            MaxEvaluations ??= DefaultMaxEvaluations;
            LeaseSeconds ??= DefaultLeaseSeconds;
            AutoArchiveAfterExecute ??= DefaultAutoArchiveAfterExecute;

            if (MaxEvaluations <= 0)
                throw new ReconciliationExecuteMaxEvaluationsInvalidException(
                    $"Reconciliation.Execute.MaxEvaluations must be positive. Current: {MaxEvaluations}");
            if (LeaseSeconds <= 0)
                throw new ReconciliationExecuteLeaseSecondsInvalidException(
                    $"Reconciliation.Execute.LeaseSeconds must be positive. Current: {LeaseSeconds}");
        }
    }

    public class AlertsEndpoint
    {
        public const bool DefaultEnabled = true;
        public const string DefaultTemplateName = "ReconciliationAlertTemplate";
        public const int DefaultBatchSize = 10_000_000;
        public const bool DefaultIncludeFailed = true;

        public bool? Enabled { get; set; }
        public string TemplateName { get; set; }
        public string[] ToEmails { get; set; }
        public int? BatchSize { get; set; }
        public bool? IncludeFailed { get; set; }

        public void ValidateAndApplyDefaults()
        {
            Enabled ??= DefaultEnabled;
            TemplateName = string.IsNullOrWhiteSpace(TemplateName) ? DefaultTemplateName : TemplateName;
            ToEmails ??= Array.Empty<string>();
            BatchSize ??= DefaultBatchSize;
            IncludeFailed ??= DefaultIncludeFailed;

            if (BatchSize <= 0)
                throw new ReconciliationAlertBatchSizeInvalidException(
                    $"Reconciliation.Alert.BatchSize must be positive. Current: {BatchSize}");
        }
    }

    public class ArchiveEndpoints
    {
        public const bool DefaultEnabled = true;
        
        public ArchivePreviewEndpoint Preview { get; set; }
        
        public ArchiveRunEndpoint Run { get; set; }

        public void ValidateAndApplyDefaults()
        {
            Preview ??= new ArchivePreviewEndpoint();
            Preview.ValidateAndApplyDefaults();

            Run ??= new ArchiveRunEndpoint();
            Run.ValidateAndApplyDefaults();
        }
    }

    public class ArchivePreviewEndpoint
    {
        public const int DefaultPreviewLimit = 5_000;

        public int? PreviewLimit { get; set; }

        public void ValidateAndApplyDefaults()
        {
            PreviewLimit ??= DefaultPreviewLimit;

            if (PreviewLimit <= 0)
                throw new ArchivePreviewLimitInvalidException(
                    $"Archive.Defaults.PreviewLimit must be positive. Current: {PreviewLimit}");
        }
    }

    public class ArchiveRunEndpoint
    {
        public bool? Enabled { get; set; }
        public ArchiveRunDefaults Defaults { get; set; }
        public ArchiveRulesSection Rules { get; set; }
        public ArchiveTerminalStatusesSection TerminalStatuses { get; set; }

        public void ValidateAndApplyDefaults()
        {
            Enabled ??= ArchiveEndpoints.DefaultEnabled;

            Defaults ??= new ArchiveRunDefaults();
            Defaults.ValidateAndApplyDefaults();

            Rules ??= new ArchiveRulesSection();
            Rules.ValidateAndApplyDefaults();

            TerminalStatuses ??= new ArchiveTerminalStatusesSection();
            TerminalStatuses.ValidateAndApplyDefaults();
        }
    }

    public class ArchiveRunDefaults
    {
        public const int DefaultMaxRunCount = 50_000;
        public const bool DefaultContinueOnError = false;
        public const bool DefaultUseConfiguredBeforeDateOnly = false;
        public const string DefaultDefaultBeforeDateStrategy = "RetentionDays";
        public const int DefaultMaxRetryPerFile = 1;
        public const int DefaultRetryDelaySeconds = 2;

        public int? MaxRunCount { get; set; }
        public bool? ContinueOnError { get; set; }
        public bool? UseConfiguredBeforeDateOnly { get; set; }
        public string DefaultBeforeDateStrategy { get; set; }
        public int? MaxRetryPerFile { get; set; }
        public int? RetryDelaySeconds { get; set; }

        public void ValidateAndApplyDefaults()
        {
            MaxRunCount ??= DefaultMaxRunCount;
            ContinueOnError ??= DefaultContinueOnError;
            UseConfiguredBeforeDateOnly ??= DefaultUseConfiguredBeforeDateOnly;
            DefaultBeforeDateStrategy = string.IsNullOrWhiteSpace(DefaultBeforeDateStrategy)
                ? DefaultDefaultBeforeDateStrategy
                : DefaultBeforeDateStrategy;
            MaxRetryPerFile ??= DefaultMaxRetryPerFile;
            RetryDelaySeconds ??= DefaultRetryDelaySeconds;

            if (MaxRunCount <= 0)
                throw new ArchiveMaxRunCountInvalidException(
                    $"Archive.Defaults.MaxRunCount must be positive. Current: {MaxRunCount}");
            if (MaxRetryPerFile < 0)
                throw new ArchiveMaxRetryPerFileInvalidException(
                    $"Archive.Defaults.MaxRetryPerFile must be non-negative. Current: {MaxRetryPerFile}");
            if (RetryDelaySeconds < 0)
                throw new ArchiveRetryDelaySecondsInvalidException(
                    $"Archive.Defaults.RetryDelaySeconds must be non-negative. Current: {RetryDelaySeconds}");
        }
    }

    public class ArchiveRulesSection
    {
        public const int DefaultRetentionDays = 90;
        public const int DefaultMinLastUpdateAgeHours = 72;
        public const bool DefaultRetentionOnlyMode = false;

        public int? RetentionDays { get; set; }
        public int? MinLastUpdateAgeHours { get; set; }
        public bool? RetentionOnlyMode { get; set; }

        public void ValidateAndApplyDefaults()
        {
            RetentionDays ??= DefaultRetentionDays;
            MinLastUpdateAgeHours ??= DefaultMinLastUpdateAgeHours;
            RetentionOnlyMode ??= DefaultRetentionOnlyMode;

            if (RetentionDays < 0)
                throw new ArchiveRetentionDaysInvalidException(
                    $"Archive.Rules.RetentionDays must be non-negative. Current: {RetentionDays}");
            if (MinLastUpdateAgeHours < 0)
                throw new ArchiveMinUpdateAgeInvalidException(
                    $"Archive.Rules.MinLastUpdateAgeHours must be non-negative. Current: {MinLastUpdateAgeHours}");
        }
    }

    public class ArchiveTerminalStatusesSection
    {
        private static readonly string[] DefaultTerminalStatuses = { "Success", "Failed" };
        private static readonly string[] DefaultEvaluationStatuses = { "Completed", "Failed" };
        private static readonly string[] DefaultOperationStatuses = { "Completed", "Failed", "Cancelled" };
        private static readonly string[] DefaultExecutionStatuses = { "Completed", "Failed", "Skipped" };
        private static readonly string[] DefaultReviewStatuses = { "Approved", "Rejected", "Cancelled" };
        private static readonly string[] DefaultAlertStatuses = { "Consumed", "Failed", "Ignored" };

        public string[] IngestionFile { get; set; }
        public string[] IngestionFileLine { get; set; }
        public string[] IngestionFileLineReconciliation { get; set; }
        public string[] ReconciliationEvaluation { get; set; }
        public string[] ReconciliationOperation { get; set; }
        public string[] ReconciliationReview { get; set; }
        public string[] ReconciliationOperationExecution { get; set; }
        public string[] ReconciliationAlert { get; set; }

        public void ValidateAndApplyDefaults()
        {
            IngestionFile ??= DefaultTerminalStatuses;
            IngestionFileLine ??= DefaultTerminalStatuses;
            IngestionFileLineReconciliation ??= DefaultTerminalStatuses;
            ReconciliationEvaluation ??= DefaultEvaluationStatuses;
            ReconciliationOperation ??= DefaultOperationStatuses;
            ReconciliationReview ??= DefaultReviewStatuses;
            ReconciliationOperationExecution ??= DefaultExecutionStatuses;
            ReconciliationAlert ??= DefaultAlertStatuses;
        }
    }
    
    public class ReportingEndpoints
    {
    }
}

