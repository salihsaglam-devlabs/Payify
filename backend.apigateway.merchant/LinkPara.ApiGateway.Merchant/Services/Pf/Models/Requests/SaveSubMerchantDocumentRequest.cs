namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class SaveSubMerchantDocumentRequest
{
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentName { get; set; }
    public Guid SubMerchantId { get; set; }
}