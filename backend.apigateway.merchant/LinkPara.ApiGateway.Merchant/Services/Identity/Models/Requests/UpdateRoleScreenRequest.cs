using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests
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
