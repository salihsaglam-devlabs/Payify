using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.HttpProviders.Emoney.Enums;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountCommand : IRequest
{
    public Guid? AccountId { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public AccountType AccountType { get; set; }
    public AccountKycLevel AccountKycLevel { get; set; }
    public Guid IdentityUserId { get; set; }
    public string IdentityNumber { get; set; }
    public Guid ParentAccountId { get; set; }
    public string CorporateTitle { get; set; }
    public string Profession { get; set; }
}

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand>
{
    private readonly IAccountService _accountService;

    public CreateAccountCommandHandler(
        IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<Unit> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        await _accountService.CreateAccountAsync(request);

        return Unit.Value;
    }
}
