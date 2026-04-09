namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class ConnectionsOptions
{
    public EndpointOptions Source { get; set; }
    public EndpointOptions Target { get; set; }

    public void Validate()
    {
        if (Source is null)
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Connections.Source");
        if (Target is null)
            throw new InvalidOperationException("Vault configuration missing: FileIngestion.Connections.Target");

        Source.Validate();
        Target.Validate();
    }
}
