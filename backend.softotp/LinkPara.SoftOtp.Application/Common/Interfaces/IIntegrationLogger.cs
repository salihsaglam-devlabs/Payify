using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;

namespace LinkPara.SoftOtp.Application.Common.Interfaces;

public interface IIntegrationLogger
{
    public Task QueueLogAsync(IntegrationLog log);
}