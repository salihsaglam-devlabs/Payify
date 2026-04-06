namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class CompanyPoolDocumentRequest
{
    public IFormFile File { get; set; }
    public Guid DocumentTypeId { get; set; }
}