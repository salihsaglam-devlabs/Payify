using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.Identity.Models;

public class RoleDetailDto
{
    public string Name { get; set; }
    public RoleScope RoleScope { get; set; }
    public bool CanSeeSensitiveData { get; set; }
    public List<string> Claims { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
