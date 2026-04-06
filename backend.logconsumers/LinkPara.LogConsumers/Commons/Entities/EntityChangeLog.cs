using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.LogConsumers.Commons.Entities;

public class EntityChangeLog : AuditEntity
{
    public string ServiceName { get; set; }
    public string TableName { get; set; }
    public string SchemaName { get; set; }
    public CrudOperationType CrudOperationType { get; set; }
    public string UserId { get; set; }
    public string ClientIpAddress { get; set; }
    public Dictionary<string, string> KeyValues { get; set; } = new();
    public Dictionary<string, string> OldValues { get; set; } = new();
    public Dictionary<string, string> NewValues { get; set; } = new();
    public List<string> AffectedColumns { get; set; } = new();
    public string CorrelationId { get; set; }
}

