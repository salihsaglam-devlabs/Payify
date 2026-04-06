using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients
{
    public class ReturnedTransactionHttpClient : HttpClientBase, IReturnedTransactionHttpClient
    {
        public ReturnedTransactionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
            : base(client, httpContextAccessor)
        {
        }

        public async Task CancelReturnedTransactionAsync(Guid id)
        {
            var response = await PutAsJsonAsync($"v1/ReturnedTransactions/{id}", "");
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }

        public async Task<ReturnedTransactionDto> GetReturnedTransactionByIdAsync(Guid id)
        {
            var response = await GetAsync($"v1/ReturnedTransactions/{id}");
            var returnedTransaction = await response.Content.ReadFromJsonAsync<ReturnedTransactionDto>();
            if (!CanSeeSensitiveData())
            {
                returnedTransaction.IbanNumber = SensitiveDataHelper.MaskSensitiveData("Iban", returnedTransaction.IbanNumber);
                returnedTransaction.NameSurname = SensitiveDataHelper.MaskSensitiveData("FullName", returnedTransaction.NameSurname);
            }
            return returnedTransaction ?? throw new InvalidOperationException();
        }
        public async Task<PaginatedList<ReturnedTransactionDto>> GetReturnedTransactionsAsync(GetReturnedTransactionsRequest request)
        {
            var url = CreateUrlWithParams($"v1/ReturnedTransactions", request, true);
            var response = await GetAsync(url);
            var returnedTransactions = await response.Content.ReadFromJsonAsync<PaginatedList<ReturnedTransactionDto>>();
            if (!CanSeeSensitiveData())
            {
                returnedTransactions.Items.ForEach(s =>
                {
                    s.IbanNumber = SensitiveDataHelper.MaskSensitiveData("Iban", s.IbanNumber);
                    s.NameSurname = SensitiveDataHelper.MaskSensitiveData("FullName", s.NameSurname);
                });
            }
            return returnedTransactions;
        }
    }
}
