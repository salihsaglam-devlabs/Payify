using LinkPara.HttpProviders.PF;
using LinkPara.HttpProviders.PF.Models.Enums;
using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Request;
using LinkPara.IKS.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;



namespace LinkPara.IKS.Infrastructure.Consumers.CronJobs
{
    public class IKSTimeoutTransactionConsumer : IConsumer<ProcessIKSTimeouts>
    {
        private readonly ILogger<IKSTimeoutTransactionConsumer> _logger;
        private readonly IGenericRepository<TimeoutIKSTransaction> _timeoutIksTransactionRepository;
        private readonly IIKSService _iKSService;
        private readonly IMerchantService _merchantService;

        public IKSTimeoutTransactionConsumer(ILogger<IKSTimeoutTransactionConsumer> logger,
                                                         IGenericRepository<TimeoutIKSTransaction> timeout_IksTransactionRepository,
                                                         IIKSService iKSService,
                                                         IMerchantService merchantService)
        {
            _logger = logger;
            _timeoutIksTransactionRepository = timeout_IksTransactionRepository;
            _iKSService = iKSService;
            _merchantService = merchantService;
        }

        public async Task Consume(ConsumeContext<ProcessIKSTimeouts> context)
        {
            await TimeoutTransactionAsync();
        }

        private async Task TimeoutTransactionAsync()
        {
            try
            {
                var timeoutTransactions = await _timeoutIksTransactionRepository.GetAll()
                                                          .Where(b => !b.IsSuccess)
                                                          .ToListAsync();
                if (timeoutTransactions.Any())
                {
                    foreach (var item in timeoutTransactions)
                    {
                        try
                        {
                            if (item is null)
                            {
                                return;
                            }
                            await TimeoutTransactionItemAsync(item);

                        }
                        catch (Exception exception)
                        {
                            _logger.LogError($"ProcessIKSTimeoutTransactionItem Consumer Error {exception}");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"TimeoutTransaction Consumer Error {exception}");
            }
        }

        private async Task TimeoutTransactionItemAsync(TimeoutIKSTransaction timeoutIKSTransaction)
        {
            try
            {

                string requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(timeoutIKSTransaction.RequestDetails, Newtonsoft.Json.Formatting.Indented);
                var merchantRequest = Newtonsoft.Json.JsonConvert.DeserializeObject<MerchantRequest>(requestJson);

                var merchantsQuery = await _iKSService.MerchantsQueryAsync(new MerchantsQueryRequest
                {
                    PspMerchantId = merchantRequest?.PspMerchantId,
                    TaxNo = merchantRequest?.TaxNo,
                });

                var merchant = merchantsQuery?.Data?.merchants.FirstOrDefault(x => x.taxNo == merchantRequest?.TaxNo
                                                                               && x.pspMerchantId == merchantRequest?.PspMerchantId);


                if (merchant != null && !timeoutIKSTransaction.IsSuccess)
                {
                    await _merchantService.UpdateMerchantIKSAsync(new HttpProviders.PF.Models.Request.UpdateMerchantIKSModel
                    {
                        GlobalMerchantId = merchant.globalMerchantId,
                        MerchantStatus = MerchantStatus.Active,
                        Id = timeoutIKSTransaction.MerchantId
                    });

                    string timeoutReturnDetailsJson = Newtonsoft.Json.JsonConvert.SerializeObject(merchant);

                    timeoutIKSTransaction.IsSuccess = true;
                    timeoutIKSTransaction.TimeoutReturnDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(timeoutReturnDetailsJson);

                    await _timeoutIksTransactionRepository.UpdateAsync(timeoutIKSTransaction);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"TimeoutTransactionItem Error {exception}");

            }
        }
    }
}
