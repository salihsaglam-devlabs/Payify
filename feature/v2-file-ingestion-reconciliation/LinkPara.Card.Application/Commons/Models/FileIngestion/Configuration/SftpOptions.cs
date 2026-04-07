namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class SftpOptions
{
    public string Host { get; set; }
    public int Port { get; set; } = 22;
    public string Username { get; set; }
    public string Password { get; set; }
    public string PrivateKeyPath { get; set; }
    public string PrivateKeyPassphrase { get; set; }
    public string KnownHostFingerprint { get; set; }
    public Dictionary<string, string> Paths { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
