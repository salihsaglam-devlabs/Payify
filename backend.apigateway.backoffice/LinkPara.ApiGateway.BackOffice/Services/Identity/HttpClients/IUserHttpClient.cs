using LinkPara.ApiGateway.BackOffice.Commons.Models.AuthorizationModels;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;

public interface IUserHttpClient
{
    Task<PaginatedList<UserDtoWithRoles>> GetAllUsersAsync(GetUsersRequest request);
   
    Task<ExistingUsersDto> GetExistingUserListAsync(GetExistingUsersRequest request);

    Task<UserDto> GetUserByIdAsync(Guid userId);

    Task AssignUserRoleAsync(Guid userId, UserRoleDto request);

    Task<List<RoleDto>> GetUserRolesAsync(Guid userId);

    Task<List<ClaimDto>> GetUserClaimsAsync(Guid userId);

    Task<UserCreateResponse> CreateUserAsync(CreateUserWithUserName request);

    Task UpdateUserAsync(UpdateUserWithUserName request);

    Task<UserDto> PatchUserAsync(Guid userId, JsonPatchDocument<PatchUserRequest> request);

    Task<TokenResponse> GetUserTokenAsync(Guid userId, string secureKey);

    Task<UserDto> GetUserByUserNameAsync(string userName);

    Task<UserDetailDto> GetUserDetailsAsync(Guid userId);
    Task<GetUserLoginActivityResponse> GetUserLoginActivityAsync(Guid userId);
    Task RemoveUserLockAsync(Guid userId);
    Task<bool> ResendEmailVerifyAsync(ResendEmailVerifyRequest request);
    Task ResetUserSecurityPictureAsync(Guid userId);
}