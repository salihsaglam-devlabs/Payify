namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IReconciliationAlarmService
{
    Task RaiseAsync(
        string alarmCode,
        string summary,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken cancellationToken = default);
}
