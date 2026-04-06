namespace LinkPara.Fraud.Application.Commons.Models;

public class BaseResponse
{
    public int HttpStatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}
