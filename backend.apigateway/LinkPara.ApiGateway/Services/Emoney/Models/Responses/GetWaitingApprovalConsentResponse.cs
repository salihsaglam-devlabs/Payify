using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class GetWaitingApprovalConsentResponse
{
    public List<WaitingApprovalConsentDto> Value { get; set; }
}
