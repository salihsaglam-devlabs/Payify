namespace LinkPara.HttpProviders.Documents.Models;

public class GetDocumentListRequest
{
    public Guid? UserId { get; set; }
    public Guid? MerchantId { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public bool OnlyLatest { get; set; }
}
