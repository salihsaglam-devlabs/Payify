using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class TopupUpdateStatusRequest
{
    public Guid CardTopupRequestId { get; set; }
    public CardTopupRequestStatus Status { get; set; }
}
