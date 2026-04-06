using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.LogConsumers.Commons.Entities;

public class AuditLog : AuditEntity
{
    public DateTime LogDate { get; set; }
    public string SourceApplication { get; set; }
    public string Resource { get; set; }
    public string Operation { get; set; }
    public string UserName { get; set; }
    public Guid UserId { get; set; }
    public string ClientIpAddress { get; set; }
    public bool IsSuccess { get; set; }
    public string Channel { get; set; }
    public Dictionary<string, string> Details { get; set; }
    public string CorrelationId { get; set; }
}
