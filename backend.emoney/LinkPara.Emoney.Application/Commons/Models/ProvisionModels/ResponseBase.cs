namespace LinkPara.Emoney.Application.Commons.Models.ProvisionModels;

public class ResponseBase
{
    public bool IsSucceed { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}