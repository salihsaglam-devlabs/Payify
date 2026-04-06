using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.HttpProviders.Identity;

public interface IUserService
{
    Task<UserDto> GetUserAsync(Guid userId);
    Task<PaginatedList<UserDto>> GetAllUsersAsync(GetUsersRequest request);
    Task<List<UserDto>> GetApplicationUserAsync(GetAppUsersRequest request);
    Task<UserCreateResponse> CreateUserAsync(CreateUserRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<List<UserDeviceInfoDto>> GetUserDeviceInfo(GetUserDeviceInfoRequest request);
    Task<UserDto> PatchAsync(Guid userId, [FromBody] JsonPatchDocument<PatchUserRequest> request);
    Task CreateUserAnswerAsync(CreateUserAnswerRequest request);
    Task UpdateUserAsync(UpdateUserWithUserName request);
    Task<GetUserLoginActivityResponse> GetUserLoginActivityAsync(Guid userId);
}