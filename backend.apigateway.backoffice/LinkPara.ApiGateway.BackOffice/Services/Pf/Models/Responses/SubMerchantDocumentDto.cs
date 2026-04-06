using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class SubMerchantDocumentDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentName { get; set; }
    public Guid SubMerchantId { get; set; }
    public string CreatedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
