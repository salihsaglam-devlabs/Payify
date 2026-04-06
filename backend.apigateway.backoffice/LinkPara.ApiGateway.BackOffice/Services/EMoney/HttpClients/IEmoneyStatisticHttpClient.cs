using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface IEmoneyStatisticHttpClient
{
    Task<EmoneyReportDto> GetReportAsync(DateTime startDate, DateTime endDate);
}
