using System.Net.Http.Headers;
using System.Text;
using LinkPara.AlertingSystem.Commons;
using LinkPara.AlertingSystem.Commons.ErrorQueueMonitor;
using LinkPara.AlertingSystem.Helper;
using LinkPara.AlertingSystem.Services;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using Newtonsoft.Json;

namespace LinkPara.AlertingSystem.Jobs.ErrorQueueMonitor;

public class QueueMonitoring : IQueueMonitoring
{
    private readonly IVaultClient _vault;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    private const int QueueFilterTimeLengthInSeconds = 7200;
    private const int QueueFilterTimePart = 1800;
    private const int MessagesReadyCountLimit = 10;
    public QueueMonitoring(IVaultClient vaultClient, IEmailService emailService, IConfiguration configuration)
    {
        _vault = vaultClient;
        _emailService = emailService;
        _configuration = configuration;
    }

    private async Task GetQueueReadyMessagesInfoAsync()
    {
        var queueDefinitions = await GetQueueInformations();

        if (queueDefinitions != null)
        {
            var waitingQueues = queueDefinitions
                .Where(q =>
                    !q.name.Contains("_error")
                    && q.messages_ready > MessagesReadyCountLimit)
                .ToList();
            
            if (waitingQueues.Count > 0)
            {
                var emailInfo = _configuration.GetSection("ReadyQueueEmail").Get<EmailInfo>();

                if (emailInfo != null)
                {
                    var tenant = Environment.GetEnvironmentVariable("Tenant");
                    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    var title = $"Payify Alert ({tenant} - {environment}) {emailInfo.Subject}";

                    emailInfo.Subject = title;
                    
                    await SendMail(waitingQueues, emailInfo, CreateMessageReadyQueueMailBody(waitingQueues));
                }
            }
        }
    }

    public async Task GetErrorQueueInfoAsync()
    {
        var queueDefinitions = await GetQueueInformations();

        if (queueDefinitions != null)
        {
            var errorQueues = queueDefinitions
                .Where(q => 
                    q.name.Contains("_error")
                    && q.messages > 0)
                .ToList();
            
            if (errorQueues.Count > 0)
            {
                var emailInfo = _configuration.GetSection("ErrorQueueEmail").Get<EmailInfo>();
                
                if (emailInfo != null)
                {
                    var tenant = Environment.GetEnvironmentVariable("Tenant");
                    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    var title = $"Payify Alert ({tenant} - {environment}) {emailInfo.Subject}";

                    emailInfo.Subject = title;
                    
                    await SendMail(errorQueues, emailInfo, CreateErrorQueueMailBody(errorQueues));
                }
            }
        }
    }
    
    public async Task GetQueueInfosAsync()
    {
        await GetQueueReadyMessagesInfoAsync();
        await GetErrorQueueInfoAsync();
    }

    private async Task<List<QueueInformation>?> GetQueueInformations()
    {
        var httpClient = CreateCustomHttpClient();
        var response =
            await httpClient.GetAsync(
                $"api/queues?lengths_age={QueueFilterTimeLengthInSeconds}&lengths_incr={QueueFilterTimePart}");

        var body = await response.Content.ReadAsStringAsync();

        var queueDefinitions = JsonConvert.DeserializeObject<List<QueueInformation>>(body);
        return queueDefinitions;
    }

    private async Task SendMail(List<QueueInformation> errorQueues, EmailInfo emailInfo, string mailBody)
    {
        
        if (emailInfo is null)
        {
            return;
        }
        
        var templateData = new Dictionary<string, string>
        {
            { "subject", emailInfo.Subject },
            { "content", mailBody }
        };
        
        foreach (var email in emailInfo.ToEmailAddresses)
        {
            await _emailService.SendEmailAsync(new SendEmail
            {
                ToEmail = email, 
                TemplateName = string.Empty,
                DynamicTemplateData = templateData,
                AttachmentList = null,
                Attachment = null
            });
        }
    }

    private string CreateErrorQueueMailBody(List<QueueInformation> queues)
    {
        var str = new StringBuilder();
        var tzi = TimeZoneInfo.Local.BaseUtcOffset.Hours;
        
        foreach (var queue in queues)
        {
            var orderedSamples = queue.messages_details.samples.OrderByDescending(x => x.timestamp).ToList();

            str.AppendLine(
                $"<p align=\"left\" style=\"color:Blue;\"><b>Queue name: {queue.name}</b><p> <p align=\"left\">Total messages: {queue.messages} <p>");
            
            long errorSum = 0;
            var i = 0;
            var isFinished = false;
            
            while (!isFinished)
            {
                var numberOfMessages = orderedSamples[i].sample - orderedSamples[i + 1].sample;
                if (numberOfMessages > 0)
                {
                    var endTime = TimeHelper.ConvertTimeStampToDateTime(orderedSamples[i].timestamp).AddHours(tzi)
                        .TimeOfDay.ToString(@"hh\:mm");
                    var beginTime = TimeHelper.ConvertTimeStampToDateTime(orderedSamples[i + 1].timestamp).AddHours(tzi)
                        .TimeOfDay.ToString(@"hh\:mm");
                    str.AppendLine(
                        $"<p align=\"left\" style=\"color:Red;\">Between {beginTime} - {endTime} added messages: {numberOfMessages} <p>");
                    errorSum += numberOfMessages;
                }
                
                i++;

                if (i + 1 == orderedSamples.Count)
                {
                    i = 0;
                    isFinished = true;
                }
            }
            str.AppendLine($"<p align=\"left\">Older messages: {queue.messages - errorSum}  <p>");
        }

        return str.ToString();
    }
    
    private string CreateMessageReadyQueueMailBody(List<QueueInformation> queues)
    {
        var str = new StringBuilder();
        
        foreach (var queue in queues)
        {
            str.AppendLine(
                $"<p align=\"left\" style=\"color:Blue;\"><b>Queue name: {queue.name}</b><p>");
            
            str.AppendLine($"<p align=\"left\">Total ready messages: {queue.messages_ready}  <p>");
        }

        return str.ToString();
    }


    private HttpClient CreateCustomHttpClient()
    {
        var settings = _vault.GetSecretValue<EventBusSettings>("SharedSecrets", "EventBusSettings");
        var user = settings.Username;
        var pass = settings.Password;
        var authenticationString = $"{user}:{pass}";
        var base64Auth = Convert.ToBase64String(
            Encoding.ASCII.GetBytes(authenticationString));
        
        var address = string.IsNullOrEmpty(settings.Port) 
            ? $"http://{settings.Host}/"
            : $"http://{settings.Host}:{settings.Port}/"; 
        
        var httpClient = new HttpClient()
        {
            BaseAddress = new Uri(address),
            DefaultRequestHeaders =
            {
                Authorization = new AuthenticationHeaderValue("Basic", base64Auth),
                Accept = { new MediaTypeWithQualityHeaderValue("application/json") }
            }
        };
        return httpClient;
    }
}