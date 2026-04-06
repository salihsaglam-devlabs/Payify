using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests
{
    public class UpdateRoleScreenRequest
    {
        public string RoleId { get; set; }
        public Guid[] ScreensId { get; set; }
        public string Name { get; set; }
        public bool CanSeeSensitiveData { get; set; }
        public RoleScope RoleScope { get; set; }
    }
}
