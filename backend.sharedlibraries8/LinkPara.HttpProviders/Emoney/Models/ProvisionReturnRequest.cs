using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.HttpProviders.Emoney.Models;

public class ProvisionReturnRequest
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public ProvisionSource ProvisionSource { get; set; }
    public string Description { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string ProvisionReferenceNumber { get; set; }
}
