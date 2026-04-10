namespace LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;

public class FtpOptions
{
    public string Host { get; set; }
    public int? Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool? UsePassive { get; set; }
    public int? TimeoutSeconds { get; set; }
    public Dictionary<string, string> Paths { get; set; }
}
