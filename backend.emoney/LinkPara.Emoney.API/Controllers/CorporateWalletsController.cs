using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.AddUser;
using MediatR;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.UpdateUser;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.DeleteUser;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.ActivateUser;
using LinkPara.SharedModels.Pagination;
using LinkPara.Emoney.Application.Features.CorporateWallets;
using LinkPara.Emoney.Application.Features.CorporateWallets.Queries.GetAllUsers;
using LinkPara.Emoney.Application.Features.CorporateWallets.Queries.GetUserById;
using LinkPara.Emoney.Application.Features.CorporateWallets.Queries.GetCorporateAccountList;
using LinkPara.Emoney.Application.Features.CorporateWallets.Queries.GetCorporateAccountById;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.UpdateCorporateAccount;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.DeactivateCorporateAccount;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.ActivateCorporateAccount;

namespace LinkPara.Emoney.API.Controllers;

public class CorporateWalletsController : ApiControllerBase
{   
    /// <summary>
    /// add account user
    /// </summary>
    /// <returns></returns>
    [HttpPost("users")]
    [Authorize(Policy = "CorporateWalletUser:Create")]
    public async Task<Unit> AddUserAsync([FromBody] AddCorporateWalletUserCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// update account user
    /// </summary>
    /// <returns></returns>
    [HttpPut("users")]
    [Authorize(Policy = "CorporateWalletUser:Update")]
    public async Task<Unit> UpdateUserAsync([FromBody] UpdateCorporateWalletUserCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// update account user
    /// </summary>
    /// <returns></returns>
    [HttpPut("users/deactivate")]
    [Authorize(Policy = "CorporateWalletUser:Delete")]
    public async Task<Unit> DeactivateUserAsync([FromBody] DeactivateCorporateWalletUserCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// update account user
    /// </summary>
    /// <returns></returns>
    [HttpPut("users/activate")]
    [Authorize(Policy = "CorporateWalletUser:Update")]
    public async Task<Unit> ActivateUserAsync([FromBody] ActivateCorporateWalletUserCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// get account users
    /// </summary>
    /// <returns></returns>
    [HttpGet("users")]
    [Authorize(Policy = "CorporateWalletUser:ReadAll")]
    public async Task<PaginatedList<CorporateWalletUserDto>> GetUsersAsync([FromQuery] GetCorporateWalletUsersQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// get account user
    /// </summary>
    /// <returns></returns>
    [HttpGet("users/{id}")]
    [Authorize(Policy = "CorporateWalletUser:Read")]
    public async Task<CorporateWalletUserDto> GetUserByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetCorporateWalletUserByIdQuery { Id = id});
    }

    /// <summary>
    /// get Corporate accounts
    /// </summary>
    /// <returns></returns>
    [HttpGet("accounts")]
    [Authorize(Policy = "CorporateWalletAccount:ReadAll")]
    public async Task<PaginatedList<CorporateAccountDto>> GetAccountsAsync([FromQuery] GetCorporateAccountListQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// get Corporate account
    /// </summary>
    /// <returns></returns>
    [HttpGet("accounts/{id}")]
    [Authorize(Policy = "CorporateWalletAccount:Read")]
    public async Task<CorporateAccountDto> GetAccountByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetCorporateAccountByIdQuery { Id = id});
    }

    /// <summary>
    /// update Corporate account
    /// </summary>
    /// <returns></returns>
    [HttpPut("accounts")]
    [Authorize(Policy = "CorporateWalletAccount:Update")]
    public async Task<Unit> UpdateAccountAsync([FromBody] UpdateCorporateAccountCommand command )
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Deactivate Corporate Account
    /// </summary>
    /// <returns></returns>
    [HttpPut("accounts/deactivate/{id}")]
    [Authorize(Policy = "CorporateWalletAccount:Delete")]
    public async Task<Unit> DeactivateAccountAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new DeactivateCorporateAccountCommand { Id = id});
    }

    /// <summary>
    /// Activate Corporate Account
    /// </summary>
    /// <returns></returns>
    [HttpPut("accounts/activate/{id}")]
    [Authorize(Policy = "CorporateWalletAccount:Update")]
    public async Task<Unit> ActivateAccountAsync([FromRoute]Guid id)
    {
        return await Mediator.Send(new ActivateCorporateAccountCommand { Id = id });
    }
}
