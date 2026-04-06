using DocumentFormat.OpenXml.Office2010.Excel;
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class MoneyTransferTransactionsHttpClient : HttpClientBase, IMoneyTransferTransactionsHttpClient
{
    public MoneyTransferTransactionsHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }
    public async Task<MoneyTransferTransactionsDto> GetTransactionByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/Transactions/{id}");
        var transaction = await response.Content.ReadFromJsonAsync<MoneyTransferTransactionsDto>();
        if (!CanSeeSensitiveData())
        {
            transaction.ReceiverIbanNumber = SensitiveDataHelper.MaskSensitiveData("Iban", transaction.ReceiverIbanNumber);
            transaction.ReceiverName = SensitiveDataHelper.MaskSensitiveData("FullName", transaction.ReceiverName);
        }
        return transaction;
    }

    public async Task<PaginatedList<MoneyTransferTransactionsDto>> GetTransactionsAsync(
        GetMoneyTransferTransactionsRequest request)
    {
        var url = CreateUrlWithParams($"v1/Transactions", request, true);
        var response = await GetAsync(url);
        var transactions = await response.Content.ReadFromJsonAsync<PaginatedList<MoneyTransferTransactionsDto>>();
        if (!CanSeeSensitiveData())
        {
            transactions.Items.ForEach(s =>
            {
                s.ReceiverIbanNumber = SensitiveDataHelper.MaskSensitiveData("Iban", s.ReceiverIbanNumber);
                s.ReceiverName = SensitiveDataHelper.MaskSensitiveData("FullName", s.ReceiverName);
            });
        }
        return transactions;

    }

    public async Task ManualPaymentAsync(Guid id)
    {
        await PutAsJsonAsync($"v1/Transactions/manual-payment/{id}", string.Empty);
    }

    public async Task CancelAsync(Guid id)
    {
        await PutAsJsonAsync($"v1/Transactions/cancel/{id}", string.Empty);
    }
}
