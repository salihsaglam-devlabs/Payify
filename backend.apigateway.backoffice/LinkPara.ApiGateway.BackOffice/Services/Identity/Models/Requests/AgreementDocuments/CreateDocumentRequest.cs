using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses.AgreementDocuments;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests.AgreementDocuments;

public class CreateDocumentRequest
{
    public RecordStatus RecordStatus { get; set; }
    public string Name { get; set; }
    public List<AgreementDocumentVersionRequest> Agreements { get; set; }
    public ProductType ProductType { get; set; }
}
