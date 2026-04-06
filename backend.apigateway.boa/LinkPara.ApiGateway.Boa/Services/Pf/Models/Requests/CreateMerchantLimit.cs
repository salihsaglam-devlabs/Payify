using LinkPara.ApiGateway.Boa.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;

public class CreateMerchantLimit
{
    public TransactionLimitType TransactionLimitType { get; set; }
    public Period Period { get; set; }
    public LimitType LimitType { get; set; }
    public int? MaxPiece { get; set; }
    public decimal? MaxAmount { get; set; }
    public string CurrencyCode { get; set; }
}