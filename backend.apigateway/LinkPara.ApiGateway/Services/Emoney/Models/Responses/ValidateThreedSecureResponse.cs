namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class ValidateThreedSecureResponse
{
    public Guid? CardTopupRequestId { get; set; }
    public bool? IsValid { get; set; }
}
