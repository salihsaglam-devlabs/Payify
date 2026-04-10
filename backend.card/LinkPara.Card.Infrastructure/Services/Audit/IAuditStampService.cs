using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore.Query;

namespace LinkPara.Card.Infrastructure.Services.Audit;

public interface IAuditStampService
{
    AuditStamp CreateStamp();
    TEntity StampForCreate<TEntity>(TEntity entity) where TEntity : AuditEntity;
    void StampForCreate(IEnumerable<AuditEntity> entities);
    TEntity StampForUpdate<TEntity>(TEntity entity) where TEntity : AuditEntity;
    void StampForUpdate(IEnumerable<AuditEntity> entities);

    SetPropertyCalls<TEntity> ApplyAuditUpdate<TEntity>(SetPropertyCalls<TEntity> setters) where TEntity : AuditEntity;

    void EnsureAuditContext();
    void SetAuditUserId(string? userId);
    string? GetCurrentAuditUserId();
}

public readonly record struct AuditStamp(string UserId, Guid UserGuid, DateTime Timestamp);

