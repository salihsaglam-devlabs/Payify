using Elastic.Apm.Api;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients
{
    public class DeviceInfoHttpClient : HttpClientBase, IDeviceInfoHttpClient
    {
        public DeviceInfoHttpClient(HttpClient client,
            IHttpContextAccessor httpContextAccessor)
            : base(client, httpContextAccessor)
        {
        }

        public async Task CreateAsync(CreateDeviceInfoRequest device)
        {
            await PostAsJsonAsync($"v1/DeviceInfo", device);
        }

        public async Task DeleteAsync(DeleteDeviceRequest device)
        {
            await PostAsJsonAsync($"v1/DeviceInfo/delete", device);
        }
    }
}
