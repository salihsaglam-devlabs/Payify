using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
public class TransactionHttpClient : HttpClientBase, ITransactionHttpClient
{
    public TransactionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
    : base(client, httpContextAccessor)
    {
    }

    public async Task<TransactionAdminDto> GetAdminTransactionAsync(Guid id)
    {
        var response = await GetAsync($"v1/Transactions/{id}");
        var transaction = await response.Content.ReadFromJsonAsync<TransactionAdminDto>();
        if (!CanSeeSensitiveData())
        {
            transaction.SenderName = SensitiveDataHelper.MaskSensitiveData("FullName", transaction.SenderName);
            transaction.ReceiverName = SensitiveDataHelper.MaskSensitiveData("FullName", transaction.ReceiverName);
        }
        return transaction ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<TransactionAdminDto>> GetAdminTransactionsAsync(GetTransactionsRequest request)
    {
        var url = CreateUrlWithParams($"v1/Transactions", request, true);
        var response = await GetAsync(url);
        var transactions = await response.Content.ReadFromJsonAsync<PaginatedList<TransactionAdminDto>>();
        if (!CanSeeSensitiveData())
        {
            transactions.Items.ForEach(transaction =>
            {
                transaction.SenderName = SensitiveDataHelper.MaskSensitiveData("FullName", transaction.SenderName);
                transaction.ReceiverName = SensitiveDataHelper.MaskSensitiveData("FullName", transaction.ReceiverName);
            });
        }
        return transactions ?? throw new InvalidOperationException();
    }
}

