using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.HealthCheck.Services;

public interface IEmailService
{
    public Task<bool> SendMailAsync(SendEmail request);
}
