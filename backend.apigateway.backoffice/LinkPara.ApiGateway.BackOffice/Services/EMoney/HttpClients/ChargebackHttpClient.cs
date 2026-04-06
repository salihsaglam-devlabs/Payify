using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients
{
    public class ChargebackHttpClient : HttpClientBase, IChargebackHttpClient
    {
        public ChargebackHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
        {
        }

        public async Task<PaginatedList<ChargebackDto>> GetChargebackAsync(GetChargebackRequest request)
        {
            var queryString = request.GetQueryString();
            var response = await GetAsync($"v1/Chargeback" + queryString);
            var responseString = await response.Content.ReadAsStringAsync();
            var chargebackList = JsonSerializer.Deserialize<PaginatedList<ChargebackDto>>(responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return chargebackList ?? throw new InvalidOperationException();
        }

        public async Task<ChargebackDto> InitializeChargebackAsync(InitChargebackRequest request)
        {
            var response = await PostAsJsonAsync($"v1/Chargeback/init", request);
            var responseString = await response.Content.ReadAsStringAsync();
            var chargeback = JsonSerializer.Deserialize<ChargebackDto>(responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return chargeback;
        }

        public async Task<ChargebackDto> ApproveChargebackAsync(ApproveChargebackRequest request)
        {
            var response = await PutAsJsonAsync($"v1/Chargeback/approve", request);
            return await response.Content.ReadFromJsonAsync<ChargebackDto>();
        }

        public async Task<ChargebackDocumentDto> AddChargebackDocumentAsync(AddChargebackDocumentRequest request)
        {
            var response = await PostAsJsonAsync($"v1/Chargeback/add-document", request);
            var responseString = await response.Content.ReadAsStringAsync();
            var chargeback = JsonSerializer.Deserialize<ChargebackDocumentDto>(responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return chargeback;
        }

        public async Task<bool> DeleteChargebackDocumentAsync(DeleteChargebackDocumentRequest request)
        {
            var response = await PutAsJsonAsync($"v1/Chargeback/delete-document", request);
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<List<ChargebackDocumentDto>> GetChargebackDocumentsAsync(GetChargebackDocumentRequest request)
        {
            var queryString = request.GetQueryString();
            var response = await GetAsync($"v1/Chargeback/get-documents" + queryString);
            var responseString = await response.Content.ReadAsStringAsync();
            var chargebackDocumentList = JsonSerializer.Deserialize<List<ChargebackDocumentDto>>(responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return chargebackDocumentList ?? throw new InvalidOperationException();
        }
    }
}
