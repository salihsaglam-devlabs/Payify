namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;
public class ActiveAccountConsentResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public List<ActiveAccountConsentDto> Result { get; set; }
}