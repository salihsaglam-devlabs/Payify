using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request
{
    public class GetAllParameterRequest: SearchQueryParams
    {
        public string GroupCode { get; set; }
    }
}
