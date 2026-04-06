using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.DigitalKyc;
using MassTransit;

namespace LinkPara.Identity.Infrastructure.Services;

public class DigitalKycService : IDigitalKycService
{
    private readonly IBus _bus;
    public DigitalKycService(IBus bus)
    {
        _bus = bus;
    }
    public async Task UpdateIntegrationStateAsync(UpdateIntegrationState request)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:DigitalKyc.UpdateIntegration"));
        await endpoint.Send(request, tokenSource.Token);
    }
}
