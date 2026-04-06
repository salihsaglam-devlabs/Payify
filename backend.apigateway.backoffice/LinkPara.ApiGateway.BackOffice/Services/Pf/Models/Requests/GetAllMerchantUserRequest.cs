using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllMerchantUserRequest : SearchQueryParams
{
    public Guid? UserId { get; set; }
    public Guid? MerchantId { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}
