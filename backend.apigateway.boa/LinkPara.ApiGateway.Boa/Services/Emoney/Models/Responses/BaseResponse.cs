namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

public class BaseResponse<T>
{
    public string Version { get; set; }
    public string BuildId { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string CorrelationId { get; set; }
    public string RequestId { get; set; }
    public T Result { get; set; }
    public ExceptionResponse Exception { get; set; }
}

public class ExceptionResponse
{
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}
