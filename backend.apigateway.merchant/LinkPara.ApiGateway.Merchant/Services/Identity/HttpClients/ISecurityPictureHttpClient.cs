using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;

public interface ISecurityPictureHttpClient
{
    Task<List<SecurityPictureDto>> GetAllAsync();
    Task SelectPictureAsync(Guid pictureId, CreateUserSecurityPictureRequest request);
}