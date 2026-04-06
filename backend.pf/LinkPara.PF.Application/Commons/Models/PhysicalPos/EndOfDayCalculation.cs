namespace LinkPara.PF.Application.Commons.Models.PhysicalPos;

public class EndOfDayCalculation
{
    public int SaleCount { get; set; }
    public int VoidCount { get; set; }
    public int RefundCount { get; set; }
    public int InstallmentSaleCount { get; set; }
    public decimal SaleAmount { get; set; }
    public decimal VoidAmount { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal InstallmentSaleAmount { get; set; }
}