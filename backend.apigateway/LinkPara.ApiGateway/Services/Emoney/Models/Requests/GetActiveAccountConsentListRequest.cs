using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetActiveAccountConsentListRequest
{
    public string AppUserId { get; set; }
}

