using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Exceptions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


namespace LinkPara.SharedModels.Persistence;

public class BaseDbContext : DbContext
{
    private readonly IDomainEventService _domainEventService;
    private readonly IContextProvider _contextProvider;
    private readonly IBus _bus;
    public BaseDbContext(DbContextOptions options
                            , IContextProvider contextProvider
                            , IDomainEventService domainEventService
                            , IBus bus) : base(options)
    {
        _contextProvider = contextProvider;
        _domainEventService = domainEventService;
        _bus = bus;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {        
        foreach (var entry in ChangeTracker.Entries<AuditEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (string.IsNullOrWhiteSpace(entry.Entity.CreatedBy))
                    {
                        if (string.IsNullOrWhiteSpace(_contextProvider.CurrentContext.UserId))
                        {
                            throw new AudititableInfoMissingException();
                        }
                        entry.Entity.CreatedBy = _contextProvider.CurrentContext.UserId;
                    }
                    entry.Entity.CreateDate = DateTime.Now;
                    entry.Entity.RecordStatus = RecordStatus.Active;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = !string.IsNullOrWhiteSpace(entry.Entity.LastModifiedBy)
                        ? entry.Entity.LastModifiedBy
                        : _contextProvider.CurrentContext.UserId;
                    entry.Entity.UpdateDate = DateTime.Now;
                    break;
            }
        }

        var events = ChangeTracker.Entries<IHasDomainEvent>()
           .Select(x => x.Entity.DomainEvents)
           .SelectMany(x => x)
           .Where(domainEvent => !domainEvent.IsPublished)
           .ToArray();

        var entityChangeLog = PrepareChangeTrackLogs(_contextProvider.CurrentContext.UserId);

        var result = await base.SaveChangesAsync(cancellationToken);

        await Task.Run(() => DispatchEvents(events), cancellationToken);
        await Task.Run(() => DispatchChangeTrackLogsAsync(entityChangeLog), cancellationToken);

        return result;
    }
    private List<EntityChangeLogModel> PrepareChangeTrackLogs(string userId)
    {
        var entries = new List<EntityChangeLogModel>();

        return ApplyChangeTracker(userId, entries);
    }
    private List<EntityChangeLogModel> ApplyChangeTracker(string userId, List<EntityChangeLogModel> entries)
    {
        foreach (var entry in ChangeTracker.Entries<ITrackChange>())
        {
            var schemaName = GetSchema(entry);

            var entityChangeLog = new EntityChangeLogModel
            {
                ShemaName = schemaName,
                TableName = entry.Entity.GetType().Name,
                UserId = userId,
                ClientIpAddress = _contextProvider.CurrentContext?.ClientIpAddress,
                ServiceName = base.Database.GetDbConnection().Database,
                LogDate = DateTime.Now,
                CorrelationId = _contextProvider.CurrentContext?.CorrelationId
            };

            ApplyEntityState(entries, entry, entityChangeLog);
        }
        return entries;
    }
    private static void ApplyEntityState(List<EntityChangeLogModel> entries, EntityEntry<ITrackChange> entry, EntityChangeLogModel entityChangeLog)
    {
        if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
        {
            entries.Add(entityChangeLog);

            var databaseValues = entry.GetDatabaseValues();

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    entityChangeLog.KeyValues[propertyName] = property.CurrentValue?.ToString();
                    continue;
                }
                switch (entry.State)
                {
                    case EntityState.Added:
                        entityChangeLog.CrudOperationType = CrudOperationType.Create;
                        entityChangeLog.NewValues[propertyName] = property.CurrentValue?.ToString();
                        break;

                    case EntityState.Deleted:
                        entityChangeLog.CrudOperationType = CrudOperationType.Delete;
                        entityChangeLog.OldValues[propertyName] = property.OriginalValue?.ToString();
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            if (databaseValues[propertyName]?.ToString() != property.CurrentValue?.ToString())
                            {
                                entityChangeLog.AffectedColumns.Add(propertyName);
                                entityChangeLog.NewValues[propertyName] = property.CurrentValue?.ToString();
                            }
                            entityChangeLog.OldValues[propertyName] = databaseValues[propertyName]?.ToString();
                            entityChangeLog.CrudOperationType = CrudOperationType.Update;
                        }
                        break;
                }
            }
        }
    }
    private async Task DispatchChangeTrackLogsAsync(List<EntityChangeLogModel> entityChangeLogList)
    {
        try
        {
            foreach (var item in entityChangeLogList)
            {
                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var busEndpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.ChangeTracker"));
                await busEndpoint.Send(item, cancellationToken.Token);
            }
        }
        catch (Exception){}        
    }
    private string GetSchema(EntityEntry entry)
    {
        var entity = entry.Entity;
        var schemaAnnotation = base.Model.FindEntityType(entity.GetType()).GetAnnotations()
        .FirstOrDefault(a => a.Name == "Relational:Schema");
        return schemaAnnotation == null ? "dbo" : schemaAnnotation.Value.ToString();
    }
    private async Task DispatchEvents(DomainEvent[] events)
    {
        try
        {
            foreach (var @event in events)
            {
                @event.IsPublished = true;
                await _domainEventService.PublishAsync(@event);
            }
        }
        catch (Exception){}    
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConvertEnumColumnsToString(modelBuilder);
    }
    private static void ConvertEnumColumnsToString(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType.IsEnum)
                {
                    var type = typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType);
                    var converter = Activator.CreateInstance(type, new ConverterMappingHints()) as ValueConverter;

                    property.SetValueConverter(converter);
                    property.SetMaxLength(50);
                }
            }
        }
    }
}
