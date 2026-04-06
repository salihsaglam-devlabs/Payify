using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetAllAccountUserRequest : SearchQueryParams
{
    public string Fullname { get; set; }
    public Guid? AccountId { get; set; }
}
