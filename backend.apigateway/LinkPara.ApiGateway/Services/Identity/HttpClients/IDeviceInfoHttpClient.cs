using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients
{
    public interface IDeviceInfoHttpClient
    {
        Task CreateAsync(CreateDeviceInfoRequest device);
        Task DeleteAsync(DeleteDeviceRequest device);
    }
}
