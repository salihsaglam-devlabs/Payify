using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.Audit;

public class AuditLogService : IAuditLogService
{
    private readonly IBus _bus;
    private readonly ILogger<AuditLogService> _logger;
    private readonly IContextProvider _contextProvider;
    private readonly IApplicationUserService _applicationUserService;

    public AuditLogService(IBus bus,
                           ILogger<AuditLogService> logger,
                           IContextProvider contextProvider,
                           IApplicationUserService applicationUserService)
    {
        _bus = bus;
        _logger = logger;
        _contextProvider = contextProvider; 
        _applicationUserService = applicationUserService;   
    }
    public async Task AuditLogAsync(AuditLog auditLog)
    {
        try
        {
            auditLog.UserId = GetUserId(auditLog.UserId);
            auditLog.ClientIpAddress = _contextProvider.CurrentContext?.ClientIpAddress;
            auditLog.Channel = _contextProvider.CurrentContext?.Channel;
            auditLog.CorrelationId = _contextProvider.CurrentContext?.CorrelationId;

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.AuditLog"));
            await endpoint.Send(auditLog, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ExceptionOnSendAuditLog detail:\n{exception}");
        }
    }
    private Guid GetUserId(Guid userId)
    {
        if (userId != Guid.Empty)
        {
            return userId;
        }
        if (!String.IsNullOrEmpty(_contextProvider.CurrentContext?.UserId))
        {
            return Guid.Parse(_contextProvider.CurrentContext?.UserId);
        }

        return _applicationUserService.ApplicationUserId;
    }
}
