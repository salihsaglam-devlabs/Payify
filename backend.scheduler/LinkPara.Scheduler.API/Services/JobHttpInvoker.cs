using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Enums;
using LinkPara.Scheduler.API.Commons.Interfaces;

namespace LinkPara.Scheduler.API.Services;

public class JobHttpInvoker : IJobHttpInvoker
{
    private readonly HttpClient _httpClient;

    public JobHttpInvoker(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task InvokeAsync(CronJob cronJob)
    {
        switch (cronJob.HttpType)
        {
            case HttpType.Get:
                await _httpClient.GetAsync(cronJob.Uri);
                break;
            case HttpType.Delete:
                await _httpClient.DeleteAsync(cronJob.Uri);
                break;
            case HttpType.Patch:
                await _httpClient.PatchAsync(cronJob.Uri, null);
                break;
            case HttpType.Post:
                await _httpClient.PostAsync(cronJob.Uri, null);
                break;
            case HttpType.Put:
                await _httpClient.PutAsync(cronJob.Uri, null);
                break;
            case HttpType.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}