namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Responses;

public class ValidateThreedSecureResponse
{
    public Guid? CardTopupRequestId { get; set; }
    public bool? IsValid { get; set; }
}