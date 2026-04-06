using LinkPara.PF.Application.Commons.Models.HostedPayment;
using LinkPara.PF.Application.Commons.Models.Payments;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class TriggerHppWebhookConsumer : IConsumer<TriggerHppWebhook>
{
    private readonly IGenericRepository<HostedPayment> _repository;
    private readonly IGenericRepository<OnUsPayment> _onUsPaymentRepository;
    private readonly ILogger<TriggerHppWebhookConsumer> _logger;
    private readonly IBus _bus;
    
    public TriggerHppWebhookConsumer(
        IGenericRepository<HostedPayment> repository,
        IGenericRepository<OnUsPayment> onUsPaymentRepository,
        ILogger<TriggerHppWebhookConsumer> logger,
        IBus bus)
    {
        _repository = repository;
        _onUsPaymentRepository = onUsPaymentRepository;
        _logger = logger;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<TriggerHppWebhook> context)
    {
        var activeHostedPayments = await _repository
            .GetAll()
            .Where(s => s.HppStatus != ChannelStatus.Active && s.WebhookStatus == WebhookStatus.Pending)
            .ToListAsync();

        foreach (var item in activeHostedPayments)
        {
            try
            {
                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.HppWebhookNotification"));
                await endpoint.Send(new HppWebhookNotification { TrackingId = item.TrackingId, MerchantId = item.MerchantId}, tokenSource.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError($"TriggerHppWebhookConsumer - TrackingId({item.TrackingId}) Error : {exception}");
            }
        }
        
        var activeOnUsPayments = await _onUsPaymentRepository
            .GetAll()
            .Where(s => s.Status != ChannelStatus.Active && s.WebhookStatus == WebhookStatus.Pending)
            .ToListAsync();

        foreach (var item in activeOnUsPayments)
        {
            try
            {
                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.OnUsWebhook"));
                await endpoint.Send(new OnUsWebhook { OnUsId = item.Id }, tokenSource.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError($"TriggerOnUsWebhook Failed ! - Id({item.Id}) Error : {exception}");
            }
        }
    }
}