using LinkPara.ApiGateway.Merchant.Services.Notification.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.HttpProviders.Notification.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class MerchantNotificationController : ApiControllerBase
{
    private readonly IMerchantNotificationHttpClient _merchantNotificationHttpClient;
    private readonly INotificationHttpClient _notificationHttpClient;

    public MerchantNotificationController(
        IMerchantNotificationHttpClient merchantNotificationHttpClient,
        INotificationHttpClient notificationHttpClient)
    {
        _merchantNotificationHttpClient = merchantNotificationHttpClient;
        _notificationHttpClient = notificationHttpClient;
    }
    
    /// <summary>
    /// Sends merchant notification
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantNotification:Create")]
    [HttpPost("")]
    public async Task SendMerchantNotificationAsync(SendMerchantNotificationRequest request)
    {
        switch (request.NotificationType)
        {
            case MerchantNotificationType.Email:
                foreach (var to in request.To)
                {
                    await _notificationHttpClient.SendEmailNotificationAsync(new EmailRequest
                    {
                        TemplateName = "MerchantNotification",
                        ToEmail = to,
                        DynamicTemplateData = new Dictionary<string, string>
                        {
                            { "message", request.Message },
                            { "subject", request.Subject },
                            { "preheader", request.PreHeader }
                        }
                    });
                }
                break;
            case MerchantNotificationType.Sms:
                await _notificationHttpClient.SendSmsNotificationAsync(new SmsRequest
                {
                    TemplateName = "MerchantNotification",
                    To = request.To,
                    TemplateParameters = new Dictionary<string, string>
                    {
                        { "message", request.Message }
                    },
                    IsOtp = false
                });
                break;
            default:
                throw new InvalidOperationException();
        }
    }
    
    /// <summary>
    /// Returns merchant notification template.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantNotification:Read")]
    [HttpGet("")]
    public async Task<MerchantNotificationTemplateDto> GetMerchantNotificationTemplateAsync([FromQuery] GetMerchantNotificationTemplateRequest request)
    {
        return await _merchantNotificationHttpClient.GetMerchantNotificationTemplateAsync(request);
    }
}