using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs
{
    public class SyncWalletBalanceDailyConsumer : IConsumer<SyncWalletBalanceDaily>
    {
        private readonly IWalletService _walletService;

        public SyncWalletBalanceDailyConsumer(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public async Task Consume(ConsumeContext<SyncWalletBalanceDaily> context)
        {
            await _walletService.SyncWalletBalanceDailyAsync();
        }
    }
}