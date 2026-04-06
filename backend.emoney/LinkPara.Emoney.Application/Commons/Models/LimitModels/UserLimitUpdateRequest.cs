using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.LimitModels;

public class AccountLimitUpdateRequest
{
    public Guid AccountId { get; set; }
    public LimitOperationType LimitOperationType { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public WalletType WalletType { get; set; }
    public bool? IsOwnIban { get; set; }
    public bool? IsDailyDistinctIban { get; set; }
    public bool? IsMonthlyDistinctIban { get; set; }
}