using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.PatchTierPermission;

public class PatchTierPermissionCommand : IRequest
{
    public Guid Id { get; set; }
    public JsonPatchDocument<TierPermissionDto> PatchTierPermission { get; set; }
}

public class PatchTierPermissionCommandHandler : IRequestHandler<PatchTierPermissionCommand>
{
    private readonly IGenericRepository<TierPermission> _repository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public PatchTierPermissionCommandHandler(IGenericRepository<TierPermission> repository,
        IMapper mapper,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _mapper = mapper;
        _auditLogService = auditLogService; 
    }

    public async Task<Unit> Handle(PatchTierPermissionCommand command, CancellationToken cancellationToken)
    {
        var dbTierPermission = await _repository.GetByIdAsync(command.Id);

        if (dbTierPermission is null)
        {
            throw new NotFoundException(nameof(TierPermission));
        }

        var requestTierPermissionDto = _mapper.Map<TierPermissionDto>(dbTierPermission);

        command.PatchTierPermission.ApplyTo(requestTierPermissionDto);
        
        _mapper.Map(requestTierPermissionDto, dbTierPermission);

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