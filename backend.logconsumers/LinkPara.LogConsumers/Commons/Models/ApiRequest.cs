namespace LinkPara.LogConsumer.Commons.Models;

public class ApiRequest
{
    public string Host { get; set; }
    public string IPAdress { get; set; }
    public string Path { get; set; }
    public string Method { get; set; }
    public string Querystring { get; set; }
    public string Scheme { get; set; }
    public string RequestBody { get; set; }
}
