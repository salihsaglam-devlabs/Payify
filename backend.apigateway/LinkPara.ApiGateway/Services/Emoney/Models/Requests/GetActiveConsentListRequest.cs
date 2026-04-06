using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetActiveConsentListRequest
{
    public string AccountId { get; set; }
    public ConsentType ConsentType { get; set; }
}

