using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class MerchantDocumentDto
{
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentName { get; set; }
    public Guid? MerchantTransactionId { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
