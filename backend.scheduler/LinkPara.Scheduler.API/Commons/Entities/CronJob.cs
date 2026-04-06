using LinkPara.Scheduler.API.Commons.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Scheduler.API.Commons.Entities;

public class CronJob : AuditEntity
{
    public string Name { get; set; }
    public string CronExpression { get; set; }
    public string Description { get; set; }
    public string Module { get; set; }
    public CronJobType CronJobType { get; set; }
    public HttpType HttpType { get; set; }
    public string Uri { get; set; }
}