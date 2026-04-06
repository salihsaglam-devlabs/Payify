using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.CheckOrUpgradeAccountTier;

public class CheckOrUpgradeAccountTierCommand : IRequest
{
    public Guid AccountId { get; set; }
    public AccountTierValidation ValidationType { get; set; }
}

public class CheckOrUpgradeAccountTierCommandHandler : IRequestHandler<CheckOrUpgradeAccountTierCommand>
{
    private readonly ITierLevelService _tierLevelService;
    private readonly IGenericRepository<Account> _accountRepository;

    public CheckOrUpgradeAccountTierCommandHandler(
        ITierLevelService tierLevelService,
        IGenericRepository<Account> accountRepository)
    {
        _tierLevelService = tierLevelService;
        _accountRepository = accountRepository;
    }

    public async Task<Unit> Handle(CheckOrUpgradeAccountTierCommand command, CancellationToken cancellationToken)
    {
        var account = await _accountRepository
            .GetAll()
            .FirstOrDefaultAsync(a => a.Id == command.AccountId, cancellationToken: cancellationToken);
        
        await _tierLevelService.CheckOrUpgradeAccountTierAsync(account, command.ValidationType);
        
        return Unit.Value;
    }
}