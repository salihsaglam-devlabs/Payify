namespace LinkPara.ApiGateway.Card.Commons.Models;
public class ClientList
{
    public List<ClientCredential> Clients { get; set; }
}

public class ClientCredential
{
    public string Username { get; set; }
    public string Password { get; set; }
}