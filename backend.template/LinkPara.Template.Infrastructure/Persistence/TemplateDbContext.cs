using System.Reflection;
using LinkPara.Template.Application.Commons.Interfaces;
using LinkPara.Template.Domain.Commons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace LinkPara.Template.Infrastructure.Persistence;

public class TemplateDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventService _domainEventService;
    private readonly IConfiguration _configuration;

    public TemplateDbContext(DbContextOptions options,
        ICurrentUserService currentUserService,
        IDomainEventService domainEventService,
        IConfiguration configuration)
        : base(options)
    {
        _currentUserService = currentUserService;
        _domainEventService = domainEventService;
        _configuration = configuration;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService.UserId;
                    entry.Entity.CreateDate = DateTimeOffset.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = _currentUserService.UserId;
                    entry.Entity.UpdateDate = DateTimeOffset.UtcNow;
                    break;
            }
        }

        var events = ChangeTracker.Entries<IHasDomainEvent>()
            .Select(x => x.Entity.DomainEvents)
            .SelectMany(x => x)
            .Where(domainEvent => !domainEvent.IsPublished)
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken);

        await DispatchEvents(events);

        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        var databaseProvider = _configuration["DatabaseProvider"];

        switch (databaseProvider)
        {
            case "PostgreSql":
                CreateDefaultMappings(builder);
                break;
            case "MsSql":
                CreateMsSqlMappings(builder);
                break;
            default:
                CreateDefaultMappings(builder);
                break;
        }

        base.OnModelCreating(builder);

        ConvertEnumColumnsToString(builder);
    }

    private static void ConvertEnumColumnsToString(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
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

    private async Task DispatchEvents(DomainEvent[] events)
    {
        foreach (var @event in events)
        {
            @event.IsPublished = true;
            await _domainEventService.Publish(@event);
        }
    }

    private static void CreateDefaultMappings(ModelBuilder builder)
    {
        var schema = "core";
    }

    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        var schema = "Core";
    }
}