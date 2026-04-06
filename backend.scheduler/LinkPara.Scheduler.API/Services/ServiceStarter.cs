using Hangfire;
using LinkPara.Scheduler.API.Commons.Interfaces;

namespace LinkPara.Scheduler.API.Services;

public static class ServiceStarter
{
    public static async Task StartServicesAsync(IApplicationBuilder app)   
    {
        var scope = app.ApplicationServices.CreateScope();
        var jobScheduler = scope.ServiceProvider.GetService<IJobScheduler>();
        
        // Run immediately at startup
        if (jobScheduler != null) 
            await jobScheduler.ScheduleAsync();

        var everyFiveMinutes = "*/5 * * * *";
        
        // Then, schedule. It will perform sync operation with database cron jobs
        RecurringJob.AddOrUpdate<IJobScheduler>("JobScheduler",t => t.ScheduleAsync(),everyFiveMinutes, TimeZoneInfo.Local);
    }
}