using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Accounts.Commands.PatchAccount;
using LinkPara.HttpProviders.Emoney.Enums;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.Emoney.Application.Features.Accounts.Commands.ValidateIdentity;

public class DeactivateAccountCommand : IRequest
{
    public Guid AccountId { get; set; }
    public string ChangeReason { get; set; }
}

public class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand>
{
    private readonly IAccountService _accountService;

    public DeactivateAccountCommandHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<Unit> Handle(DeactivateAccountCommand command, CancellationToken cancellationToken)
    {
        var request = new PatchAccountCommand();
        request.AccountId = command.AccountId;
        request.PatchAccountDto = new JsonPatchDocument<PatchAccountDto>();
        request.PatchAccountDto.Replace(x => x.ChangeReason, command.ChangeReason);
        request.PatchAccountDto.Replace(x => x.AccountStatus, AccountStatus.Inactive);
        await _accountService.PatchAccountAsync(request);

        return Unit.Value;
    }
}