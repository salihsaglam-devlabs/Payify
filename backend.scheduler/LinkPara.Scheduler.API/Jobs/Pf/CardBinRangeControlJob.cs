using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Pf;

public class CardBinRangeControlJob : IJobTrigger
{
	private readonly IBus _bus;

	public CardBinRangeControlJob(IBus bus)
	{
		_bus = bus;
	}

	public async Task TriggerAsync(CronJob job)
	{
		var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.CardBinRangeControl"));
		await endpoint.Send(new CardBinRange(), tokenSource.Token);
	}
}
