namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;

public class RequestResponseLogCreated
{
    public ApiRequest Request { get; set; }
    public ApiResponse Response { get; set; }
    public DateTime CreatedDate { get; set; }
}