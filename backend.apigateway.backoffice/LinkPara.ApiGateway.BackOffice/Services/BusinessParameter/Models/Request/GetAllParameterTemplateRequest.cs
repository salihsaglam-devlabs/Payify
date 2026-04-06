using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request
{
    public class GetAllParameterTemplateRequest: SearchQueryParams
    {
        public string GroupCode { get; set; }
    }
}
