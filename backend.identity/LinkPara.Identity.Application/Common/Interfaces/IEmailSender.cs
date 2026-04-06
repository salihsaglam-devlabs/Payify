using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.Identity.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(SendEmail request);
}
