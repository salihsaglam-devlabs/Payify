namespace LinkPara.ApiGateway.Services.Identity.Models.Responses;

public class SoftOtpApprovementDto
{
    public DateTime? CreateDate { get; set; }
    public string TransactionToken { get; set; }
    public string PushToken { get; set; }
}