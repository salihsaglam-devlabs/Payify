namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;
public class CardDetailResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public CardDetailResponseDto Result { get; set; }
}