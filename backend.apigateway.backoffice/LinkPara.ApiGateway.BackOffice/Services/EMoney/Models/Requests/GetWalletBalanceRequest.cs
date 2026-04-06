using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetWalletBalanceRequest : SearchQueryParams
{
    public string AccountName { get; set; }
    public Guid? AccountId { get; set; }
    public string CurrencyCode { get; set; }
    public string WalletNumber { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? TransactionDate { get; set; }
    public WalletBlockageStatus? BlockageStatus { get; set; }
}
