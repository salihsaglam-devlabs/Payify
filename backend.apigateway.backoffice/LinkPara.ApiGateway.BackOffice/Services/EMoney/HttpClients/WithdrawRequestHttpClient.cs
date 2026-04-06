using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class WithdrawRequestHttpClient : HttpClientBase, IWithdrawRequestHttpClient
{
    public WithdrawRequestHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<WithdrawRequestDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/WithdrawRequests/{id}");
        var withdrawRequest = await response.Content.ReadFromJsonAsync<WithdrawRequestDto>();
        if (!CanSeeSensitiveData())
        {
            withdrawRequest.ReceiverIbanNumber = SensitiveDataHelper.MaskSensitiveData("Iban", withdrawRequest.ReceiverIbanNumber);
            withdrawRequest.ReceiverName = SensitiveDataHelper.MaskSensitiveData("FullName", withdrawRequest.ReceiverName);
            withdrawRequest.ReceiverTaxNumber = SensitiveDataHelper.MaskSensitiveData("TaxNumber", withdrawRequest.ReceiverTaxNumber);
            withdrawRequest.SenderName = SensitiveDataHelper.MaskSensitiveData("FullName", withdrawRequest.SenderName);
        }
        return withdrawRequest ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<WithdrawRequestAdminDto>> GetWithdrawRequestListAsync(GetWithdrawRequestListRequest request)
    {
        var url = CreateUrlWithParams($"v1/WithdrawRequests", request, true);
        var response = await GetAsync(url);
        var withdraws = await response.Content.ReadFromJsonAsync<PaginatedList<WithdrawRequestAdminDto>>();
        if (!CanSeeSensitiveData())
        {
            withdraws.Items.ForEach(s =>
            {
                s.ReceiverIbanNumber = SensitiveDataHelper.MaskSensitiveData("Iban", s.ReceiverIbanNumber);
                s.ReceiverName = SensitiveDataHelper.MaskSensitiveData("FullName", s.ReceiverName);
            });
        }
        return withdraws ?? throw new InvalidOperationException();
    }
}
