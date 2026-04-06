using System.Net.Http.Json;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.HostedPayment;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class HppWebhookNotificationConsumer : IConsumer<HppWebhookNotification>
{
    private readonly IHostedPaymentService _hostedPaymentService;
    private readonly IGenericRepository<HostedPayment> _hostedPaymentRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<HppWebhookNotificationConsumer> _logger;
    private readonly HttpClient _client;
    private readonly IParameterService _parameterService;

    public HppWebhookNotificationConsumer(
        IHostedPaymentService hostedPaymentService,
        IGenericRepository<HostedPayment> hostedPaymentRepository,
        ILogger<HppWebhookNotificationConsumer> logger,
        IAuditLogService auditLogService,
        HttpClient client,
        IParameterService parameterService)
    {
        _hostedPaymentService = hostedPaymentService;
        _hostedPaymentRepository = hostedPaymentRepository;
        _logger = logger;
        _auditLogService = auditLogService;
        _client = client;
        _parameterService = parameterService;
    }

    public async Task Consume(ConsumeContext<HppWebhookNotification> context)
    {
        try
        {
            var hpp = await _hostedPaymentRepository.GetAll()
                .FirstOrDefaultAsync(s => s.TrackingId == context.Message.TrackingId);

            var hppTransaction =
                await _hostedPaymentService.GetHppTransactionAsync(context.Message.TrackingId,
                    context.Message.MerchantId);

            try
            {
                _client.BaseAddress = new Uri(hppTransaction.CallbackUrl);
                var response = await _client.PostAsJsonAsync("", hppTransaction);
                if (response.IsSuccessStatusCode)
                {
                    await MarkAsCompletedAsync(hpp);
                }
                else
                {
                    await MarkAsFailedAsync(hpp);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"HppWebhookNotificationConsumer post webhook data failed : {exception}");
                await MarkAsFailedAsync(hpp);
            }

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "HppWebhookNotificationConsumer",
                    SourceApplication = "PF",
                    Resource = "HostedPayment",
                    UserId = Guid.Empty,
                    Details = new Dictionary<string, string>
                    {
                        { "TrackingId", context.Message.TrackingId },
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"HppWebhookNotificationConsumer Error : {exception}");
        }
    }

    private async Task MarkAsCompletedAsync(HostedPayment hpp)
    {
        hpp.WebhookStatus = WebhookStatus.Completed;
        hpp.WebhookRetryCount++;
        
        await _hostedPaymentRepository.UpdateAsync(hpp);
    }

    private async Task MarkAsFailedAsync(HostedPayment hpp)
    {
        var maxTriggerCount = await _parameterService.GetParameterAsync("PFPaymentParameters", "MaxWebhookTriggerCount");
        hpp.WebhookRetryCount++;
        
        if (hpp.WebhookRetryCount >= Convert.ToInt32(maxTriggerCount.ParameterValue))
        {
            hpp.WebhookStatus = WebhookStatus.Failed;
        }

        await _hostedPaymentRepository.UpdateAsync(hpp);
    }
}