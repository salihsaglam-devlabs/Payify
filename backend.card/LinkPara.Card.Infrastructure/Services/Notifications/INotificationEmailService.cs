namespace LinkPara.Card.Infrastructure.Services.Notifications;

public interface INotificationEmailService
{
    Task SendEmailAsync(
        string templateName,
        IReadOnlyDictionary<string, string> templateData,
        string toEmail,
        CancellationToken cancellationToken = default);
}