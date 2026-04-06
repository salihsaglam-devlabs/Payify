using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.Accounts
{
    public class PatchAccountUserDto : IMapFrom<AccountUser>
    {
        public Guid UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}
