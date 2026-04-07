using LinkPara.Card.Application.Commons.Models.Locking;

namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class FileIngestionAndReconciliationVaultSettings
{
    public FileIngestionSettings FileIngestion { get; set; } = new();
    public FileParsingRulesOptions FileParsingRules { get; set; } = new();
    public ProcessExecutionLockSettings ProcessExecutionLockSettings { get; set; } = new();
}
