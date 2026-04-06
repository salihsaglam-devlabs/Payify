using DocumentFormat.OpenXml.Office2010.Excel;
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class MerchantTransactionHttpClient : HttpClientBase, IMerchantTransactionHttpClient
{
    public MerchantTransactionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<MerchantTransactionDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/MerchantTransactions/{id}");
        var transaction = await response.Content.ReadFromJsonAsync<MerchantTransactionDto>();

        if (!CanSeeSensitiveData())
        {
            transaction.CardNumber = SensitiveDataHelper.MaskSensitiveData("CardNumber", transaction.CardNumber);
            transaction.MerchantCustomerName = SensitiveDataHelper.MaskSensitiveData("Name", transaction.MerchantCustomerName);
            transaction.MerchantCustomerPhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", transaction.MerchantCustomerPhoneNumber);
            transaction.CardHolderName = SensitiveDataHelper.MaskSensitiveData("FullName", transaction.CardHolderName);          
        }

        return transaction ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<MerchantTransactionDto>> GetAllAsync(GetAllMerchantTransactionRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantTransactions", request, true);
        var response = await GetAsync(url);
        var transactions = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantTransactionDto>>();

        if (!CanSeeSensitiveData())
        {
            transactions.Items.ForEach(s =>
            {
                s.CardNumber = SensitiveDataHelper.MaskSensitiveData("CardNumber", s.CardNumber);
                s.MerchantCustomerName = SensitiveDataHelper.MaskSensitiveData("Name", s.MerchantCustomerName);
                s.MerchantCustomerPhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", s.MerchantCustomerPhoneNumber);
                s.CardHolderName = SensitiveDataHelper.MaskSensitiveData("FullName", s.CardHolderName);
            });
        }

        return transactions ?? throw new InvalidOperationException();
    }

    public async Task<UpdateMerchantTransactionRequest> PatchAsync(Guid id, JsonPatchDocument<UpdateMerchantTransactionRequest> merchantTransactionPatch)
    {
        merchantTransactionPatch.Operations.RemoveAll(x => x.path.Contains("files"));

        var response = await PatchAsync($"v1/MerchantTransactions/{id}", merchantTransactionPatch);
        var merchantTransaction = await response.Content.ReadFromJsonAsync<UpdateMerchantTransactionRequest>();
        return merchantTransaction ?? throw new InvalidOperationException();
    }

    public async Task<List<MerchantTransactionStatusModel>> GetStatusCountAsync(MerchantTransactionStatusRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantTransactions/statusCount", request, true);
        var response = await GetAsync(url);
        var transactions = await response.Content.ReadFromJsonAsync<List<MerchantTransactionStatusModel>>();
        return transactions ?? throw new InvalidOperationException();
    }
    public async Task<string> GenerateOrderNumberAsync(Guid merchantId)
    {
        var response = await GetAsync($"v1/MerchantTransactions/{merchantId}/generate-orderNumber");
        var transaction = await response.Content.ReadAsStringAsync();
        return transaction ?? throw new InvalidOperationException();
    }

    public async Task ManualReturnAsync(ManualReturnRequest request)
    {
        await PostAsJsonAsync("v1/MerchantTransactions/manual-return", request);
    }

    public async Task<PaginatedList<MerchantInstallmentTransactionDto>> GetAllInstallmentTransactionAsync(GetAllMerchantInstallmentTransactionRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantTransactions/merchantInstallmentTransactions", request, true);
        var response = await GetAsync(url);
        var transactions = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantInstallmentTransactionDto>>();

        if (!CanSeeSensitiveData())
        {
            transactions.Items.ForEach(s =>
            {
                s.CardNumber = SensitiveDataHelper.MaskSensitiveData("CardNumber", s.CardNumber);
                s.MerchantCustomerName = SensitiveDataHelper.MaskSensitiveData("Name", s.MerchantCustomerName);
                s.MerchantCustomerPhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", s.MerchantCustomerPhoneNumber);
                s.CardHolderName = SensitiveDataHelper.MaskSensitiveData("FullName", s.CardHolderName);
            });
        }

        return transactions ?? throw new InvalidOperationException();
    }
}
