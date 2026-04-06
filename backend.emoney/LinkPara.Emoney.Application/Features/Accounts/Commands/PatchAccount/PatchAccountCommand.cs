using LinkPara.Emoney.Application.Commons.Attributes;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.Emoney.Application.Features.Accounts.Commands.PatchAccount;

public class PatchAccountCommand : IRequest<AccountDto>
{
    [Audit]
    public Guid AccountId { get; set; }
    public JsonPatchDocument<PatchAccountDto> PatchAccountDto { get; set; }
}

public class PatchAccountCommandHandler : IRequestHandler<PatchAccountCommand, AccountDto>
{
    private readonly IAccountService _accountService;

    public PatchAccountCommandHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<AccountDto> Handle(PatchAccountCommand command, CancellationToken cancellationToken)
    {
        return await _accountService.PatchAccountAsync(command);
    }
}

