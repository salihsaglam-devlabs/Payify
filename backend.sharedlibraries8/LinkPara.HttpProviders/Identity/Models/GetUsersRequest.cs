using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.Identity.Models
{
    public class GetUsersRequest : SearchQueryParams
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdentityNumber { get; set; }
        public string UserName { get; set; }
        public UserType? UserType { get; set; }
        public UserStatus? UserStatus { get; set; }
        public RecordStatus? RecordStatus { get; set; }
    }
}
