using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;

public interface IEmoneyAccountHttpClient
{
    Task CreateAccountAsync(CreateEmoneyAccountRequest request);
    Task<AccountDto> GetAccountByUserIdAsync(Guid userId);
    Task PatchAccountAsync(Guid accountId, JsonPatchDocument<UpdateAccountRequest> request);
}
