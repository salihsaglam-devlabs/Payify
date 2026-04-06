using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Identity.Infrastructure.Consumers.CronJobs;

    public class DeleteUserSessionConsumer : IConsumer<DeleteUserSession>
{
    private readonly ILogger<DeleteUserSessionConsumer> _logger;
    private readonly ApplicationDbContext _context;

    public DeleteUserSessionConsumer(ILogger<DeleteUserSessionConsumer> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    public async Task Consume(ConsumeContext<DeleteUserSession> context)
    {
        await DeleteUserSessionAsync();
    }
    private async Task DeleteUserSessionAsync()
    {
        var expirationTimeLimit = DateTime.UtcNow.AddHours(-1);

        var userSessions = await _context.UserSession
                .Where(b => b.RefreshTokenExpiration <= expirationTimeLimit)
                .ToListAsync();

        if (userSessions.Any())
        {
            try
            {
                _context.RemoveRange(userSessions);

                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError($"UserSessions Consumer Error {exception}");
            }
        }
    }
}
