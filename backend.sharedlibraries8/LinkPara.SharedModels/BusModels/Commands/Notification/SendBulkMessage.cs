namespace LinkPara.SharedModels.BusModels.Commands.Notification;

public class SendBulkMessage
{
    public List<Guid> OrderHistoryIdList { get; set; }
}