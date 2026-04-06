using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.SoftOtp.Application.Common.Interfaces;

public interface ISmsSender
{
    Task SendSmsAsync(SendSms request);
}