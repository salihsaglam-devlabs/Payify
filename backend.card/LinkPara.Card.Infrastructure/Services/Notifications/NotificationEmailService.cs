using LinkPara.SharedModels.BusModels.Commands.Notification;
using MassTransit;

namespace LinkPara.Card.Infrastructure.Services.Notifications;

public class NotificationEmailService : INotificationEmailService
{
    private static readonly Uri SendEmailExchangeUri = new("exchange:Notification.SendEmail");

    private readonly IBus _bus;

    public NotificationEmailService(IBus bus)
    {
        _bus = bus;
    }

    public async Task SendEmailAsync(
        string templateName,
        IReadOnlyDictionary<string, string> templateData,
        string toEmail,
        CancellationToken cancellationToken = default)
    {
        var endpoint = await _bus.GetSendEndpoint(SendEmailExchangeUri);
        await endpoint.Send(new SendEmail
        {
            TemplateName = templateName,
            DynamicTemplateData = templateData.ToDictionary(x => x.Key, x => x.Value),
            ToEmail = toEmail
        }, cancellationToken);
    }
}