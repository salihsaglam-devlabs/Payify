using LinkPara.Approval.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using MassTransit;

namespace LinkPara.Approval.Infrastructure.Services;

public class EmailSenderService : IEmailSenderService
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