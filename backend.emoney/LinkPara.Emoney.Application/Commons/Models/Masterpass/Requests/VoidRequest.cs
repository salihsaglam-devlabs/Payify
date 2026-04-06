namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Requests;

public class VoidRequest
{
    public int MerchantId { get; set; }
    public string OrderId { get; set; }
    public string TransactionDate { get; set; }
    public string TerminalGroupId { get; set; }
}