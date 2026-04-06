namespace LinkPara.Emoney.ApiGateway.Models.Requests;

public class ProvisionRequest
{
    public string WalletNumber { get; set; }

    public decimal Amount { get; set; }

    public string CurrencyCode { get; set; }

    public string Description { get; set; }

    public string ConversationId { get; set; }

    public string ClientIpAddress { get; set; }
    
}
