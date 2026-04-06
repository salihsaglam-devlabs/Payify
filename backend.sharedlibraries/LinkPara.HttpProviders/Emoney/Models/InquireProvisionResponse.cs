using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.HttpProviders.Emoney.Models;

public class InquireProvisionResponse
{
    public string ConversationId { get; set; }
    public string WalletNumber { get; set; }
    public string Description { get; set; }
    public ProvisionStatus ProvisionStatus { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal TotalReturnedAmount { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorDescription { get; set; }
}