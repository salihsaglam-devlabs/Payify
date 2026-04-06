using LinkPara.HealthCheck.Models;
using LinkPara.HealthCheck.Services;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text;

namespace LinkPara.HealthCheck.Controllers;

[Route("[controller]")]
[ApiController]
public class WebhooksController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ISecurityService _securityService;
    private readonly IConfiguration _configuration;
    private readonly EmailConfiguration emailConfiguration;

    public WebhooksController(IEmailService emailService,
        IConfiguration configuration,
        ISecurityService securityService)
    {
        _emailService = emailService;
        _configuration = configuration;
        _securityService = securityService;

        emailConfiguration = configuration.GetSection("EmailConfiguration")
            .Get<EmailConfiguration>();
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task SendMail([FromBody] WebhookMessage message)
    {
        var allowed = _securityService.IpIsAllowed(emailConfiguration.AllowedIpAddresses);
        if (allowed)
        {
            var str = new StringBuilder();

            if (string.IsNullOrEmpty(message.Description))
            {
                str.AppendLine($"Message : {message.Message}</br>");
            }
            else
            {
                str.AppendLine($"Report For : {message.Service}</br>")
                .AppendLine($"Message : {message.Message}</br>")
                .AppendLine($"Description : {message.Description}</br>");
            }

            foreach (var mail in emailConfiguration.ToEmailAddresses)
            {
                await _emailService.SendMailAsync(new SendEmail
                {
                    ToEmail = mail.Address,
                    TemplateName = "DefaultTemplate",
                    DynamicTemplateData = new()
                      {
                         { "subject", "HealthCheck" },
                         { "content", str.ToString() }
                      }
                });
            }
        }
    }
}
