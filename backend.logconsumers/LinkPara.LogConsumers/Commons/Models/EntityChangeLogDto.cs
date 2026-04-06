using LinkPara.LogConsumers.Commons.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.LogConsumers.Commons.Models;

public class EntityChangeLogDto
{
    public Guid Id { get; set; }
    public ServiceName ServiceName { get; set; }
    public string TableName { get; set; }
    public CrudOperationType CrudOperationType { get; set; }
    public string UserId { get; set; }
    public Dictionary<string, string> OldValues { get; set; }
    public Dictionary<string, string> NewValues { get; set; }
    public List<string> AffectedColumns { get; set; }
    public DateTime LogDate { get; set; }
    public string CorrelationId { get; set; }
}
