using LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Boa.Services.Identity.HttpClients;

public interface IUserHttpClient
{
    Task<PaginatedList<UserDto>> GetUsersAsync(UserFilterRequest filter);
}