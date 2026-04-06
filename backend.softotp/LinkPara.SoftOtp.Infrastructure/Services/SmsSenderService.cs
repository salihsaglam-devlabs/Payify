using LinkPara.SoftOtp.Application.Common.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using MassTransit;

namespace LinkPara.SoftOtp.Infrastructure.Services;

public class SmsSenderService : ISmsSender
{
    private readonly IBus _bus;

    public SmsSenderService(IBus bus)
    {
        _bus = bus;
    }

    public async Task SendSmsAsync(SendSms request)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Notification.SendSms"));
        await endpoint.Send(request, tokenSource.Token);        
    }
}