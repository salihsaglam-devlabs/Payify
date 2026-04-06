using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetCorporateAccountsRequest : SearchQueryParams
{
    public string Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public CompanyPoolCompanyType? CompanyType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}
