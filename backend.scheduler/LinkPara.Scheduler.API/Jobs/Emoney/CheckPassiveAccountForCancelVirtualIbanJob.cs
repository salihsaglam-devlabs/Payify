using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Emoney
{
    public class CheckPassiveAccountForCancelVirtualIbanJob : IJobTrigger
    {
        private readonly IBus _bus;
        public CheckPassiveAccountForCancelVirtualIbanJob(IBus bus)
        {
            _bus = bus;
        }
        public async Task TriggerAsync(Commons.Entities.CronJob job)
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Emoney.CheckPassiveAccountForCancelVirtualIban"));
            await endpoint.Send(new CheckPassiveAccountForCancelVirtualIban(), tokenSource.Token);
        }
    }
}