using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.DisableCustomTierLevel;

public class DisableCustomTierLevelCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DisableCustomTierLevelCommandHandler : IRequestHandler<DisableCustomTierLevelCommand>
{
    private readonly IGenericRepository<TierLevel> _tierRepository;
    private readonly IGenericRepository<AccountCustomTier> _accountTierRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public DisableCustomTierLevelCommandHandler(IGenericRepository<TierLevel> tierRepository,
        IGenericRepository<AccountCustomTier> accountTierRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider)
    {
        _tierRepository = tierRepository;
        _accountTierRepository = accountTierRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(DisableCustomTierLevelCommand command, CancellationToken cancellationToken)
    {
        var tierLevel = await _tierRepository.GetByIdAsync(command.Id);
        if (tierLevel is null)
        {
            throw new NotFoundException(nameof(TierLevel), command.Id);
        }

        if (tierLevel.TierLevelType is not TierLevelType.Custom)
        {
            throw new InvalidTierLevelException();
        }

        _ = Guid.TryParse(_contextProvider.CurrentContext.UserId, out var userId);

        var anyActiveUsingByAccounts = await _accountTierRepository.GetAll()
            .Where(q => q.TierLevelId == tierLevel.Id)
            .ToListAsync(cancellationToken);

        if (anyActiveUsingByAccounts.Count > 0)
        {
            anyActiveUsingByAccounts.ForEach(s =>
            {
                s.RecordStatus = RecordStatus.Passive;
                s.LastModifiedBy = userId.ToString();
                s.UpdateDate = DateTime.Now;
            });

            await _accountTierRepository.UpdateRangeAsync(anyActiveUsingByAccounts);
        }

        tierLevel.RecordStatus = RecordStatus.Passive;
        tierLevel.UpdateDate = DateTime.Now;

        await _tierRepository.UpdateAsync(tierLevel);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "DeleteCustomTierLevel",
                SourceApplication = "Emoney",
                Resource = "TierLevel",
                Details = new Dictionary<string, string>
                {
                       {"Id", tierLevel.Id.ToString() },
                       {"Name", tierLevel.Name }
                }
            });

        return Unit.Value;
    }

}