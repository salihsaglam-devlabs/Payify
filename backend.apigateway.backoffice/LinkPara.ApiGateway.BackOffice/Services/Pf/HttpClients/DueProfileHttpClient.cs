using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public class DueProfileHttpClient : HttpClientBase, IDueProfileHttpClient
    {
        public DueProfileHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
            : base(client, httpContextAccessor)
        {
        }

        public async Task CreateAsync(CreateDueProfileRequest request)
        {
            var response = await PostAsJsonAsync($"v1/DueProfile", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }

        public async Task DeleteAsync(Guid id)
        {
            var response = await DeleteAsync($"v1/DueProfile/{id}");
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }

        public async Task<DueProfileDto> GetByIdAsync(Guid id)
        {
            var response = await GetAsync($"v1/DueProfile/{id}");
            var dueProfile = await response.Content.ReadFromJsonAsync<DueProfileDto>();
            return dueProfile ?? throw new InvalidOperationException();
        }

        public async Task<PaginatedList<DueProfileDto>> GetFilterListAsync(GetFilterDueProfileRequest request)
        {
            var url = CreateUrlWithParams($"v1/DueProfile", request, true);
            var response = await GetAsync(url);
            var acquireBanks = await response.Content.ReadFromJsonAsync<PaginatedList<DueProfileDto>>();
            return acquireBanks ?? throw new InvalidOperationException();
        }

        public async Task UpdateAsync(UpdateDueProfileRequest request)
        {
            var response = await PutAsJsonAsync($"v1/DueProfile", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }
    }
}
