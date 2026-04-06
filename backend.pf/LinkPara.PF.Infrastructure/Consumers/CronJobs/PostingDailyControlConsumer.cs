using LinkPara.HttpProviders.Notification;
using LinkPara.HttpProviders.Notification.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class PostingDailyControlConsumer : IConsumer<PostingDailyControl>
{
    private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;
    private readonly IBus _bus;
    private readonly IVaultClient _vaultClient;
    private readonly INotificationService _notificationService;

    public PostingDailyControlConsumer(
        IGenericRepository<PostingBatchStatus> postingBatchStatusRepository,
        IBus bus,
        IVaultClient vaultClient,
        INotificationService notificationService)
    {
        _postingBatchStatusRepository = postingBatchStatusRepository;
        _bus = bus;
        _vaultClient = vaultClient;
        _notificationService = notificationService;
    }

    async Task IConsumer<PostingDailyControl>.Consume(ConsumeContext<PostingDailyControl> context)
    {
        var batchStatus = await _postingBatchStatusRepository.GetAll()
            .Where(b => b.PostingBatchLevel == PostingBatchLevel.PosBlockageAccounting 
                && b.PostingDate == DateTime.Today)
            .FirstOrDefaultAsync();

        if (batchStatus is not null 
            && !batchStatus.IsCriticalError
            && batchStatus.BatchStatus == BatchStatus.Completed)
        {
            return;
        }

        var notificationConfig =
                    _vaultClient.GetSecretValue<PostingFailedNotification>("PFSecrets", "PostingFailedNotification");

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (notificationConfig.IsNotificationActiveForDevelopment)
        {
            await _notificationService.SendAdvancedEmailNotificationAsync(new AdvancedEmailRequest
            {
                EventName = "PF.PostingFailed",
                PreferredLanguage = "TR",
                ReceiverId = Guid.Empty,
                ToEmail = notificationConfig.Development.ToArray(),
                TemplateParameters = new ()
                {
                    {"Ortam", environment},
                    {"Hata", "Merhaba,\r\nPosting sırasında sistemsel bir hata oluştu. Kontrol edilmesi gerekmektedir."}
                }
            });
        }

        await SendToBusAsync(environment);
    }
    private async Task SendToBusAsync(string environment)
    {
        await _bus.Publish(new PostingFailed
        {
            Environment = environment,
            Exception = "Merhaba,\r\nPosting sırasında sistemsel bir hata oluştu. Kontrol edilmesi gerekmektedir."
        });
    }
}
