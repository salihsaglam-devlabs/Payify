using LinkPara.SharedModels.BusModels.Commands.Notification;
using MassTransit;

namespace LinkPara.AlertingSystem.Services;

public class EmailService : IEmailService
{
    private readonly IBus _bus;

    public EmailService(IBus bus)
    {
        _bus = bus;
    }

    public async Task SendEmailAsync(SendEmail request)
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Notification.SendEmail"));
        await endpoint.Send(request);
    }
}
