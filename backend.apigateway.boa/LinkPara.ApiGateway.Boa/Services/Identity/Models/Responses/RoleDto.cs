using LinkPara.ApiGateway.Boa.Services.Identity.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Boa.Services.Identity.Models.Responses;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public RoleScope RoleScope { get; set; }
    public bool CanSeeSensitiveData { get; set; }
    public RecordStatus RecordStatus { get; set; }
}