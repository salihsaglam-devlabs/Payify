using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.Documents.Models;

public class GetDocumentResponse
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; }
    public string ContentType { get; set; }
    public DateTime CreateDate { get; set; }
    public Guid? UserId { get; set; }
    public Guid? MerchantId { get; set; }
    public Guid? AccountId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
