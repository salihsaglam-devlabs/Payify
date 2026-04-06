using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.PutTierPermission;

public class PutTierPermissionCommand : IRequest
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }
}

public class PutTierPermissionCommandHandler : IRequestHandler<PutTierPermissionCommand>
{
    private readonly IGenericRepository<TierPermission> _repository;
    private readonly IAuditLogService _auditLogService;

    public PutTierPermissionCommandHandler(IGenericRepository<TierPermission> repository,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService; 
    }

    public async Task<Unit> Handle(PutTierPermissionCommand command, CancellationToken cancellationToken)
    {
        var dbTierPermission = await _repository.GetByIdAsync(command.Id);

        if (dbTierPermission is null)
        {
            throw new NotFoundException(nameof(TierPermission));
        }

        dbTierPermission.IsEnabled = command.IsEnabled;
        await _repository.UpdateAsync(dbTierPermission);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateTierPermission",
                SourceApplication = "Emoney",
                Resource = "TierPermission",
                Details = new Dictionary<string, string>
                {
                    {"Id", dbTierPermission.Id.ToString() },
                    {"TierLevel", dbTierPermission.TierLevel.ToString() },
                    {"PermissionType", dbTierPermission.PermissionType.ToString() },
                    {"IsEnabled", dbTierPermission.IsEnabled.ToString() }
                }
            });

        return Unit.Value;
    }
}