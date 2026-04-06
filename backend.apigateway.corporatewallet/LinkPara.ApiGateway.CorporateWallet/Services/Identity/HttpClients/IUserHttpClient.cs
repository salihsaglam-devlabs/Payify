using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;

public interface IUserHttpClient
{
    Task<UserDto> GetUserAsync(Guid userId);

    Task<UserDto> GetUserByUserNameAsync(string userName);

    Task<UserDto> GetUserAsync(GetUserRequest request);
    Task<ExistingUsersDto> GetExistingUserListAsync(GetExistingUsersRequest request);

    Task<PaginatedList<UserDto>> GetUsersAsync(UserFilterRequest filter);

    Task UpdateUserAsync(UpdateUserRequest user);
    Task<GetUserLoginActivityResponse> GetUserLoginActivity(Guid userId);
    Task<UserIdByUserNameResponse> GetUserIdByUserNameAsync(string userName);
    Task<List<UserAgreementDocumentsStatusDto>> GetUserDocumentsAsync(Guid userId, UserDocumentFilterRequest request);
    Task<List<RoleDto>> GetUserRolesAsync(Guid userId);
}