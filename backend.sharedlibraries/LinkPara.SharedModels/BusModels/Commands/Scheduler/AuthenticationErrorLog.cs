using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.SharedModels.BusModels.Commands.Scheduler
{
    public class AuthenticationErrorLog
    {
        public Guid MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string TransactionType { get; set; }
        public string ClientIpAddress { get; set; }
    }
}
