using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface IMoneyTransferStatisticHttpClient
{
    Task<MoneyTransferReportDto> GetReportAsync(DateTime startDate, DateTime endDate);
}
