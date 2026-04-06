namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

public class CompanyDocumentTypeResponse
{
    public string Name { get; set; }
    public Guid DocumentTypeId { get; set; }
    public bool IsRequired { get; set; }
}
