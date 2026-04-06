using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class IncomingTransactionHttpClient : HttpClientBase, IIncomingTransactionHttpClient
{
    public IncomingTransactionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    public async Task<IncomingTransactionDto> GetIncomingTransactionByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/IncomingTransactions/{id}");
        var incomingTransaction = await response.Content.ReadFromJsonAsync<IncomingTransactionDto>();
        if (!CanSeeSensitiveData())
        {
            incomingTransaction.SenderIbanNumber = SensitiveDataHelper.MaskSensitiveData("Iban", incomingTransaction.SenderIbanNumber);
            incomingTransaction.SenderName = SensitiveDataHelper.MaskSensitiveData("FullName", incomingTransaction.SenderName);
            incomingTransaction.ReceiverName = SensitiveDataHelper.MaskSensitiveData("FullName", incomingTransaction.ReceiverName);
            incomingTransaction.SenderTaxNumber = SensitiveDataHelper.MaskSensitiveData("TaxNumber", incomingTransaction.SenderTaxNumber);
        }
        return incomingTransaction ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<IncomingTransactionDto>> GetIncomingTransactionsAsync(GetIncomingTransactionsRequest request)
    {
        var url = CreateUrlWithParams($"v1/IncomingTransactions", request, true);
        var response = await GetAsync(url);
        var incomingTransactions = await response.Content.ReadFromJsonAsync<PaginatedList<IncomingTransactionDto>>();
        if (!CanSeeSensitiveData())
        {
            incomingTransactions.Items.ForEach(s =>
            {
                s.SenderIbanNumber = SensitiveDataHelper.MaskSensitiveData("Iban", s.SenderIbanNumber);
                s.SenderName = SensitiveDataHelper.MaskSensitiveData("FullName", s.SenderName);
                s.ReceiverName = SensitiveDataHelper.MaskSensitiveData("FullName", s.ReceiverName);
                s.SenderTaxNumber = SensitiveDataHelper.MaskSensitiveData("TaxNumber", s.SenderTaxNumber);
            });
        }
        return incomingTransactions;
    }

    public async Task ManualPaymentAsync(Guid id)
    {
        await PutAsJsonAsync($"v1/IncomingTransactions/{id}", string.Empty);
    }
}
