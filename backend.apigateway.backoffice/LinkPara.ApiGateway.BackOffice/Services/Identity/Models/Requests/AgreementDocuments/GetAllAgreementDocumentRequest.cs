using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests.AgreementDocuments
{
    public class GetAllAgreementDocumentRequest : SearchQueryParams
    {
        public RecordStatus? RecordStatus { get; set; }
        public string AgreementTitle { get; set; }
        public ProductType? ProductType { get; set; }
    }
}
