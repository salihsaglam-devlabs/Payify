using LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.Models.Requests;

public class GetChargeTransactionsSearchRequest : SearchQueryParams
{
    public string FullName { get; set; }
    public string WalletNumber { get; set; }
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
    public ChargeTransactionType? ChargeTransactionType { get; set; }
    public SourceCampaignType? SourceCampaignType { get; set; }
}
