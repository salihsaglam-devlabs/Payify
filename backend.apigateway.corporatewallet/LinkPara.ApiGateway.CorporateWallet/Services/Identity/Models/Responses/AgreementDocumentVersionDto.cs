namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;

public class AgreementDocumentVersionDto
{
    public Guid AgreementDocumentId { get; set; }
    public string Title { get; set; }
    public string Version { get; set; }
    public string Content { get; set; }
    public bool IsForceUpdate { get; set; }
}
