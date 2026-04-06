using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IPfStatisticHttpClient
{
    Task<PfReportDto> GetReportAsync(DateTime startDate, DateTime endDate);
}
