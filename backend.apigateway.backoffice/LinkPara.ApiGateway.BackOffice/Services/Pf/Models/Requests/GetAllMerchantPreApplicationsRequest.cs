using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllMerchantPreApplicationsRequest : SearchQueryParams
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ResponsiblePerson { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public PosProductType? ProductType { get; set; }
    public MonthlyTurnover? MonthlyTurnover { get; set; }
    public ApplicationStatus? ApplicationStatus { get; set; }
    public string? Website { get; set; }
}