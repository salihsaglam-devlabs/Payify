namespace LinkPara.ApiGateway.BackOffice.Services.Document.Models;

public class UpdateDocumentDto
{
    public Guid? Id { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public string OriginalFileName { get; set; }

    public Guid? UserId { get; set; }
    public Guid? MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public Guid? AccountId { get; set; }
    public Guid DocumentTypeId { get; set; }
}
