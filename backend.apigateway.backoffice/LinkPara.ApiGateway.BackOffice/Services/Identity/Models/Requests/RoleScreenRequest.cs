using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests
{
    public class RoleScreenRequest
    {
        public Guid[] ScreensId { get; set; }
        public string Name { get; set; }
        public bool CanSeeSensitiveData { get; set; }
        public RoleScope RoleScope { get; set; }
    }
}
