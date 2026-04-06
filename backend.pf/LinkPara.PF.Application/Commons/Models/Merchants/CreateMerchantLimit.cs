using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class CreateMerchantLimit
{
    public TransactionLimitType TransactionLimitType { get; set; }
    public Period Period { get; set; }
    public LimitType LimitType { get; set; }
    public int? MaxPiece { get; set; }
    public decimal? MaxAmount { get; set; }
    public string CurrencyCode { get; set; }
}