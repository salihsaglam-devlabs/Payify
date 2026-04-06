namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class FtpOptions
{
    public string Host { get; set; }
    public int Port { get; set; } = 21;
    public string Username { get; set; }
    public string Password { get; set; }
    public bool UsePassive { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 60;
    public Dictionary<string, string> Paths { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
