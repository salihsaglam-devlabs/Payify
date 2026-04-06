using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.CreateCustomTierLevel;

public class CreateCustomTierLevelCommand : IRequest
{
    public CustomTierLevelDto TierLevelDto { get; set; }
}

public class CreateCustomTierLevelCommandHandler : IRequestHandler<CreateCustomTierLevelCommand>
{
    private readonly IGenericRepository<TierLevel> _repository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public CreateCustomTierLevelCommandHandler(IGenericRepository<TierLevel> repository,
        IMapper mapper,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _mapper = mapper;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(CreateCustomTierLevelCommand command, CancellationToken cancellationToken)
    {
        var customTierLevel = _mapper.Map<CustomTierLevelDto, TierLevel>(command.TierLevelDto);

        customTierLevel.TierLevelType = TierLevelType.Custom;

        if (await IsExistAsync(customTierLevel))
            throw new DuplicateRecordException();

        await _repository.AddAsync(customTierLevel);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "CreateCustomTierLevel",
            SourceApplication = "Emoney",
            Resource = "TierLevel",
            Details = new Dictionary<string, string>
            {
                   {"Id", customTierLevel.Id.ToString() },
                   {"Name", command.TierLevelDto.Name }
            }
        });

        return Unit.Value;
    }

    private async Task<bool> IsExistAsync(TierLevel customTierLevel)
    {
        return await _repository.GetAll().AnyAsync(x =>
            x.Name == customTierLevel.Name &&
            x.CurrencyCode == customTierLevel.CurrencyCode &&
            x.RecordStatus == RecordStatus.Active
        );
    }
}