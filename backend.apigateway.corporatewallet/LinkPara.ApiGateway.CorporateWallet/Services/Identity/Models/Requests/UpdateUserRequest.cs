using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;
using LinkPara.ApiGateway.CorporateWallet.Commons.Mappings;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;

public class UpdateUserRequest
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserType UserType { get; set; }
    public List<Guid> Roles { get; set; }
    public RecordStatus RecordStatus { get; set; }
}

public class UpdateUserServiceRequest : UpdateUserRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
