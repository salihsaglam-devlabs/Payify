using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class SyncOperationalTransferReportUserModel
{
    public Guid UserId { get; set; }
    public OperationalTransferNotificationType NotificationType { get; set; }
}
