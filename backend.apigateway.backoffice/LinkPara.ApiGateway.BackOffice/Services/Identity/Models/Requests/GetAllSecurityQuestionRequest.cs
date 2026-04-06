using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests
{
    public class GetAllSecurityQuestionRequest : SearchQueryParams
    {
        public RecordStatus? RecordStatus { get; set; }
        public string Question { get; set; }
        public string LanguageCode { get; set; }
    }
}
