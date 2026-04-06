using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public interface ISavedAccountHttpClient
{
    Task<List<SavedBankAccountDto>> GetBankAccountsAsync(string userId);
    Task SaveBankAccountAsync(SaveBankAccountRequest request);
    Task UpdateBankAccountAsync(Guid id, UpdateBankAccountRequest request);
    Task UpdateWalletAccountAsync(Guid id, UpdateWalletAccountRequest request);
    Task DeleteSavedAccountAsync(Guid id, string userId);
    Task SaveWalletAccountAsync(SaveWalletAccountRequest request);
    Task<List<SavedWalletAccountDto>> GetWalletAccountsAsync(string userId);
    Task<SavedBankAccountDto> GetBankAccountByIdAsync(Guid id);
    Task<SavedWalletAccountDetailDto> GetWalletAccountByIdAsync(Guid id);
}
