using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients;

public interface IUserHttpClient
{
    Task<UserDto> GetUserAsync(Guid userId);

    Task<UserDto> GetUserByUserNameAsync(string userName);
    Task<bool> GetIsUserExistAsync(string userName);
    Task<UserDto> GetUserAsync(GetUserRequest request);
    Task<ExistingUsersDto> GetExistingUserListAsync(GetExistingUsersRequest request);

    Task<PaginatedList<UserDto>> GetUsersAsync(UserFilterRequest filter);

    Task UpdateUserAsync(UpdateUserRequest user);

    Task UpdateKycAsync(UpdateKycRequest user);
    
    Task<List<UserAgreementDocumentsStatusDto>> GetUserDocumentsAsync(Guid userId, UserDocumentFilterRequest request);
    Task<GetUserLoginActivityResponse> GetUserLoginActivity(Guid userId);
    Task<UserIdByUserNameResponse> GetUserIdByUserNameAsync(string userName);
}