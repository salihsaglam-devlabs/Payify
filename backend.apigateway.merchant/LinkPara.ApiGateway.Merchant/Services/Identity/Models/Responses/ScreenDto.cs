using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses
{
    public class ScreenDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Module { get; set; }
        public string Icon { get; set; }
        public string ModuleIcon { get; set; }
        public string Link { get; set; }
        public string ModuleLink { get; set; }
        public int ModulePriority { get; set; }
        public int Priority { get; set; }
        public PermissionOperationType OperationType { get; set; }
    }
}
