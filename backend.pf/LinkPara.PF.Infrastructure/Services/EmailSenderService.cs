using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using MassTransit;

namespace LinkPara.PF.Infrastructure.Services;

public class EmailSenderService : IEmailSender
{
    private readonly IBus _bus;

    public EmailSenderService(IBus bus)
    {
        _bus = bus;
    }

    public async Task SendEmailAsync(SendEmail request)
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Notification.SendEmail"));
        await endpoint.Send(request);        
    }
}
