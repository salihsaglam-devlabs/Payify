using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public class BillingCommissionHttpClient : HttpClientBase, IBillingCommissionHttpClient 
{
    public BillingCommissionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<BillingCommissionDto>> GetCommissionsAsync(GetAllBillingCommissionRequest request)
    {
        var url = CreateUrlWithParams($"v1/Commissions", request, true);   
        var response = await GetAsync(url);
        
        return await response.Content.ReadFromJsonAsync<PaginatedList<BillingCommissionDto>>();
    }
    
    public async Task<BillingCommissionDto> GetCommissionAsync(Guid commissionId)
    {
        var url = $"v1/Commissions/{commissionId}";
        var response = await GetAsync(url);
        
        return await response.Content.ReadFromJsonAsync<BillingCommissionDto>();
    }
    
    public async Task DeleteCommissionAsync(Guid id)
    {
        var url = $"v1/Commissions/{id}";
        
        await DeleteAsync(url);
    }
    
    public async Task CreateCommissionAsync(CreateBillingCommissionRequest request)
    {
        var url = $"v1/Commissions";
        
        var response = await PostAsJsonAsync(url, request);
    }
}