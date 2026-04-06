using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;

public interface IEpinStatisticHttpClient
{
    Task<EpinReportDto> GetReportAsync(DateTime startDate, DateTime endDate);
}
