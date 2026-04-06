namespace LinkPara.ApiGateway.BackOffice.Filters.LoginActionFilter;

public class LicenceCheckResponse
{
    public int status { get; set; }
    public string message { get; set; }
    public int remaining { get; set; }
    public DateTime lastExecution { get; set; }
}