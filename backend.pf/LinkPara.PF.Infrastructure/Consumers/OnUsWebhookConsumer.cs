using System.Net.Http.Json;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments;
using LinkPara.PF.Application.Features.Payments.Commands.Inquire;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class OnUsWebhookConsumer : IConsumer<OnUsWebhook>
{
    private readonly IInquireService _inquireService;
    private readonly IGenericRepository<OnUsPayment> _onUsPaymentRepository;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<OnUsWebhookConsumer> _logger;
    private readonly HttpClient _client;
    private readonly IParameterService _parameterService;
    private const int MaxTriggerCount = 3;

    public OnUsWebhookConsumer(
        IInquireService inquireService,
        IGenericRepository<OnUsPayment> onUsPaymentRepository,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        ILogger<OnUsWebhookConsumer> logger,
        IAuditLogService auditLogService,
        HttpClient client,
        IParameterService parameterService)
    {
        _inquireService = inquireService;
        _onUsPaymentRepository = onUsPaymentRepository;
        _merchantTransactionRepository = merchantTransactionRepository;
        _logger = logger;
        _auditLogService = auditLogService;
        _client = client;
        _parameterService = parameterService;
    }

    public async Task Consume(ConsumeContext<OnUsWebhook> context)
    {
        var onUsPayment = await _onUsPaymentRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == context.Message.OnUsId);
        try
        {
            var merchantTransaction = await _merchantTransactionRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == onUsPayment.MerchantTransactionId);

            var onUsResult = await _inquireService.InquireAsync(new InquireCommand
            {
                PaymentConversationId = merchantTransaction.ConversationId,
                OrderId = merchantTransaction.OrderId,
                MerchantId = merchantTransaction.MerchantId,
                ConversationId = onUsPayment.WebhookRetryCount.ToString(),
                LanguageCode = merchantTransaction.LanguageCode
            });

            try
            {
                _client.BaseAddress = new Uri(onUsPayment.CallbackUrl);
                var response = await _client.PostAsJsonAsync("", onUsResult);
                if (response.IsSuccessStatusCode)
                {
                    await MarkAsCompletedAsync(onUsPayment);
                }
                else
                {
                    await MarkAsFailedAsync(onUsPayment);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"OnUsWebhookConsumer post webhook data failed : {exception}");
                await MarkAsFailedAsync(onUsPayment);
                return;
            }

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "OnUsWebhookConsumer",
                    SourceApplication = "PF",
                    Resource = "OnUsPayment",
                    UserId = Guid.Empty,
                    Details = new Dictionary<string, string>
                    {
                        { "Id", context.Message.OnUsId.ToString() },
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"OnUsWebhookConsumer Error : {exception}");
            await MarkAsFailedAsync(onUsPayment);
        }
    }

    private async Task MarkAsCompletedAsync(OnUsPayment onUsPayment)
    {
        onUsPayment.WebhookStatus = WebhookStatus.Completed;
        onUsPayment.WebhookRetryCount+=1;
        
        await _onUsPaymentRepository.UpdateAsync(onUsPayment);
    }

    private async Task MarkAsFailedAsync(OnUsPayment onUsPayment)
    {
        int maxTriggerCount;
        try
        {
            var maxTriggerCountParameter = await _parameterService.GetParameterAsync("PFPaymentParameters", "MaxWebhookTriggerCount");
            maxTriggerCount = Convert.ToInt32(maxTriggerCountParameter.ParameterValue);
        }
        catch (Exception e)
        {
            _logger.LogError($"OnUsWebhookConsumer MaxWebhookTriggerCount parameter not found. Proceeding with default value {MaxTriggerCount}");
            maxTriggerCount = MaxTriggerCount;
        }
        
        onUsPayment.WebhookRetryCount+=1;
        
        if (onUsPayment.WebhookRetryCount >= maxTriggerCount)
        {
            onUsPayment.WebhookStatus = WebhookStatus.Failed;
        }

        await _onUsPaymentRepository.UpdateAsync(onUsPayment);
    }
}