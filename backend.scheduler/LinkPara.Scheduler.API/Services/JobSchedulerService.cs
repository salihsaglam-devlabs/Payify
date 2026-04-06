using Hangfire;
using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Enums;
using LinkPara.Scheduler.API.Commons.Interfaces;
using MassTransit;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Scheduler.API.Services;

public class JobSchedulerService : IJobScheduler
{
    private readonly IGenericRepository<CronJob> _repository;
    private readonly IJobHttpInvoker _httpInvoker;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobSchedulerService> _logger;

    public JobSchedulerService(IGenericRepository<CronJob> repository,
        IJobHttpInvoker httpInvoker,
        IServiceProvider serviceProvider,
        ILogger<JobSchedulerService> logger)
    {
        _repository = repository;
        _httpInvoker = httpInvoker;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ScheduleAsync()
    {
        var cronJobs = _repository.GetAll()
            .ToList();

        foreach (var cronJob in cronJobs)
        {
            try
            {
                if (cronJob.RecordStatus != RecordStatus.Active)
                {
                    RecurringJob.RemoveIfExists(cronJob.Name);
                }
                else
                {
                    if (cronJob.CronJobType == CronJobType.QueueMessage)
                    {
                        try
                        {
                            var triggerJob = await JobFactory(cronJob.Name, cronJob.Module);
                            
                            // add job to hangfire
                            RecurringJob.AddOrUpdate(cronJob.Name,
                                () => triggerJob.TriggerAsync(cronJob),
                                cronJob.CronExpression,
                                TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));
                        }
                        catch (Exception exception)
                        {
                            _logger.LogError("JobFactoryException JobName: {Name} - Error: {Exception}", cronJob.Name, exception);
                        }
                    }
                    else
                    {
                        try
                        {
                            // add job to hangfire
                            RecurringJob.AddOrUpdate(cronJob.Name,
                                () => _httpInvoker.InvokeAsync(cronJob),
                                cronJob.CronExpression,
                                 TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));
                        }
                        catch (Exception exception)
                        {
                            _logger.LogError("JobFactoryException JobName: {Name} - Error: {Exception}", cronJob.Name, exception);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogCritical("CronSchedulerError!: {Exception}", exception);
                throw;
            }
        }
    }

    private Task<IJobTrigger> JobFactory(string cronJobName, string application)
    {
        var type = Type.GetType($"LinkPara.Scheduler.API.Jobs.{application}.{cronJobName}");

        if (type is null)
        {
            throw new Exception($"Job {cronJobName} not found");
        }

        var instance = GetReflectedType<IJobTrigger>(type);

        return Task.FromResult(instance);
    }

    private T GetReflectedType<T>(Type typeToReflect)
        where T : class
    {
        var propertyTypeAssemblyQualifiedName = typeToReflect.AssemblyQualifiedName;
        var constructors = typeToReflect.GetConstructors();
        if (constructors.Length == 0)
        {
            return (T)Activator.CreateInstance(Type.GetType(propertyTypeAssemblyQualifiedName));
        }

        var parameters = constructors?.FirstOrDefault()?.GetParameters();
        if (parameters == null)
        {
            return (T)Activator.CreateInstance(Type.GetType(propertyTypeAssemblyQualifiedName));
        }

        var injectedParameters = parameters.Select(parameter => _serviceProvider.GetService(parameter.ParameterType))
            .ToArray();

        return (T)Activator.CreateInstance(Type.GetType(propertyTypeAssemblyQualifiedName), injectedParameters);
    }
}