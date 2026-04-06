using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public interface IEmoneyAccountHttpClient
{
    Task<AccountDto> GetAccountByUserIdAsync(Guid userId);
    Task<AccountDto> GetAccountByWalletNumberAsync(string walletNumber);
    Task CreateAccountAsync(CreateEmoneyAccountRequest request);
    Task CreateAccountUserAsync(CreateEmoneyAccountUserRequest request);
    Task PatchAccountAsync(Guid accountId, JsonPatchDocument<UpdateAccountRequest> request);
    Task ValidateAccountUserIdentityAsync(ValidateIdentityRequest request, string userId);
    Task<PaginatedList<CustodyAccountResponse>> GetCustodyAccountListAsync(GetCustodyAccountListRequest request);
}
