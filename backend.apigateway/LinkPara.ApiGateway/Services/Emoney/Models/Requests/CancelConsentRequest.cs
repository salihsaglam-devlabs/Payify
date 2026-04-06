using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class CancelConsentRequest
{
    public string ConsentId { get; set; }
    public string Username { get; set; }
    public ConsentType ConsentTypeValue { get; set; }
    public string RevokeCode { get; set; }
    public bool IsDecoupledAuth { get; set; }

}

