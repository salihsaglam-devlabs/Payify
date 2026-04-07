using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.FileIngestion;

public enum FileSourceType
{
    [Description("Files are listed and read from remote sources (FTP/SFTP, etc.).")]
    Remote = 1,
    [Description("Local filesystem. If FilePath is provided, read directly from that path; if empty, list and read files from default path/profile in configuration.")]
    Local = 2
}
