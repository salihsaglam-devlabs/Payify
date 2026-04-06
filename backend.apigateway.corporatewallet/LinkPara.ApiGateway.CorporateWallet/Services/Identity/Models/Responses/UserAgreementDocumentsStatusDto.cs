namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;

public class UserAgreementDocumentsStatusDto
{
    public Guid AgreementDocumentId { get; set; }
    public string Title { get; set; }
    public string Version { get; set; }
    public bool IsLatest { get; set; }
    public bool IsForceUpdate { get; set; }
    public bool IsSigned { get; set; }
    public string Content { get; set; }
}
