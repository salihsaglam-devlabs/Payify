using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Card.Application.Commons.Helpers;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.ContextProvider;
using LinkPara.Card.Application.Commons.Attributes;
using MediatR;
using System.Reflection;

namespace LinkPara.Card.Application.Commons.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
{
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly ITimeProvider _timeProvider;

    public LoggingBehaviour(IAuditLogService auditLogService, IContextProvider contextProvider, ITimeProvider timeProvider)
    {
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _timeProvider = timeProvider;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!request.ToString().Contains("Command"))
        {
            return await next();
        }

        try
        {
            var response = await next();
            try
            {
                var auditLog = CreateAuditLogFromRequest(request);
                var auditProperties = GetAuditableProperties(request);

                if (!string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId))
                {
                    auditLog.UserId = Guid.Parse(_contextProvider.CurrentContext.UserId);
                }

                auditLog.IsSuccess = true;
                auditLog.Details = auditProperties;

                await _auditLogService.AuditLogAsync(auditLog);
            }
            catch { }
            return response;
        }
        catch (Exception ex)
        {
            try
            {
                var auditLog = CreateAuditLogFromRequest(request);
                auditLog.IsSuccess = false;
                auditLog.Details = new Dictionary<string, string>
                {
                     {"ErrorMessage", ExceptionDetailHelper.BuildDetailMessage(ex, 2000) }
                };
                await _auditLogService.AuditLogAsync(auditLog);
            }
            catch { }
            throw;
        }
    }

    private Dictionary<string, string> GetAuditableProperties(TRequest request)
    {
        return request.GetType().GetProperties()
            .Select(prop => new
            {
                PropertyInfo = prop,
                AuditAttribute = prop.GetCustomAttribute<AuditAttribute>()
            })
            .Where(x => x.AuditAttribute != null)
            .ToDictionary(x => x.PropertyInfo.Name, x => x.PropertyInfo.GetValue(request)?.ToString());
    }

    private AuditLog CreateAuditLogFromRequest(TRequest request)
    {
        var auditLog = new AuditLog
        {
            LogDate = _timeProvider.Now,
            CorrelationId = _contextProvider.CurrentContext.CorrelationId
        };

        Type requestType = request.GetType();

        auditLog.Operation = requestType.Name.Replace("Command", "");

        var namespaceParts = requestType.Namespace?
            .Replace("LinkPara.", "")
            .Replace("Application.", "")
            .Replace("Features.", "")
            .Split(".");

        if (namespaceParts != null && namespaceParts?.Length >= 2)
        {
            auditLog.SourceApplication = namespaceParts[0];
            auditLog.Resource = namespaceParts[1];
        }
        return auditLog;
    }
}