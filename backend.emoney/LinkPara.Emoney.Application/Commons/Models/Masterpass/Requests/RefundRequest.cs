namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Requests;

public class RefundRequest
{
    public int MerchantId { get; set; }
    public string OrderId { get; set; }
    public string OrderDate { get; set; }
    public string TerminalGroupId { get; set; }
    public string Amount { get; set; }
}