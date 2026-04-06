using LinkPara.HttpProviders.Documents.Models;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

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
    public List<CorporateWalletUserDto> Users { get; set; }
}
