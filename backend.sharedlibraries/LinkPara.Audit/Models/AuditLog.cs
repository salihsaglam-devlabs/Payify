
namespace LinkPara.Audit.Models;

public class AuditLog
{
    public DateTime LogDate { get; set; }
    public string SourceApplication { get; set; }
    public string Resource { get; set; }
    public string Operation { get; set; }
    public string UserName { get; set; }
    public Guid UserId { get; set; }
    public bool IsSuccess { get; set; }
    public string ClientIpAddress { get; set; }
    public string Channel { get; set; }
    public Dictionary<string,string> Details { get; set; }
    public string CorrelationId { get; set; }
}

