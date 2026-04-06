namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class CardsResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public CardsResponseDto Result { get; set; }
}