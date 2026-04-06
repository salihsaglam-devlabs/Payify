using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SoftOtp.Application.Common.Interfaces;
using MassTransit;

namespace LinkPara.SoftOtp.Infrastructure.Services;

public class PushNotificationSenderService : IPushNotificationSender
{
    private readonly IBus _bus;
    public PushNotificationSenderService(IBus bus)
    {
        _bus = bus;
    }

    public async Task SendPushNotificationAsync(SendPushNotification request)
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Notification.SendPushNotification"));
        await endpoint.Send(request);
    }
}