using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Audit.Models
{
    public class UserActivityLog
    {
        public DateTime LogDate { get; set; }
        public string SourceApplication { get; set; }
        public string Resource { get; set; }
        public string Operation { get; set; }
        public string UserName { get; set; }
        public Guid ViewerId { get; set; }
        public Guid? ViewedId { get; set; }
        public string ClientIpAddress { get; set; }
        public string Channel { get; set; }
        public string CorrelationId { get; set; }
    }
}