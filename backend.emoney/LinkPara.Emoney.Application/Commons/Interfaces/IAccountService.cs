using LinkPara.Emoney.Application.Features.Accounts;
using LinkPara.Emoney.Application.Features.Accounts.Commands.CreateAccount;
using LinkPara.Emoney.Application.Features.Accounts.Commands.PatchAccount;
using LinkPara.Emoney.Application.Features.Accounts.Commands.PatchAccountUser;
using LinkPara.Emoney.Domain.Entities;
using MediatR;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IAccountService
{
    public Task CreateAccountAsync(CreateAccountCommand request);
    public Task<AccountDto> PatchAccountAsync(PatchAccountCommand command);
    public Task PatchAccountUserAsync(PatchAccountUserCommand request);
    public Task TransformCustodyAccountsAsync();
    public Task<AccountUser> GetCorporateAccountUserAsync(Guid userId);
    public Task DeclareAccountsAsync();
}
