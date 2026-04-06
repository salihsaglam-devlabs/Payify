using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.Identity.Application.Common.Interfaces;

public interface ISmsSender
{
    Task SendSmsAsync(SendSms request);
}
