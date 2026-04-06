using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.Audit.Models;

namespace LinkPara.Audit
{
    public interface IUserActivityLogService
    {
        public Task UserActivityLogAsync(UserActivityLog userActivityLog);
    }
}