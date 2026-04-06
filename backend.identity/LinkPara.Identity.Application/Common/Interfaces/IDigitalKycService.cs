using LinkPara.SharedModels.BusModels.IntegrationEvents.DigitalKyc;

namespace LinkPara.Identity.Application.Common.Interfaces;

public interface IDigitalKycService
{
    Task UpdateIntegrationStateAsync(UpdateIntegrationState request);
}
