namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class GetFuturePaymentOrderListResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public List<GetFuturePaymentOrderListResponseDto> Result { get; set; }
}