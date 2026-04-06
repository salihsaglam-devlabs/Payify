using LinkPara.ApiGateway.BackOffice.Services.LogConsumers.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.LogConsumers.Models.Requests;

public class GetFilterEntityChangeLogRequest : SearchQueryParams
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public string TableName { get; set; }
    public CrudOperationType? CrudOperationType { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public ServiceName? ServiceName { get; set; }
    public string CorrelationId { get; set; }
}
