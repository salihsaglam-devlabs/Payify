namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class CardDetailResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public CardDetailResponseDto Result { get; set; }
}