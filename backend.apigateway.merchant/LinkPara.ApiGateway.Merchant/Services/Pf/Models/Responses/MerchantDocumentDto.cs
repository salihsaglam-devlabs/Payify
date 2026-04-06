using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class MerchantDocumentDto
{
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentName { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
