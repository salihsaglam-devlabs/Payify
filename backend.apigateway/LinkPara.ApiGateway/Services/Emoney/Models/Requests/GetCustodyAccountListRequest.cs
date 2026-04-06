using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetCustodyAccountListRequest : SearchQueryParams
{
    public string ParentAccountId { get; set; }
    public string ParentIdentityNumber { get; set; }
    public string ParentNameSurname { get; set; }
    public string ParentPhoneNumber { get; set; }
    public string ChildIdentityNumber { get; set; }
    public string ChildNameSurname { get; set; }
    public string ChildPhoneNumber { get; set; }
}
