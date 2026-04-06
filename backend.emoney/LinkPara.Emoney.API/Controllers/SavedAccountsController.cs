using LinkPara.Emoney.Application.Features.SavedAccounts;
using LinkPara.Emoney.Application.Features.SavedAccounts.Commands.DeleteSavedAccount;
using LinkPara.Emoney.Application.Features.SavedAccounts.Commands.SaveBankAccount;
using LinkPara.Emoney.Application.Features.SavedAccounts.Commands.SaveWalletAccount;
using LinkPara.Emoney.Application.Features.SavedAccounts.Commands.UpdateBankAccount;
using LinkPara.Emoney.Application.Features.SavedAccounts.Commands.UpdateWalletAccount;
using LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedBankAccountById;
using LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedBankAccounts;
using LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedWalletAccountById;
using LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedWalletAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class SavedAccountsController : ApiControllerBase
{

    /// <summary>
    /// Returns bank accounts of user.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneySavedAccount:ReadAll")]
    [HttpGet("bank-accounts")]
    public async Task<List<SavedBankAccountDto>> GetBankAccountsAsync([FromQuery] GetSavedBankAccountsQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns wallets of user.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneySavedAccount:ReadAll")]
    [HttpGet("wallets")]
    public async Task<List<SavedWalletAccountDto>> GetWalletAccountsAsync([FromQuery] GetSavedWalletAccountsQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns wallet by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneySavedAccount:Read")]
    [HttpGet("wallets/{id}")]
    public async Task<SavedWalletAccountDto> GetWalletAccountsAsync(Guid id)
    {
        return await Mediator.Send(new GetSavedWalletAccountByIdQuery { Id = id });
    }

    /// <summary>
    /// Saves bank account for user.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneySavedAccount:Create")]
    [HttpPost("bank-accounts")]
    public async Task SaveBankAccountAsync(SaveBankAccountCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Saves wallet for user.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneySavedAccount:Create")]
    [HttpPost("wallets")]
    public async Task SaveWalletAccountAsync(SaveWalletAccountCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Update wallet account for user.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneySavedAccount:Update")]
    [HttpPut("wallets/{Id}")]
    public async Task UpdateWalletAccounAsync(Guid Id, UpdateWalletAccountCommand command)
    {
        command.Id = Id;
        await Mediator.Send(command);
    }

    /// <summary>
    /// Update bank account for user.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneySavedAccount:Update")]
    [HttpPut("bank-accounts/{Id}")]
    public async Task UpdateBankAccounAsync(Guid Id, UpdateBankAccountCommand command)
    {
        command.Id = Id;
        await Mediator.Send(command);
    }

    /// <summary>
    /// Deletes account for user.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneySavedAccount:Delete")]
    [HttpDelete("{Id}")]
    public async Task DeleteBankAccountAsync(Guid Id, [FromQuery] DeleteSavedAccountCommand command)
    {
        command.Id = Id;
        await Mediator.Send(command);
    }

    /// <summary>
    /// Returns bank account by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneySavedAccount:Read")]
    [HttpGet("bank-accounts/{id}")]
    public async Task<SavedBankAccountDto> GetBankAccountFromIdAsync(Guid id)
    {
        return await Mediator.Send(new GetSavedBankAccountByIdQuery { Id = id});
    }

}