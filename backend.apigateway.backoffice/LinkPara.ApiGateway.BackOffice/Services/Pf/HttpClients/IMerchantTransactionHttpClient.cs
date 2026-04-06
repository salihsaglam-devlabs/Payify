using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IMerchantTransactionHttpClient
{
    Task<PaginatedList<MerchantTransactionDto>> GetAllAsync(GetAllMerchantTransactionRequest request);
    Task<PaginatedList<MerchantInstallmentTransactionDto>> GetAllInstallmentTransactionAsync(GetAllMerchantInstallmentTransactionRequest request);
    Task<MerchantTransactionDto> GetByIdAsync(Guid id);
    Task<UpdateMerchantTransactionRequest> PatchAsync(Guid id, JsonPatchDocument<UpdateMerchantTransactionRequest> merchantTransactionPatch);
    Task<List<MerchantTransactionStatusModel>> GetStatusCountAsync(MerchantTransactionStatusRequest request);
    Task<string> GenerateOrderNumberAsync(Guid merchantId);
    Task ManualReturnAsync(ManualReturnRequest request);
}
