using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Emoney;
public class SavedAccountsController : ApiControllerBase
{
    private readonly ISavedAccountHttpClient _savedAccountHttpClient;


    public SavedAccountsController(ISavedAccountHttpClient savedAccountHttpClient)
    {
        _savedAccountHttpClient = savedAccountHttpClient;
    }

    /// <summary>
    /// Returns bank accounts of user
    /// </summary>
    [Authorize(Policy = "EmoneySavedAccount:ReadAll")]
    [HttpGet("bank-accounts")]
    public async Task<List<SavedBankAccountDto>> GetBankAccountsAsync()
    {
        return await _savedAccountHttpClient.GetBankAccountsAsync(UserId);
    }

    /// <summary>
    /// Returns bank account by id
    /// </summary>
    /// <param name="id"></param>
    [Authorize(Policy = "EmoneySavedAccount:Read")]
    [HttpGet("bank-accounts/{id}")]
    public async Task<SavedBankAccountDto> GetBankAccountByIdAsync(Guid id)
    {
        return await _savedAccountHttpClient.GetBankAccountByIdAsync(id);
    }


    /// <summary>
    /// Get Wallet Account By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneySavedAccount:Read")]
    [HttpGet("wallets/{id}")]
    public async Task<SavedWalletAccountDetailDto> GetWalletAccountByIdAsync(Guid id)
    {
        return await _savedAccountHttpClient.GetWalletAccountByIdAsync(id);
    }

    /// <summary>
    /// Returns wallets of user
    /// </summary>
    [Authorize(Policy = "EmoneySavedAccount:ReadAll")]
    [HttpGet("wallets")]
    public async Task<List<SavedWalletAccountDto>> GetWalletAccountsAsync()
    {
        return await _savedAccountHttpClient.GetWalletAccountsAsync(UserId);
    }

    /// <summary>
    /// Saves bank account for user
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneySavedAccount:Create")]
    [HttpPost("bank-accounts")]
    public async Task SaveBankAccountAsync(SaveBankAccountRequest request)
    {
        await _savedAccountHttpClient.SaveBankAccountAsync(request);
    }

    /// <summary>
    /// Saves the wallet for user
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneySavedAccount:Create")]
    [HttpPost("wallets")]
    public async Task SaveWalletAccount(SaveWalletAccountRequest request)
    {
        await _savedAccountHttpClient.SaveWalletAccountAsync(request);
    }

    /// <summary>
    /// Update bank account for user
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneySavedAccount:Update")]
    [HttpPut("bank-accounts/{id}")]
    public async Task UpdateBankAccountAsync(Guid id, UpdateBankAccountRequest request)
    {
        await _savedAccountHttpClient.UpdateBankAccountAsync(id, request);
    }

    /// <summary>
    /// Update wallet account for user
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneySavedAccount:Update")]
    [HttpPut("wallets/{id}")]
    public async Task UpdateWalletAccountAsync(Guid id, UpdateWalletAccountRequest request)
    {
        await _savedAccountHttpClient.UpdateWalletAccountAsync(id, request);
    }

    /// <summary>
    /// Deletes the account for user
    /// </summary>
    /// <param name="id"></param>
    [Authorize(Policy = "EmoneySavedAccount:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteBankAccountAsync(Guid id)
    {
        await _savedAccountHttpClient.DeleteSavedAccountAsync(id, UserId);
    }
}
