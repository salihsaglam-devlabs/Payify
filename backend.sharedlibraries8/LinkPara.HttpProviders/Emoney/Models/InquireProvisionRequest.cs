using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.HttpProviders.Emoney.Models;

public class InquireProvisionRequest
{
    public string ConversationId { get; set; }
    public string ProvisionReference { get; set; }
}
