using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;

public interface IUserHttpClient
{
    Task<UserDto> GetUserByIdAsync(Guid userId);
    Task<ExistingUsersDto> GetExistingUserListAsync(GetExistingUsersRequest request);
    Task<UserDto> GetUserByUserNameAsync(string userName); 
    Task<PaginatedList<UserDtoWithRoles>> GetAllUsersAsync(GetUsersRequest request);
    Task<UserCreateResponse> CreateUserAsync(CreateUserWithUserName request);
    Task UpdateUserAsync(UpdateUserWithUserName request);
    Task<GetUserLoginActivityResponse> GetUserLoginActivity(Guid userId);
    Task<List<UserAgreementDocumentsStatusDto>> GetUserDocumentsAsync(Guid userId);
    Task<bool> VerifyEmailAsync(VerifyEmailRequest request);
}