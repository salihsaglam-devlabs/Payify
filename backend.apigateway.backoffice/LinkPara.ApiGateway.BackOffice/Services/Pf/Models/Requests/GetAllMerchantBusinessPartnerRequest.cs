using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class GetAllMerchantBusinessPartnerRequest : SearchQueryParams
    { 
        public Guid? MerchantId { get; set; }
    }
}
