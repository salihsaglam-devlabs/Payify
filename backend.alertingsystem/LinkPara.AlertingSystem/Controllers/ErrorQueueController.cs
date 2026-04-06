using LinkPara.AlertingSystem.Commons.ErrorQueueMonitor;
using Microsoft.AspNetCore.Mvc;

namespace Linkpara.AlertingSystem.Controllers;

[ApiController]
[Route("[controller]")]
public class ErrorQueueController: ControllerBase
{
    private IQueueMonitoring _queueMonitoring;

    public ErrorQueueController(IQueueMonitoring queueMonitoring)
    {
        _queueMonitoring = queueMonitoring;
    }
    
    /// <summary>
    /// Checks error queues and sends mail if there are messages
    /// </summary>
    /// <param></param>
    /// sample request:
    [HttpPost("start-monitor")]
    public async Task StartQueueMonitor()
    { 
        await _queueMonitoring.GetQueueInfosAsync();
    }
}