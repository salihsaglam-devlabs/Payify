using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients;

public interface IUserInboxHttpClient
{
    Task<List<UserInboxDto>> GetUserInboxAsync(UserInboxRequest request);
    Task UpdateReadedUserInboxAsync(UserInboxRequest request);
    Task DeleteSelectedAsync(DeleteUserInboxRequest request);
}