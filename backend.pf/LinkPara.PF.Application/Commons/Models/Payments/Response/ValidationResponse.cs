namespace LinkPara.PF.Application.Commons.Models.Payments.Response;

public class ValidationResponse
{
    public string Code { get; set; }
    public string Message { get; set; }
    public bool IsValid { get; set; }
}