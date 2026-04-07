namespace LinkPara.Card.Domain.Enums;

public static class ReconciliationOperationCodeExtensions
{
    public static string ToOperationCode(this ReconciliationOperationCode operationCode)
    {
        return operationCode.ToString();
    }
}
