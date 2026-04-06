using LinkPara.Emoney.Application.Commons.Attributes;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.DeleteAccountCustomTier;

public class DeleteAccountCustomTierCommand : IRequest
{
    [Audit]
    public Guid Id { get; set; }
}

public class DeleteAccountCustomTierCommandHandler : IRequestHandler<DeleteAccountCustomTierCommand>
{
    private readonly IGenericRepository<AccountCustomTier> _accountCustomTierRepository;

    public DeleteAccountCustomTierCommandHandler(
        IGenericRepository<AccountCustomTier> accountCustomTierRepository)
    {
        _accountCustomTierRepository = accountCustomTierRepository;
    }

    public async Task<Unit> Handle(DeleteAccountCustomTierCommand request, CancellationToken cancellationToken)
    {
        var accountCustomTier = await _accountCustomTierRepository.GetByIdAsync(request.Id);

        if (accountCustomTier is null)
        {
            throw new NotFoundException(nameof(AccountCustomTier), request.Id);
        }

        accountCustomTier.RecordStatus = RecordStatus.Passive;
        await _accountCustomTierRepository.UpdateAsync(accountCustomTier);

        return Unit.Value;
    }
}