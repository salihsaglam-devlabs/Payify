using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.Audit
{
    public class UserActivityLogService : IUserActivityLogService
    {
        private const string _channel = "Backoffice";
        private readonly IBus _bus;
        private readonly ILogger<UserActivityLogService> _logger;
        private readonly IContextProvider _contextProvider;
        private readonly IApplicationUserService _applicationUserService;
        public UserActivityLogService(IBus bus,
                                       ILogger<UserActivityLogService> logger,
                                       IContextProvider contextProvider,
                                       IApplicationUserService applicationUserService)
        {
            _bus = bus;
            _logger = logger;
            _contextProvider = contextProvider;
            _applicationUserService = applicationUserService;
        }
        public async Task UserActivityLogAsync(UserActivityLog userActivityLog)
        {
            try
            {
                userActivityLog.Channel = _contextProvider.CurrentContext?.Channel;
                if (userActivityLog.Channel != _channel)
                {
                    return;
                }
                userActivityLog.ViewerId = GetUserId(userActivityLog.ViewerId);
                userActivityLog.ViewedId = userActivityLog.ViewedId;
                userActivityLog.ClientIpAddress = _contextProvider.CurrentContext?.ClientIpAddress;
                userActivityLog.CorrelationId = _contextProvider.CurrentContext?.CorrelationId;

                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.UserActivityLog"));
                await endpoint.Send(userActivityLog, tokenSource.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError($"ExceptionOnSendUserActivityLog detail:\n{exception}");
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
}