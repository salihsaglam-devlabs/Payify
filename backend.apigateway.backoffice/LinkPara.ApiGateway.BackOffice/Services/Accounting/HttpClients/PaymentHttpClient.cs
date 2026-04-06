using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Accounting.HttpClients;

public class PaymentHttpClient : HttpClientBase, IPaymentHttpClient
{
    private readonly ILogger<PaymentHttpClient> _logger;
    public PaymentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor, ILogger<PaymentHttpClient> logger) 
        : base(client, httpContextAccessor)
    {
        _logger = logger;
    }

    public async Task DeletePaymentAsync(Guid id)
    {
        await DeleteAsync($"v1/Payments/delete/{id}");
    }

    public async Task<ActionResult<AccountingPaymentDto>> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/Payments/{id}");

        var result = await response.Content.ReadFromJsonAsync<AccountingPaymentDto>();
        if (!CanSeeSensitiveData())
        {
            result.SourceFullName = SensitiveDataHelper.MaskSensitiveData("FullName", result.SourceFullName);
            result.DestinationFullName = SensitiveDataHelper.MaskSensitiveData("FullName", result.DestinationFullName);
        }

        return result;
    }

    public async Task<ActionResult<PaginatedList<AccountingPaymentDto>>> GetListPaymentsAsync(GetFilterPaymentRequest request)
    {
        try
        {
            var url = CreateUrlWithParams($"v1/Payments", request, true);
            var response = await GetAsync(url);
            var payments = await response.Content.ReadFromJsonAsync<PaginatedList<AccountingPaymentDto>>();
            if (!CanSeeSensitiveData())
            {
                payments.Items.ForEach(s =>
                {
                    s.SourceFullName = SensitiveDataHelper.MaskSensitiveData("FullName", s.SourceFullName);
                    s.DestinationFullName = SensitiveDataHelper.MaskSensitiveData("FullName", s.DestinationFullName);
                });
            }
            return payments ?? throw new InvalidOperationException();
        }
        catch (Exception exception)
        {
            _logger.LogError($"ErrorOnGetPayments Exception : {exception}");
            throw;
        }
    }

    public async Task SavePaymentAsync(SavePaymentRequest request)
    {
        await PostAsJsonAsync("v1/Payments", request);
    }
}
