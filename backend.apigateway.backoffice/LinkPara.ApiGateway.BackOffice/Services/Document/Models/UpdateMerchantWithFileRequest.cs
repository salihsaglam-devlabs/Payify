using LinkPara.ApiGateway.BackOffice.Services.Document.Models;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdateMerchantWithFileRequest
{
    public UpdateMerchantRequest MerchantRequest { get; set; }
    public List<FormFileDto> FormFiles { get; set; }
}
