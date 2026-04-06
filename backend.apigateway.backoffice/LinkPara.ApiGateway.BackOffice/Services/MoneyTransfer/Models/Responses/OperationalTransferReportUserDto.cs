using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
public class OperationalTransferReportUserDto
{
    public Guid UserId { get; set; }
    public OperationalTransferNotificationType NotificationType { get; set; }
}
