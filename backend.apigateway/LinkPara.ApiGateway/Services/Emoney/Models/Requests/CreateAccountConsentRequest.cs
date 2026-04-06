using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class CreateAccountConsentRequest
{
    public string HhsCode { get; set; }
    public string AppUserId { get; set; }
    public YosForwardType ForwardType { get; set; }
    public DateTime AccessExpireDate { get; set; }
    public List<string> PermissionTypes { get; set; }
}

public class AccountConsentIdentityInfo
{
    public string OhkTur { get; set; }
    public string KmlkTur { get; set; }
    public string KmlkVrs{ get; set; }
    public string KrmKmlkTur{ get; set; }
    public string KrmKmlkVrs{ get; set; }
}

