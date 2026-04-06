using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class GetFilterMerchantLimitRequest : SearchQueryParams
    {
        public Guid MerchantId { get; set; }
    }
}
