using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetFilterBankHealthCheckRequest : SearchQueryParams
{
    public DateTime? LastCheckDateStart { get; set; }
    public DateTime? LastCheckDateEnd { get; set; }
    public Guid? AcquireBankId { get; set; }
    public HealthCheckType? HealthCheckType { get; set; }
}
