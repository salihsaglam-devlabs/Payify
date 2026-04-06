using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Emoney.Infrastructure.Services
{
    public class PushNotificationSenderService : IPushNotificationSender
    {
        private readonly IBus _bus;
        public PushNotificationSenderService(IBus bus)
        {
            _bus = bus;
        }

        public async Task SendPushNotificationAsync(SendPushNotification request)
        {
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Notification.SendPushNotification"));
            await endpoint.Send(request);
        }
    }
}
