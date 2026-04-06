namespace LinkPara.ContextProvider;

public class Context
{
    public string UserId { get; set; }
    public string UserType { get; set; }
    public string ClientIpAddress { get; set; }
    public string Port { get; set; }
    public string Gateway { get; set; }
    public string Channel { get; set; }
    public string CanSeeSensitiveData { get; set; }
    public string CorrelationId { get; set; }
    public string Language { get; set; }
}