using LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers;

public class StatisticsController : ApiControllerBase
{
    private readonly IEmoneyStatisticHttpClient _emoneyHttpClient;
    private readonly IMoneyTransferStatisticHttpClient _moneyTransferHttpClient;
    private readonly IPfStatisticHttpClient _pfHttpClient;
    private readonly IBillingStatisticHttpClient _billingHttpClient;
    private readonly IEpinStatisticHttpClient _epinHttpClient;

    public StatisticsController(IEmoneyStatisticHttpClient emoneyHttpClient,
        IMoneyTransferStatisticHttpClient moneyTransferHttpClient,
        IPfStatisticHttpClient pfHttpClient,
        IBillingStatisticHttpClient billingHttpClient,
        IEpinStatisticHttpClient epinHttpClient)
    {
        _emoneyHttpClient = emoneyHttpClient;
        _moneyTransferHttpClient = moneyTransferHttpClient;
        _pfHttpClient = pfHttpClient;
        _billingHttpClient = billingHttpClient;
        _epinHttpClient = epinHttpClient;
    }

    [HttpGet("emoney")]
    [Authorize(Policy = "Statistic:Read")]
    public async Task<EmoneyReportDto> GetEmoneyReportAsync(DateTime startDate, DateTime endDate)
    {
        return await _emoneyHttpClient.GetReportAsync(startDate, endDate);
    }

    [HttpGet("moneytransfer")]
    [Authorize(Policy = "Statistic:Read")]
    public async Task<MoneyTransferReportDto> GetMoneyTransferReportAsync(DateTime startDate, DateTime endDate)
    {
        return await _moneyTransferHttpClient.GetReportAsync(startDate, endDate);
    }

    [HttpGet("pf")]
    [Authorize(Policy = "Statistic:Read")]
    public async Task<PfReportDto> GetPfReportAsync(DateTime startDate, DateTime endDate)
    {
        return await _pfHttpClient.GetReportAsync(startDate, endDate);
    }

    [HttpGet("billing")]
    [Authorize(Policy = "Statistic:Read")]
    public async Task<BillingReportDto> GetBillingReportAsync(DateTime startDate, DateTime endDate)
    {
        return await _billingHttpClient.GetReportAsync(startDate, endDate);
    }

    [HttpGet("epin")]
    [Authorize(Policy = "Statistic:Read")]
    public async Task<EpinReportDto> GetEpinReportAsync(DateTime startDate, DateTime endDate)
    {
        return await _epinHttpClient.GetReportAsync(startDate, endDate);
    }

}
