namespace LinkPara.Card.Infrastructure.Services.AlertService;

public interface IAlertService
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}