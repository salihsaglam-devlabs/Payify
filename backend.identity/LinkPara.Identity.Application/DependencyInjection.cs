using System.Reflection;
using FluentValidation;
using LinkPara.Audit;
using LinkPara.Identity.Application.Common.Behaviours;
using LinkPara.Security;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddSingleton<IHashGenerator, HashGenerator>();
        services.AddSingleton<ISecureRandomGenerator, SecureRandomGenerator>();

        return services;
    }
}