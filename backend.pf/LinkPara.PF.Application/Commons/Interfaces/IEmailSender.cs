using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(SendEmail request);
}
