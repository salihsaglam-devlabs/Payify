namespace LinkPara.HealthCheck.Models;

public class EmailConfiguration
{
    public string From { get; set; }
    public string SmtpServer { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public List<ToEmailAddress> ToEmailAddresses { get; set; }
    public List<string> AllowedIpAddresses { get; set; }
}

public class ToEmailAddress
{
    public string Address { get; set; }
}
