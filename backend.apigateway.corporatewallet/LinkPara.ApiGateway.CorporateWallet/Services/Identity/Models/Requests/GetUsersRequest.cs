using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;

public class GetUsersRequest : SearchQueryParams
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string FullName { get; set; }
    public UserStatus? UserStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? RoleId { get; set; }
    public UserType? UserType { get; set; }
}
