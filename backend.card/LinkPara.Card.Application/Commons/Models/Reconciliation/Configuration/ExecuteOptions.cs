using LinkPara.Card.Application.Commons.Exceptions;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Configuration;

public class ExecuteOptions
{
    public const int DefaultMaxEvaluations = 100_000;
    public const int DefaultLeaseSeconds = 300;

    public int? MaxEvaluations { get; set; }

    public int? LeaseSeconds { get; set; }

    public void ValidateAndApplyDefaults()
    {
        MaxEvaluations ??= DefaultMaxEvaluations;
        LeaseSeconds ??= DefaultLeaseSeconds;

        if (MaxEvaluations <= 0)
            throw new ReconciliationExecuteMaxEvaluationsInvalidException($"Reconciliation.Execute.MaxEvaluations must be positive. Current: {MaxEvaluations}");
        if (LeaseSeconds <= 0)
            throw new ReconciliationExecuteLeaseSecondsInvalidException($"Reconciliation.Execute.LeaseSeconds must be positive. Current: {LeaseSeconds}");
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}
