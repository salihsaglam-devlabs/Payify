namespace LinkPara.Card.Infrastructure.Services.Alert;

public interface IAlertService
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}