namespace LinkPara.ApiGateway.Commons.MultiFactorModels;

public class OneTouchTransactionResponse
{
    public bool IsSuccess { get; set; }
    public string TransactionToken { get; set; }
}