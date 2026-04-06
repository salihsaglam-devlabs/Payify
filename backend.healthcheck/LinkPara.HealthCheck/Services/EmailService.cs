using LinkPara.SharedModels.BusModels.Commands.Notification;
using MassTransit;

namespace LinkPara.HealthCheck.Services;

public class EmailService : IEmailService
{
    private readonly IBus _bus;
    public EmailService(IBus bus)
    {
        _bus = bus;
    }

    public async Task<bool> SendMailAsync(SendEmail request)
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Notification.SendEmail"));
            await endpoint.Send(request, tokenSource.Token);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
