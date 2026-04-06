namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class CompanyPoolDocumentRequest
{
    public IFormFile File { get; set; }
    public Guid DocumentTypeId { get; set; }
}