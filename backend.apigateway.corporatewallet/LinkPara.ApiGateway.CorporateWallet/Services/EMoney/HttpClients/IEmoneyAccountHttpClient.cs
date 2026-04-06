using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public interface IEmoneyAccountHttpClient
{
    Task<AccountDto> GetAccountByUserIdAsync(Guid userId);
    Task CreateAccountAsync(CreateEmoneyAccountRequest request);
    Task CreateAccountUserAsync(CreateEmoneyAccountUserRequest request);
    Task PatchAccountAsync(Guid accountId, JsonPatchDocument<UpdateAccountRequest> request);
    Task ValidateAccountUserIdentityAsync(ValidateIdentityRequest request, string userId);
}
