using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses.AgreementDocuments
{
    public class AgreementDocumentResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LastVersion { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreateDate { get; set; }
        public ProductType ProductType { get; set; }
        public List<AgreementDocumentVersionResponse> Agreements { get; set; }
    }
}
