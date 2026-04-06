using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(SendEmail request);
}
