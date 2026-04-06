using FluentValidation;
using LinkPara.Documents.Application.Commons.Behaviours;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LinkPara.Documents.Application;

public static class DependencyInjection
{
   public static IServiceCollection AddApplication(this IServiceCollection services)
   {
      services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
      services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
      services.AddMediatR(Assembly.GetExecutingAssembly());
      services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

      return services;
   }
}