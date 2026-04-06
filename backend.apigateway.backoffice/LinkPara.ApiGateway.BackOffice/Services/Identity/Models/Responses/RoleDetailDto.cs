using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;

public class RoleDetailDto
{
    public string Name { get; set; }
    public RoleScope RoleScope { get; set; }
    public bool CanSeeSensitiveData { get; set; }
    public List<string> Claims { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
