using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests.AgreementDocuments
{
    public class AgreedUserListRequest : SearchQueryParams
    {
        public Guid Id { get; set; }
    }
}
