using LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.Models.Responses;

public class ChargeTransactionResponse
{
    public string FullName { get; set; }
    public string WalletNumber { get; set; }
    public SourceCampaignType SourceCampaignType { get; set; }
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid ProcessGuid { get; set; }
    public ChargeTransactionType ChargeTransactionType { get; set; }
    public string MerchantName { get; set; }
}
