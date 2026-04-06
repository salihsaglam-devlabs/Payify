using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.AlertingSystem.Services;

public interface IEmailService
{
    Task SendEmailAsync(SendEmail request);


}