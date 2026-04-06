using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.DisableTierPermission;

public class DisableTierPermissionCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DisableTierPermissionCommandHandler : IRequestHandler<DisableTierPermissionCommand>
{
    private readonly IGenericRepository<TierPermission> _repository;
    private readonly IAuditLogService _auditLogService;

    public DisableTierPermissionCommandHandler(IGenericRepository<TierPermission> repository,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(DisableTierPermissionCommand command, CancellationToken cancellationToken)
    {
        var tierPermission = await _repository.GetByIdAsync(command.Id);
        if (tierPermission is null)
        {
            throw new NotFoundException(nameof(TierPermission), command.Id);
        }

        tierPermission.RecordStatus = RecordStatus.Passive;
        tierPermission.UpdateDate = DateTime.Now;

        await _repository.UpdateAsync(tierPermission);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "DeleteTierPermission",
                SourceApplication = "Emoney",
                Resource = "TierPermission",
                Details = new Dictionary<string, string>
                {
                    {"Id", tierPermission.Id.ToString() }
                }
            });

        return Unit.Value;
    }

}