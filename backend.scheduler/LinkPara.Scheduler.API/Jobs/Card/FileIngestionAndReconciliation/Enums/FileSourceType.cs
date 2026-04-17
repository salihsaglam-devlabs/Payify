using System.ComponentModel;

namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Enums;

public enum FileSourceType
{
    [Description("Files are listed and read from remote sources (FTP/SFTP, etc.).")]
    Remote = 1,

    [Description("Local filesystem.")]
    Local = 2
}