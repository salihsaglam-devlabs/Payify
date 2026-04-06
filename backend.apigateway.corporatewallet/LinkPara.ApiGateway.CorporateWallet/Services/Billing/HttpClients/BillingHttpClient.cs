using LinkPara.ApiGateway.CorporateWallet.Commons.Extensions;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;

public class BillingHttpClient : HttpClientBase, IBillingHttpClient
{
    private readonly IWalletHttpClient _walletHttpClient;

    public BillingHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor, IWalletHttpClient walletHttpClient)
        : base(client, httpContextAccessor)
    {
        _walletHttpClient = walletHttpClient;
    }

    public async Task<BillPreviewResponseDto> BillPreviewAsync(BillPreviewRequest request, Guid userId)
    {
        var userWallets = await _walletHttpClient.GetUserWalletsAsync(new GetUserWalletsFilterRequest { });

        if (userWallets.Any(u => u.WalletNumber.Equals(request.WalletNumber)))
        {
            var response = await PostAsJsonAsync($"v1/Billings/bill-preview", request);

            return await response.Content.ReadFromJsonAsync<BillPreviewResponseDto>();
        }
        throw new ForbiddenAccessException();
    }

    public async Task<BillCancelResponseDto> CancelBillPaymentAsync(BillCancelRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Billings/cancel-bill-payment", request);

        return await response.Content.ReadFromJsonAsync<BillCancelResponseDto>();
    }

    public async Task<BillInquiryResponseDto> InquireBillAsync(BillInquiryRequest request)
    {
        var queryString = request.GetQueryString();

        var response = await GetAsync($"v1/Billings/inquire-bill" + queryString);

        return await response.Content.ReadFromJsonAsync<BillInquiryResponseDto>();
    }

    public async Task<BillPaymentResponseDto> PayInquiredBillAsync(BillPaymentRequest request, Guid userId)
    {
        var userWallets = await _walletHttpClient.GetUserWalletsAsync(new GetUserWalletsFilterRequest { });

        if (userWallets.Any(u => u.WalletNumber.Equals(request.WalletNumber)))
        {
            var response = await PostAsJsonAsync($"v1/Billings/pay-inquired-bill", request);

            return await response.Content.ReadFromJsonAsync<BillPaymentResponseDto>();
        }
        throw new ForbiddenAccessException();
    }
}
