namespace LinkPara.Emoney.Application.Commons.Helpers;

public static class CalculationHelper
{
    public static decimal ToDecimal2(this decimal amount)
    {
        return Math.Round(amount, 2);
    }
}
