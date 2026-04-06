
using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface ISmsSenderService
{
    Task SendSmsAsync(SendSms request);
}
