using LinkPara.ContextProvider;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Infrastructure.Services.Audit;

internal sealed class AuditStampService : IAuditStampService
{
    private readonly IContextProvider _contextProvider;
    private readonly IAuditUserContextAccessor _auditUserContextAccessor;

    public AuditStampService(
        IContextProvider contextProvider,
        IAuditUserContextAccessor auditUserContextAccessor)
    {
        _contextProvider = contextProvider;
        _auditUserContextAccessor = auditUserContextAccessor;
    }

    public AuditStamp CreateStamp()
    {
        var userId = _auditUserContextAccessor.CurrentUserId ?? _contextProvider.CurrentContext?.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Audit user context is required for persistence operations.");
        }

        return new AuditStamp(userId, DateTime.UtcNow);
    }

    public void StampForCreate(AuditEntity entity)
    {
        var stamp = CreateStamp();
        ApplyCreate(entity, stamp);
    }

    public void StampForCreate(IEnumerable<AuditEntity> entities)
    {
        var stamp = CreateStamp();
        foreach (var entity in entities)
        {
            ApplyCreate(entity, stamp);
        }
    }

    public void StampForUpdate(AuditEntity entity)
    {
        var stamp = CreateStamp();
        ApplyUpdate(entity, stamp);
    }

    public void StampForUpdate(IEnumerable<AuditEntity> entities)
    {
        var stamp = CreateStamp();
        foreach (var entity in entities)
        {
            ApplyUpdate(entity, stamp);
        }
    }

    private static void ApplyCreate(AuditEntity entity, AuditStamp stamp)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        entity.CreateDate = stamp.Timestamp;
        entity.CreatedBy = stamp.UserId;
        entity.RecordStatus = RecordStatus.Active;
    }

    private static void ApplyUpdate(AuditEntity entity, AuditStamp stamp)
    {
        entity.UpdateDate = stamp.Timestamp;
        entity.LastModifiedBy = stamp.UserId;
    }
}
