namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ReconciliationOptions
{
    public const string SectionName = "Reconciliation";

    public ConsumerOptions Consumer { get; set; }

    public AlertOptions Alert { get; set; }
    
    public EvaluateOptions Evaluate { get; set; }

    public ExecuteOptions Execute { get; set; }

    public void ValidateAndApplyDefaults()
    {
        Consumer ??= new ConsumerOptions();
        Alert ??= new AlertOptions();
        Evaluate ??= new EvaluateOptions();
        Execute ??= new ExecuteOptions();

        Consumer.ValidateAndApplyDefaults();
        Alert.ValidateAndApplyDefaults();
        Evaluate.ValidateAndApplyDefaults();
        Execute.ValidateAndApplyDefaults();
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}

public class ConsumerOptions
{
    public const bool DefaultRespondToContext = false;

    public bool? RespondToContext { get; set; }

    public void ValidateAndApplyDefaults()
    {
        RespondToContext ??= DefaultRespondToContext;
    }

    [Obsolete("Use ValidateAndApplyDefaults() instead")]
    public void Validate() => ValidateAndApplyDefaults();
}
