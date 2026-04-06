using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
public interface IOperationalTransferReportUserHttpClient
{
    Task<List<OperationalTransferReportUserDto>> GetListAsync();
    Task SyncAsync(SyncOperationalTransferReportUserRequest request);
    Task DeleteAsync(Guid id);
}
