using LinkPara.SharedModels.BusModels.Commands.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Emoney.Application.Commons.Interfaces
{
    public interface IPushNotificationSender
    {
        Task SendPushNotificationAsync(SendPushNotification request);

    }
}
