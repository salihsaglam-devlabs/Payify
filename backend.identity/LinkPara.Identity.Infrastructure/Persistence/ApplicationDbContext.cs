using System.Reflection;
using LinkPara.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using LinkPara.SharedModels.Exceptions;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.DomainEvents;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MassTransit;
using Microsoft.Extensions.Configuration;
using LinkPara.SharedModels.Persistence;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Identity.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid, UserClaim, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, RoleClaim, IdentityUserToken<Guid>>
{
    public DbSet<AgreementDocument> AgreementDocument { get; set; }
    public DbSet<AgreementDocumentVersion> AgreementDocumentVersion { get; set; }
    public DbSet<UserAgreementDocument> UserAgreementDocument { get; set; }
    public DbSet<SecurityQuestion> SecurityQuestion { get; set; }
    public DbSet<UserSecurityAnswer> UserSecurityAnswer { get; set; }
    public DbSet<UserPasswordHistory> UserPasswordHistory { get; set; }
    public DbSet<UserAddress> UserAddress { get; set; }
    public DbSet<UserPicture> UserPictures { get; set; }
    public DbSet<UserLoginLastActivity> UserLoginLastActivity { get; set; }
    public DbSet<Permission> Permission { get; set; }
    public DbSet<DeviceInfo> DeviceInfo { get; set; }
    public DbSet<UserDeviceInfo> UserDeviceInfo { get; set; }
    public DbSet<UserSession> UserSession { get; set; }
    public DbSet<RoleScreen> RoleScreen { get; set; }
    public DbSet<Screen> Screen { get; set; }
    public DbSet<ScreenClaim> ScreenClaim { get; set; }
    public DbSet<LoginActivity> LoginActivity { get; set; }

    public DbSet<LoginWhitelist> LoginWhiteList { get; set; }
    public DbSet<SecurityPicture> SecurityPicture { get; set; }
    public DbSet<UserSecurityPicture> UserSecurityPicture { get; set; }

    private readonly IContextProvider _contextProvider;
    private readonly IDomainEventService _domainEventService;
    private readonly IBus _bus;
    private readonly IVaultClient _vaultClient;
    
    public ApplicationDbContext(
        DbContextOptions options, 
        IVaultClient vaultClient,
        IContextProvider contextProvider,
        IDomainEventService domainEventService,
        IBus bus
        ) : base(options)
    {
        _vaultClient = vaultClient;
        _contextProvider = contextProvider;
        _domainEventService = domainEventService;
        _bus = bus;
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        var databaseProvider = _vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        base.OnModelCreating(builder);
        switch (databaseProvider)
        {
            case "MsSql":
                CreateMsSqlMappings(builder);
                break;
            default:
                CreateDefaultMappings(builder);
                break;
        }      
        ConvertEnumColumnsToString(builder);
    }
    private static void CreateDefaultMappings(ModelBuilder builder)
    {
        var schema = "core";
        builder.Entity<User>().ToTable("user", schema); 
        builder.Entity<Role>().ToTable("role", schema); 
        builder.Entity<RoleClaim>().ToTable("role_claim", schema);
        builder.Entity<UserClaim>().ToTable("user_claim", schema); 
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_token", schema);
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_role", schema);
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_login", schema);

        builder.Entity<AgreementDocument>().ToTable("agreement_document", schema);
        builder.Entity<AgreementDocumentVersion>().ToTable("agreement_document_version", schema);
        builder.Entity<DeviceInfo>().ToTable("device_info", schema);
        builder.Entity<Permission>().ToTable("permission", schema);
        builder.Entity<SecurityQuestion>().ToTable("security_question", schema);
        builder.Entity<UserAddress>().ToTable("user_address", schema);
        builder.Entity<UserAgreementDocument>().ToTable("user_agreement_document", schema);
        builder.Entity<UserDeviceInfo>().ToTable("user_device_info", schema);
        builder.Entity<UserLoginLastActivity>().ToTable("user_login_last_activity", schema);
        builder.Entity<UserPasswordHistory>().ToTable("user_password_history", schema);
        builder.Entity<UserPicture>().ToTable("user_picture", schema);
        builder.Entity<UserSecurityAnswer>().ToTable("user_security_answer", schema);
        builder.Entity<UserSession>().ToTable("user_session", schema);
        builder.Entity<RoleScreen>().ToTable("role_screen", schema);
        builder.Entity<Screen>().ToTable("screen", schema);
        builder.Entity<ScreenClaim>().ToTable("screen_claim", schema);
        builder.Entity<LoginWhitelist>().ToTable("login_whitelist", schema);
        builder.Entity<LoginActivity>().ToTable("login_activity", schema);
        builder.Entity<SecurityPicture>().ToTable("security_picture", schema);
        builder.Entity<UserSecurityPicture>().ToTable("user_security_picture", schema);
    }
    private static void CreateMsSqlMappings(ModelBuilder builder)
    {
        var schema = "Core";
        builder.Entity<User>().ToTable("User", schema); ;
        builder.Entity<Role>().ToTable("Role", schema); ;
        builder.Entity<RoleClaim>().ToTable("RoleClaim", schema);
        builder.Entity<UserClaim>().ToTable("UserClaim", schema); ;
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserToken", schema);
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRole", schema);
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogin", schema);

        builder.Entity<AgreementDocument>().ToTable("AgreementDocument", schema);
        builder.Entity<AgreementDocumentVersion>().ToTable("AgreementDocumentVersion", schema);
        builder.Entity<DeviceInfo>().ToTable("DeviceInfo", schema);
        builder.Entity<Permission>().ToTable("Permission", schema);
        builder.Entity<SecurityQuestion>().ToTable("SecurityQuestion", schema);
        builder.Entity<UserAddress>().ToTable("UserAddress", schema);
        builder.Entity<UserAgreementDocument>().ToTable("UserAgreementDocument", schema);
        builder.Entity<UserDeviceInfo>().ToTable("UserDeviceInfo", schema);
        builder.Entity<UserLoginLastActivity>().ToTable("UserLoginLastActivity", schema);
        builder.Entity<UserPasswordHistory>().ToTable("UserPasswordHistory", schema);
        builder.Entity<UserPicture>().ToTable("UserPicture", schema);
        builder.Entity<UserSecurityAnswer>().ToTable("UserSecurityAnswer", schema);
        builder.Entity<UserSession>().ToTable("UserRefreshToken", schema);
        builder.Entity<RoleScreen>().ToTable("RoleScreen", schema);
        builder.Entity<Screen>().ToTable("Screen", schema);
        builder.Entity<ScreenClaim>().ToTable("ScreenClaim", schema);
        builder.Entity<LoginWhitelist>().ToTable("LoginWhitelist", schema);
        builder.Entity<LoginActivity>().ToTable("LoginActivity", schema);
        builder.Entity<SecurityPicture>().ToTable("SecurityPicture", schema);
        builder.Entity<UserSecurityPicture>().ToTable("UserSecurityPicture", schema);
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

        await DispatchEvents(events);

        await DispatchChangeTrackLogsAsync(entityChangeLog);

        return result;
    }
    private async Task DispatchChangeTrackLogsAsync(List<EntityChangeLogModel> entityChangeLogList)
    {
        foreach (var item in entityChangeLogList)
        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var busEndpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.ChangeTracker"));
            await busEndpoint.Send(item, cancellationToken.Token);
        }
    }
    private List<EntityChangeLogModel> PrepareChangeTrackLogs(string userId)
    {
        var entries = new List<EntityChangeLogModel>();

        return ApplyChangeTracker(userId, entries);
    }
    private List<EntityChangeLogModel> ApplyChangeTracker(string userId, List<EntityChangeLogModel> entries)
    {
        foreach (var entry in base.ChangeTracker.Entries<ITrackChange>())
        {
            var schemaName = GetSchema(entry);

            var entityChangeLog = new EntityChangeLogModel
            {
                ServiceName = base.Database.GetDbConnection().Database,
                ShemaName = schemaName,
                TableName = entry.Entity.GetType().Name,
                UserId = userId,
                ClientIpAddress = _contextProvider.CurrentContext?.ClientIpAddress,
                LogDate = DateTime.Now,
                CorrelationId = _contextProvider.CurrentContext?.CorrelationId,
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
    private string GetSchema(EntityEntry entry)
    {
        var entity = entry.Entity;
        var schemaAnnotation = base.Model.FindEntityType(entity.GetType()).GetAnnotations()
        .FirstOrDefault(a => a.Name == "Relational:Schema");
        return schemaAnnotation == null ? "dbo" : schemaAnnotation.Value.ToString();
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
            await _domainEventService.PublishAsync(@event);
        }
    }
}