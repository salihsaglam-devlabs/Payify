using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses.AgreementDocuments;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests.AgreementDocuments
{
    public class UpdateAgreementDocumentRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public ProductType ProductType { get; set; }
        public List<DocumentVersionDto> Agreements { get; set; }
    }
}
