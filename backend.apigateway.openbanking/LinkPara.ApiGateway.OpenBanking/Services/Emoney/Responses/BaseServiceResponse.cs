namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class BaseServiceResponse<T>
{
    public T Value { get; set; }
    public bool Success { get; set; }
    public string ExecutionReferenceId { get; set; }
}
