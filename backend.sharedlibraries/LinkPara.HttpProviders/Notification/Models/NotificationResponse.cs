using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.HttpProviders.Notification.Models
{
    public class NotificationResponse
    {
        public bool Success { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }
}
