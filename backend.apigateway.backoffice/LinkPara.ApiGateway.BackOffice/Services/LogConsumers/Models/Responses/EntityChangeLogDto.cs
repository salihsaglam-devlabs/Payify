using LinkPara.ApiGateway.BackOffice.Services.LogConsumers.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.LogConsumers.Models.Responses;

public class EntityChangeLogDto
{
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

