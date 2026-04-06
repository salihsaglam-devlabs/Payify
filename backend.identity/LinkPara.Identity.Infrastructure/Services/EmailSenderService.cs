using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using MassTransit;

namespace LinkPara.Identity.Infrastructure.Services;

public class EmailSenderService : IEmailSender
{
    private readonly IBus _bus;

    public EmailSenderService(IBus bus)
    {
        _bus = bus;
    }

    public async Task SendEmailAsync(SendEmail request)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Notification.SendEmail"));
        await endpoint.Send(request, tokenSource.Token);        
    }
}
