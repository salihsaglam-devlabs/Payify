namespace LinkPara.Card.Application.Commons.Interfaces;

public interface INotificationEmailService
{
    Task SendEmailAsync(
        string templateName,
        IReadOnlyDictionary<string, string> templateData,
        string toEmail,
        CancellationToken cancellationToken = default);
}
