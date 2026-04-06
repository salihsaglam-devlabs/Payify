using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetCompanyPoolListRequest : SearchQueryParams
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Title { get; set; }
    public CompanyPoolStatus? CompanyPoolStatus { get; set; }
    public CompanyPoolChannel? Channel { get; set; }
    public CompanyPoolCompanyType? CompanyType { get; set; }
}
