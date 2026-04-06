using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests
{
    public class AllActiveUserRequest : SearchQueryParams
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneCode { get; set; }
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string UserName { get; set; }
    }
}
