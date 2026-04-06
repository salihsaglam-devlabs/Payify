namespace LinkPara.IWallet.ApiGateway.Models.Responses;

public class BaseResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string Message { get; set; }
}
