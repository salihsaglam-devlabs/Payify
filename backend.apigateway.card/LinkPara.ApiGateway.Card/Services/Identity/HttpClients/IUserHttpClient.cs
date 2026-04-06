using LinkPara.ApiGateway.Card.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Card.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Card.Services.Identity.HttpClients;

public interface IUserHttpClient
{
    Task<PaginatedList<UserDto>> GetUsersAsync(UserFilterRequest filter);
}