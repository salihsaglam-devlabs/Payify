using LinkPara.ApiGateway.Boa.Commons.Helpers;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
public class UpdateBalanceRequest
{
    public TransactionType TransactionType { get; set; }
    public TransactionDirection TransactionDirection { get; set; }
    public string Channel { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CurrencyCode { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal FeeAmount { get; set; }
    public string Utid { get; set; }
    public string Description { get; set; }
    public string WalletNumber { get; set; }
    public bool IsBalanceControl { get; set; }
}
