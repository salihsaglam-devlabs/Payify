using LinkPara.ApiGateway.BackOffice.Services.Notification.Models;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public interface IUserSimBlockageHttpClient
{
    Task<PaginatedList<UserSimBlockageDto>> GetUserSimBlockageListAsync(GetUserSimBlockageRequest request);
    Task RemoveUserSimBlockageAsync(RemoveUserSimBlockageRequest request);
}
