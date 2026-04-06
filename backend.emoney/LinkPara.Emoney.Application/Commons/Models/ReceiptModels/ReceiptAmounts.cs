namespace LinkPara.Emoney.Application.Commons.Models.ReceiptModels;

public sealed class ReceiptAmounts
{
    public decimal Amount { get; set; }
    public decimal Commission { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Kmv { get; set; }
    public string TotalAmountText { get; set; }
}

