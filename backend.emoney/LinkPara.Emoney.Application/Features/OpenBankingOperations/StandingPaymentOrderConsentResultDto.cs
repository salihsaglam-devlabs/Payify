namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class StandingPaymentOrderConsentResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public StandingPaymentOrderConsentResponseDto Result { get; set; }
}