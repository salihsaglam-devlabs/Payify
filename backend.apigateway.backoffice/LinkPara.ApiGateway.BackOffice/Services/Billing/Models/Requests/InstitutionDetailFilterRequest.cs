using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;

public class InstitutionDetailFilterRequest : SearchQueryParams
{
    public Guid InstitutionSummaryId { get; set; }
}