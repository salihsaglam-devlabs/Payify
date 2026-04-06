using LinkPara.HttpProviders.Documents.Models;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
public class CorporateAccountDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string Name { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public CompanyPoolDto CompanyPool { get; set; }
    public List<GetDocumentResponse> Documents { get; set; }
}
