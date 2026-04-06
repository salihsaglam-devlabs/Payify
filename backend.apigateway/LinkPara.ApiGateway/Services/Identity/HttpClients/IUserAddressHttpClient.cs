using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients;

public interface IUserAddressHttpClient
{
    Task<List<UserAddressDto>> GetAddressByUserIdAsync(string userId);

    Task CreateAddressAsync(CreateAddressRequest request);

    Task UpdateAddressAsync(UpdateAddressRequest request);

}