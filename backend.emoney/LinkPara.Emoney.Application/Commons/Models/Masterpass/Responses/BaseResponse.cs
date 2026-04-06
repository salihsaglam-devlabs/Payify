namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Responses;

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
    public string Level { get; set; }
    public string Code { get; set; }
    public string Message { get; set; }
    public List<ValidationError> ValidationErrors { get; set; } = new List<ValidationError>();
    public List<string> Details { get; set; } = new List<string>();
    public string VposErrorCode { get; set; }
    public string VposErrorMessage { get; set; }
    public string RetrievalReferenceNumber { get; set; }
    public string ThirdPartyApiResponse { get; set; }
    public string AcquirerIcaNumber { get; set; }
}

public class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }
    public string Code { get; set; }
}
