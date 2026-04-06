namespace LinkPara.PF.Application.Commons.Models.VposModels.Request;

public class PosPaymentDetailRequest : PosRequestBase
{
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string SubMerchantCode { get; set; }
    public string Password { get; set; }
    public string RRN { get; set; }
    public string ServiceProviderPspMerchantId { get; set; }
    public int Currency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
