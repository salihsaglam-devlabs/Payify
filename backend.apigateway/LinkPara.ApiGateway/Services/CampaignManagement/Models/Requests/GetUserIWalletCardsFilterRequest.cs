using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.CampaignManagement.Models.Requests;

public class GetUserIWalletCardsFilterRequest
{
}

public class GetUserIWalletCardsFilterServiceRequest : GetUserIWalletCardsFilterRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
