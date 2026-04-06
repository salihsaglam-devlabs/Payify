using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.Approval.Application.Commons.Interfaces;

public interface IEmailSenderService
{
    Task SendEmailAsync(SendEmail request);
}