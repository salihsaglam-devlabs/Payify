using LinkPara.Emoney.Application.Commons.Models.AccountModels;
using LinkPara.Emoney.Application.Features.Accounts;
using LinkPara.Emoney.Application.Features.Accounts.Commands.CreateAccount;
using LinkPara.Emoney.Application.Features.Accounts.Commands.PatchAccount;
using LinkPara.Emoney.Application.Features.Accounts.Commands.PatchAccountUser;
using LinkPara.Emoney.Application.Features.Accounts.Commands.ValidateIdentity;
using LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountByIdQuery;
using LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountDetailQuery;
using LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountKycChangesById;
using LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountListQuery;
using LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountUserListQuery;
using LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountWalletListQuery;
using LinkPara.Emoney.Application.Features.Accounts.Queries.GetCustodyAccountListQuery;
using LinkPara.Emoney.Application.Features.Accounts.Queries.GetParentAccountByUserId;
using LinkPara.Emoney.Application.Features.AccountUsers.Commands;
using LinkPara.Emoney.Application.Features.AccountUsers.Queries.GetAllAccountUserQuery;
using LinkPara.Emoney.Application.Features.Wallets;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class AccountsController : ApiControllerBase
{
    /// <summary>
    /// Returns an account
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Read")]
    [HttpGet("{id}")]
    public async Task<AccountDto> GetAccountByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetAccountByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns an account by identityuser or walletnumber
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Read")]
    [HttpGet("detail")]
    public async Task<AccountDto> GetAccountDetailAsync([FromQuery] GetAccountDetailQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns the all accounts.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<AccountDto>> GetAccountListAsync([FromQuery] GetAccountListQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a new account
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("")]
    public async Task CreateAccountAsync(CreateAccountCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates an account
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="patchAccountDto"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Update")]
    [HttpPatch("{accountId}")]
    public async Task<AccountDto> PatchAsync(Guid accountId, [FromBody] JsonPatchDocument<PatchAccountDto> patchAccountDto)
    {
        return await Mediator.Send(new PatchAccountCommand
        {
            AccountId = accountId,
            PatchAccountDto = patchAccountDto
        });
    }

    /// <summary>
    /// Creates a new account user
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Create")]
    [HttpPost("{accountId}/users/")]
    public async Task CreateAccountUserAsync(CreateAccountUserCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Returns an account users
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:ReadAll")]
    [HttpGet("{accountId}/users/")]
    public async Task<List<AccountUserDto>> GetAccountUserListAsync([FromRoute] Guid accountId)
    {
        return await Mediator.Send(new GetAccountUserListQuery { AccountId = accountId });
    }

    /// <summary>
    /// Returns all account user
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:ReadAll")]
    [HttpGet("accountUsers")]
    public async Task<PaginatedList<AccountUserDto>> GetAllAccountUserAsync([FromQuery] GetAllAccountUserQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns an account wallets
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:ReadAll")]
    [HttpGet("{accountId}/wallets/")]
    public async Task<List<WalletDto>> GetAccountWalletListAsync([FromRoute] Guid accountId)
    {
        return await Mediator.Send(new GetAccountWalletListQuery { AccountId = accountId });
    }

    /// <summary>
    /// Updates an account user
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="accountUserId"></param>
    /// <param name="patchAccountUserDto"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Update")]
    [HttpPatch("{accountId}/users/{accountUserId}")]
    public async Task PatchAccountUserAsync(Guid accountId, Guid accountUserId, [FromBody] JsonPatchDocument<PatchAccountUserDto> patchAccountUserDto)
    {
        await Mediator.Send(new PatchAccountUserCommand { AccountId = accountId, AccountUserId = accountUserId, PatchAccountUserDto = patchAccountUserDto });
    }
    
    /// <summary>
    /// Validates user identity
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Update")]
    [HttpPost("validate-identity")]
    public async Task ValidateAccountUserIdentityAsync(ValidateIdentityCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Returns account kyc changes
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Read")]
    [HttpGet("{id}/kyc-changes")]
    public async Task<List<AccountKycChangeDto>> GetAccountKycChangesByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetAccountKycChangesByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns CustodyAccounts according to filter
    /// </summary>
    /// <param name="query">Filter</param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Read")]
    [HttpGet("getCustodyAccountList")]
    public async Task<PaginatedList<CustodyAccountResponse>> GetCustodyAccountListAsync([FromQuery] GetCustodyAccountListQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Deactivates Account
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Update")]
    [HttpPost("deactivate-account")]
    public async Task DeactivateAccountAsync(DeactivateAccountCommand command)
    {
        await Mediator.Send(command);
    }


    /// <summary>
    /// Returns parent account of a custudy account if exist
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyAccount:Read")]
    [HttpGet("{userId}/getParentAccount")]
    public async Task<ParentAccountResponse> GetParentAccountByUserIdAsync([FromRoute] Guid userId)
    {
        return await Mediator.Send(new GetParentAccountByUserIdQuery{ UserId = userId });
    }
}
