using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients
{
    public class QuestionHttpClient : HttpClientBase, IQuestionHttpClient
    {
        public QuestionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
             : base(client, httpContextAccessor)
        {
        }

        public async Task<PaginatedList<SecurityQuestionDto>> GetAllSecurityQuestionAsync(GetAllSecurityQuestionRequest request)
        {
            var url = CreateUrlWithParams($"v1/Questions/getAllQuestion", request, true);
            var response = await GetAsync(url);
            var questionList = await response.Content.ReadFromJsonAsync<PaginatedList<SecurityQuestionDto>>();
            return questionList ?? throw new InvalidOperationException();
        }

        public async Task SaveAsync(SecurityQuestionRequest request)
        {
            var response = await PostAsJsonAsync($"v1/Questions", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }
        public async Task UpdateAsync(UpdateSecurityQuestionRequest request)
        {
            var response = await PutAsJsonAsync($"v1/Questions", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }
        public async Task DeleteSecurityQuestionAsync(Guid id)
        {
            var response = await DeleteAsync($"v1/Questions/{id}");
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }
        public async Task<SecurityQuestionDto> GetSecurityQuestionByIdAsync(Guid id)
        {
            var response = await GetAsync($"v1/Questions/{id}");
            var securityQuestion = await response.Content.ReadFromJsonAsync<SecurityQuestionDto>();
            return securityQuestion ?? throw new InvalidOperationException();
        }

    }
}
