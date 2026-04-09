namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class EndpointOptions
{
    public string Protocol { get; set; }
    public LocalOptions Local { get; set; }
    public FtpOptions Ftp { get; set; }
    public SftpOptions Sftp { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Protocol))
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Connections endpoint Protocol");
    }
}
