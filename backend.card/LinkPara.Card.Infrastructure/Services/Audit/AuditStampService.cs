using System.Security.Cryptography;
using System.Text;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore.Query;

namespace LinkPara.Card.Infrastructure.Services.Audit;

internal sealed class AuditStampService : IAuditStampService
{
    private static readonly AsyncLocal<string?> AsyncLocalUserId = new();
    private readonly IContextProvider _contextProvider;

    public AuditStampService(IContextProvider contextProvider)
    {
        _contextProvider = contextProvider;
    }
    
    public AuditStamp CreateStamp()
    {
        var resolved = ResolveUser(AsyncLocalUserId.Value, _contextProvider.CurrentContext?.UserId);
        return new AuditStamp(resolved.UserId, resolved.UserGuid, DateTime.Now);
    }

    
    public TEntity StampForCreate<TEntity>(TEntity entity) where TEntity : AuditEntity
    {
        ApplyCreate(entity, CreateStamp());
        return entity;
    }

    public void StampForCreate(IEnumerable<AuditEntity> entities)
    {
        var stamp = CreateStamp();
        foreach (var entity in entities)
            ApplyCreate(entity, stamp);
    }

    public TEntity StampForUpdate<TEntity>(TEntity entity) where TEntity : AuditEntity
    {
        ApplyUpdate(entity, CreateStamp());
        return entity;
    }

    public void StampForUpdate(IEnumerable<AuditEntity> entities)
    {
        var stamp = CreateStamp();
        foreach (var entity in entities)
            ApplyUpdate(entity, stamp);
    }
    
    public SetPropertyCalls<TEntity> ApplyAuditUpdate<TEntity>(SetPropertyCalls<TEntity> setters)
        where TEntity : AuditEntity
    {
        var stamp = CreateStamp();
        return setters
            .SetProperty(e => e.UpdateDate, stamp.Timestamp)
            .SetProperty(e => e.LastModifiedBy, stamp.UserId);
    }
    
    public void EnsureAuditContext()
    {
        if (!string.IsNullOrWhiteSpace(AsyncLocalUserId.Value))
            return;

        var contextUserId = _contextProvider.CurrentContext?.UserId;
        if (!string.IsNullOrWhiteSpace(contextUserId))
            AsyncLocalUserId.Value = contextUserId;
    }

    public void SetAuditUserId(string? userId) => AsyncLocalUserId.Value = userId;

    public string? GetCurrentAuditUserId() => AsyncLocalUserId.Value;
    
    private static void ApplyCreate(AuditEntity entity, AuditStamp stamp)
    {
        if (entity.Id == Guid.Empty)
            entity.Id = Guid.NewGuid();

        entity.CreateDate = stamp.Timestamp;
        entity.CreatedBy = stamp.UserId;
        entity.RecordStatus = RecordStatus.Active;
    }

    private static void ApplyUpdate(AuditEntity entity, AuditStamp stamp)
    {
        entity.UpdateDate = stamp.Timestamp;
        entity.LastModifiedBy = stamp.UserId;
    }
    
    private static readonly Guid UrlNamespace = new("6ba7b811-9dad-11d1-80b4-00c04fd430c8");
    private const string SystemName = "System";
    private static readonly Guid SystemGuid = CreateDeterministicGuid(SystemName);

    private static (string UserId, Guid UserGuid) ResolveUser(string? accessorUserId, string? contextUserId)
    {
        var candidate = !string.IsNullOrWhiteSpace(accessorUserId)
            ? accessorUserId.Trim()
            : contextUserId?.Trim();

        if (string.IsNullOrWhiteSpace(candidate))
            return (SystemName, SystemGuid);

        if (Guid.TryParse(candidate, out var parsedGuid))
            return (candidate, parsedGuid);

        if (string.Equals(candidate, SystemName, StringComparison.OrdinalIgnoreCase))
            return (SystemName, SystemGuid);

        return (candidate, CreateDeterministicGuid(candidate));
    }

    private static Guid CreateDeterministicGuid(string value)
    {
        var namespaceBytes = UrlNamespace.ToByteArray();
        SwapByteOrder(namespaceBytes);

        var valueBytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA1.HashData(namespaceBytes.Concat(valueBytes).ToArray());
        var newGuid = new byte[16];
        Array.Copy(hash, newGuid, 16);

        newGuid[6] = (byte)((newGuid[6] & 0x0F) | 0x50);
        newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);
        SwapByteOrder(newGuid);

        return new Guid(newGuid);
    }

    private static void SwapByteOrder(byte[] guid)
    {
        (guid[0], guid[3]) = (guid[3], guid[0]);
        (guid[1], guid[2]) = (guid[2], guid[1]);
        (guid[4], guid[5]) = (guid[5], guid[4]);
        (guid[6], guid[7]) = (guid[7], guid[6]);
    }
}

internal static class AuditSetPropertyCallsExtensions
{
    public static SetPropertyCalls<TEntity> ApplyAuditUpdate<TEntity>(
        this SetPropertyCalls<TEntity> setters,
        AuditStamp stamp)
        where TEntity : AuditEntity
    {
        return setters
            .SetProperty(e => e.UpdateDate, stamp.Timestamp)
            .SetProperty(e => e.LastModifiedBy, stamp.UserId);
    }
}
