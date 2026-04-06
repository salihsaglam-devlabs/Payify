using Hangfire.Dashboard;

namespace LinkPara.Scheduler.API.Filters;

public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly WebApplication _app;

    public DashboardAuthorizationFilter(WebApplication app)
    {
        _app = app;
    }
    public bool Authorize(DashboardContext context)
    {
        // all environments allowed to open dashboard
        return true;
    }
}
