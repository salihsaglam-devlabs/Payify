using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface IIntegrationLogger
{
    public Task QueueLogAsync(IntegrationLog log);
}