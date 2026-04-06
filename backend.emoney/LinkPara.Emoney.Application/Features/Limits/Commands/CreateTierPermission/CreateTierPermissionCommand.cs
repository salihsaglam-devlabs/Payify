using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Attributes;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.CreateTierPermission;

public class CreateTierPermissionCommand : IRequest
{
    [Audit]
    public TierLevelType TierLevel { get; set; }
    [Audit]
    public TierPermissionType PermissionType { get; set; }
    [Audit]
    public bool IsEnabled { get; set; }
}

public class CreateTierPermissionCommandHandler : IRequestHandler<CreateTierPermissionCommand>
{
    private readonly IGenericRepository<TierPermission> _repository;
    private readonly IContextProvider _contextProvider;

    public CreateTierPermissionCommandHandler(
        IGenericRepository<TierPermission> repository,
        IContextProvider contextProvider)
    {
        _repository = repository;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(CreateTierPermissionCommand request, CancellationToken cancellationToken)
    {
        var duplicateCheck = await _repository.GetAll()
            .AnyAsync(s =>
                s.TierLevel == request.TierLevel &&
                s.PermissionType == request.PermissionType &&
                s.RecordStatus == RecordStatus.Active, cancellationToken: cancellationToken);

        if (duplicateCheck)
        {
            throw new DuplicateRecordException();
        }

        var tierPermission = new TierPermission
        {
            TierLevel = request.TierLevel,
            PermissionType = request.PermissionType,
            IsEnabled = request.IsEnabled,
            CreatedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString()
        };

        await _repository.AddAsync(tierPermission);

        return Unit.Value;
    }
}