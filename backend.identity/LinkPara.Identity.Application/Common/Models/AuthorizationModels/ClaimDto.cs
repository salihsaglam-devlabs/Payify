using LinkPara.SharedModels.Authorization.Models;

namespace LinkPara.Identity.Application.Common.Models.AuthorizationModels;

public class ClaimDto
{
    public string Type { get; set; }
    public string[] Values { get; set; }
}