using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetAllMerchantUserRequest : SearchQueryParams
{
    public Guid? UserId { get; set; }
    public string Fullname { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string RoleId { get; set; }
    public Guid? MerchantId { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}
