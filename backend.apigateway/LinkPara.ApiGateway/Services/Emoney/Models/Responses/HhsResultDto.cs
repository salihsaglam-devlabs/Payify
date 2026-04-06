namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class HhsResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public List<HhsDto> Result{ get; set; }
}