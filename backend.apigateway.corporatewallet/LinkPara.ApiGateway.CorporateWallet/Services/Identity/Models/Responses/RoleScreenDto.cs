using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses
{
    public class RoleScreenDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; }
        public RoleScope RoleScope { get; set; }
        public bool CanSeeSensitiveData { get; set; }
        public List<ScreenDto> Screens { get; set; }
    }
}
