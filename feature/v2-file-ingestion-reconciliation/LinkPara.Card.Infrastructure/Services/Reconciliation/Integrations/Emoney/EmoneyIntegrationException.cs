namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;

internal sealed class EmoneyIntegrationException : Exception
{
    public EmoneyIntegrationException(string message)
        : base(message)
    {
    }

    public EmoneyIntegrationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
