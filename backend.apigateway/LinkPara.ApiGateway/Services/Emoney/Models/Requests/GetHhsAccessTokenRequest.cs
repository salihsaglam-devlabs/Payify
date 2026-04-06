using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetHhsAccessTokenRequest
{
    public string ConsentId { get; set; }
    public ConsentType ConsentType { get; set; }
    public string AccessCode { get; set; }
}

