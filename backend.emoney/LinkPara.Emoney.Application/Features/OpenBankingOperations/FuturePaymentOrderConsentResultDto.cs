namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class FuturePaymentOrderConsentResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public FuturePaymentOrderConsentResponseDto Result { get; set; }
}