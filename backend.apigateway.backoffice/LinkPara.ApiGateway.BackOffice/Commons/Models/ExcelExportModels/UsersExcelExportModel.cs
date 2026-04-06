using LinkPara.ApiGateway.BackOffice.Commons.Mappings;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Commons.Models.ExcelExportModels
{
    public class UsersExcelExportModel : IMapFrom<UserDto>
    {
        public string UserType { get; set; }
        public string UserStatus { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}

