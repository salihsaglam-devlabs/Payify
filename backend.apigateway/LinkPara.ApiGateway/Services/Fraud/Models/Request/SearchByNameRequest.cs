using LinkPara.ApiGateway.Services.Fraud.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Fraud.Models.Request
{
    public class SearchByNameRequest : SearchQueryParams
    {
        public string Name { get; set; }
        public string BirthYear { get; set; }
        public SearchType SearchType { get; set; }
    }
}
