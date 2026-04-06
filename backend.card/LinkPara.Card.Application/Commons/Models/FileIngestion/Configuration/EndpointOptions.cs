namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class EndpointOptions
{
    public string Protocol { get; set; } = "Local";
    public LocalOptions Local { get; set; } = new();
    public FtpOptions Ftp { get; set; } = new();
    public SftpOptions Sftp { get; set; } = new();
}
