using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public interface IBillingStatisticHttpClient
{
    Task<BillingReportDto> GetReportAsync(DateTime startDate, DateTime endDate);
}
