using LinkPara.SharedModels.BusModels.Commands.Scheduler.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllApiLogRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public PaymentLogType? PaymentType { get; set; }
    public string ApiLogRequest { get; set; }
    public string ApiLogResponse { get; set; }
}
