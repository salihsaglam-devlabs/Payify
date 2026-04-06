using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.CreateAccountCustomTierLevel;

public class CreateAccountCustomTierLevelCommand : IRequest
{
    public AccountCustomTierDto AccountCustomTierDto { get; set; }
}

public class CreateAccountCustomTierLevelCommandHandler : IRequestHandler<CreateAccountCustomTierLevelCommand>
{
    private readonly IGenericRepository<AccountCustomTier> _repository;
    private readonly IGenericRepository<TierLevel> _tierLevelRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IAuditLogService _auditLogService;

    public CreateAccountCustomTierLevelCommandHandler(IGenericRepository<AccountCustomTier> repository,
        IGenericRepository<TierLevel> tierLevelRepository,
        IContextProvider contextProvider,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _tierLevelRepository = tierLevelRepository;
        _contextProvider = contextProvider;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(CreateAccountCustomTierLevelCommand command, CancellationToken cancellationToken)
    {
        var tierLevel = await _tierLevelRepository.GetByIdAsync(command.AccountCustomTierDto.TierLevelId);

        if (tierLevel is null)
        {
            throw new NotFoundException(nameof(TierLevel), command.AccountCustomTierDto.TierLevelId);
        }

        if (await AccountHasCustomTierAsync(command.AccountCustomTierDto))
            throw new DuplicateRecordException();

        var customAccountTierLevel = new AccountCustomTier
        {
            AccountId = command.AccountCustomTierDto.AccountId,
            AccountName = command.AccountCustomTierDto.AccountName,
            CreateDate = DateTime.Now,
            Email = command.AccountCustomTierDto.Email,
            PhoneCode = command.AccountCustomTierDto.PhoneCode,
            PhoneNumber = command.AccountCustomTierDto.PhoneNumber,
            TierLevelId = command.AccountCustomTierDto.TierLevelId,
            CreatedBy = _contextProvider.CurrentContext.UserId,
            RecordStatus = RecordStatus.Active            
        };

        await _repository.AddAsync(customAccountTierLevel);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "CreateAccountCustomTierLevel",
            SourceApplication = "Emoney",
            Resource = "AccountCustomTier",
            Details = new Dictionary<string, string>
            {
                {"Id", customAccountTierLevel.Id.ToString() },
                {"TierLevelId", command.AccountCustomTierDto.TierLevelId.ToString() },
                {"AccountId", command.AccountCustomTierDto.AccountId.ToString() }
            }
        });

        return Unit.Value;
    }

    private async Task<bool> AccountHasCustomTierAsync(AccountCustomTierDto request)
    {
        return await _repository.GetAll().AnyAsync(x =>
            x.AccountId == request.AccountId &&
            x.RecordStatus == RecordStatus.Active
        );
    }
}