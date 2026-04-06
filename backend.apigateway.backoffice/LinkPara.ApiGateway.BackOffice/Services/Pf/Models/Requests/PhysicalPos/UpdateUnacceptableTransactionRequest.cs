using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums.PhysicalPos;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class UpdateUnacceptableTransactionRequest
{
    public UnacceptableTransactionStatus CurrentStatus {get; set;}
}