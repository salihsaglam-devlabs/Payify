using LinkPara.Scheduler.API.Commons.Enums;

namespace LinkPara.Scheduler.API.Commons.Models;

public class CreateCronJobRequest
{
    public string Name { get; set; }
    public string CronExpression { get; set; }
    public string Description { get; set; }
    public string Module { get; set; }
    public CronJobType CronJobType { get; set; }
    public HttpType HttpType { get; set; }
    public string Uri { get; set; }
}