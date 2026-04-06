using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetFilterPricingProfileRequest : SearchQueryParams
{
    public ProfileStatus? ProfileStatus { get; set; }
    public ProfileType? ProfileType { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public string PricingProfileNumber { get; set; }
}
