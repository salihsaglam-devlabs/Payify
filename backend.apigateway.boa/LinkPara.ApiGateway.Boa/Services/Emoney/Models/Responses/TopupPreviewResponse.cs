namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

public class TopupPreviewResponse
{
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal BsmvTotal { get; set; }
    public decimal BsmvRate { get; set; }
    public decimal Fee { get; set; }
    public string WalletNumber { get; set; }
    public string FullName { get; set; }
    public decimal TotalAmount { get; set; }
}