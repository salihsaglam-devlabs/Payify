using LinkPara.Emoney.ApiGateway.Models.Enums;

namespace LinkPara.Emoney.ApiGateway.Models.Responses;

public class InquireProvisionResponse
{
    public string ConversationId { get; set; }
    public string WalletNumber { get; set; }
    public string Description { get; set; }
    public ProvisionStatus ProvisionStatus { get; set; }
}
