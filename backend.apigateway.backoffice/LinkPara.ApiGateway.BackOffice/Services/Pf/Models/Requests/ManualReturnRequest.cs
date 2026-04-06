using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class ManualReturnRequest
{
    public Guid MerchantTransactionId { get; set; }
    public decimal Amount { get; set; }
    public List<MerchantDocumentDto> Files { get; set; }
}