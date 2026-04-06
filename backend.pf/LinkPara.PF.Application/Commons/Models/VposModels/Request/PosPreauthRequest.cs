namespace LinkPara.PF.Application.Commons.Models.VposModels.Request;

public class PosPreauthRequest : PosRequestBase
{
    public string CardNumber { get; set; }
    public string Cvv2 { get; set; }
    public string ExpireMonth { get; set; }
    public string ExpireYear { get; set; }
    public decimal? Amount { get; set; }
    public decimal? BonusAmount { get; set; }
    public int? Installment { get; set; }
    public string OrderNumber { get; set; }
    public int Currency { get; set; }
    public string SubMerchantCode { get; set; }
}
