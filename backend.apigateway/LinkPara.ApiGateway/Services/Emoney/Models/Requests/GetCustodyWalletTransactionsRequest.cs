using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetCustodyWalletTransactionsRequest : SearchQueryParams
{
    public Guid ParentAccountId { get; set; }
    public Guid WalletId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-30).Add(-DateTime.Now.TimeOfDay);
    public DateTime EndDate { get; set; } = DateTime.Now;
}