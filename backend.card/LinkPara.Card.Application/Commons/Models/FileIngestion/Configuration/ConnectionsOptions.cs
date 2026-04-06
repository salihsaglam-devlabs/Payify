namespace LinkPara.Card.Application.Commons.Models.FileIngestion;

public class ConnectionsOptions
{
    public EndpointOptions Source { get; set; } = new();
    public EndpointOptions Target { get; set; } = new();
}
