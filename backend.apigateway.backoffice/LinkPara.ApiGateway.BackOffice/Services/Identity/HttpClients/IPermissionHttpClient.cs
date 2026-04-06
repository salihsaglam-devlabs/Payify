using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;

public interface IPermissionHttpClient
{
    Task<List<PermissionDto>> GetAllPermissionsAsync();

    Task UpdateAsync(Guid permissionId, UpdatePermissionRequest request);

    Task SyncPermissionsAsync();
}