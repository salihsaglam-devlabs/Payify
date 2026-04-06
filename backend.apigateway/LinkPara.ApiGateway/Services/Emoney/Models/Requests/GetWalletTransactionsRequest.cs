using System.ComponentModel.DataAnnotations;
using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetWalletTransactionsRequest : SearchQueryParams
{
    public Guid WalletId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public TransactionDirection? Direction { get; set; }
    public TransactionType?[] TransactionTypes { get; set; }
}