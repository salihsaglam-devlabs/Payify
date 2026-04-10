using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore.Query;

namespace LinkPara.Card.Infrastructure.Services.Audit;

/// <summary>
/// Audit property'lerinin tek noktadan yönetilmesini sağlar.
/// Tüm Create / Update işlemlerinde audit alanları bu servis üzerinden setlenir.
/// </summary>
public interface IAuditStampService
{
    // ── Stamp factory ────────────────────────────────────────────────
    AuditStamp CreateStamp();

    // ── Entity-level stamping ────────────────────────────────────────
    TEntity StampForCreate<TEntity>(TEntity entity) where TEntity : AuditEntity;
    void StampForCreate(IEnumerable<AuditEntity> entities);
    TEntity StampForUpdate<TEntity>(TEntity entity) where TEntity : AuditEntity;
    void StampForUpdate(IEnumerable<AuditEntity> entities);

    // ── EF Core ExecuteUpdate desteği ────────────────────────────────
    SetPropertyCalls<TEntity> ApplyAuditUpdate<TEntity>(SetPropertyCalls<TEntity> setters) where TEntity : AuditEntity;

    // ── Consumer / background-job audit context ──────────────────────
    void EnsureAuditContext();
    void SetAuditUserId(string? userId);
    string? GetCurrentAuditUserId();
}

// ── Value objects ────────────────────────────────────────────────────
public readonly record struct AuditStamp(string UserId, Guid UserGuid, DateTime Timestamp);

