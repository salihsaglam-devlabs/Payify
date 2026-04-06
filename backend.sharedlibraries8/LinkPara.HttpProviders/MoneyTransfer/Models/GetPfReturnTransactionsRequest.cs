using LinkPara.HttpProviders.MoneyTransfer.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.MoneyTransfer.Models;
public class GetPfReturnTransactionsRequest : SearchQueryParams
{
    public string OrderNumber { get; set; }
    public string MerchantNumber { get; set; }
    public string SenderName { get; set; }
    public string SenderIban { get; set; }
    public string Description { get; set; }
    public string BankReferenceNumber { get; set; }
    public PfReturnTransactionStatus? TransactionStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
}
