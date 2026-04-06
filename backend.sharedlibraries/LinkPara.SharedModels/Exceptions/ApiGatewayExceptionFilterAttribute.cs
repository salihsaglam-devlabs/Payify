using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.SharedModels.Exceptions;

public class ApiGatewayExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<ApiGatewayExceptionFilterAttribute> _logger;
    private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;
    private readonly IStringLocalizer _localizer;

    public ApiGatewayExceptionFilterAttribute(IStringLocalizerFactory factory, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ApiGatewayExceptionFilterAttribute>();
        
        _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(ApiException), HandleApiException },
            { typeof(GenericException), HandleGenericException },
            { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
            { typeof(AuthorizationException), HandleHmacAuthorizationException },
            { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(InternalServiceException), HandleInternalServiceException },
            { typeof(CustomApiException), HandleCustomApiException }
        };
        
        _localizer = factory.Create("Exceptions", Assembly.GetEntryAssembly()?.FullName);
    }
    
    private void HandleCustomApiException(ExceptionContext context)
    {
        var exception = (CustomApiException)context.Exception;

        var details = new ProblemDetails()
        {
            Detail = exception.Message
        };

        details.Extensions.Add("code", exception.Code);

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    public override void OnException(ExceptionContext context)
    {
        HandleException(context);

        base.OnException(context);
    }
    
    private void HandleException(ExceptionContext context)
    {
        var type = context.Exception.GetType();
        if (_exceptionHandlers.ContainsKey(type))
        {
            _exceptionHandlers[type].Invoke(context);
            return;
        }

        var baseType = type.BaseType;

        if (baseType != null && _exceptionHandlers.ContainsKey(baseType))
        {
            _exceptionHandlers[baseType].Invoke(context);
            return;
        }

        if (!context.ModelState.IsValid)
        {
            HandleInvalidModelStateException(context);
            return;
        }

        HandleUnknownException(context);
    }
    
    private void HandleApiException(ExceptionContext context)
    {
        var exception = (ApiException)context.Exception;

        var details = new ProblemDetails()
        {
            Detail = exception.Message
        };
        
        details.Extensions.Add("code", exception.Code);

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
        
        _logger.LogError("ApiException : Code: {code} {message}",
            exception.Code, exception);
    }
    
    private void HandleGenericException(ExceptionContext context)
    {
        var exception = (GenericException)context.Exception;

        var details = new ProblemDetails()
        {
            Detail = _localizer.GetString(exception.GetType().Name)
        };
        
        details.Extensions.Add("code", exception.Code);

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
        
        _logger.LogError("GenericException (400) : Code: {code} {message}",
            exception.Code, exception);
    }
    
    private void HandleInternalServiceException(ExceptionContext context)
    {
        var exception = (InternalServiceException)context.Exception;

        var details = new ProblemDetails()
        {
            Detail = exception.Message
        };
        
        details.Extensions.Add("code", exception.Code);

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
        
        _logger.LogError("InternalServiceException (400) : Code: {code} {message}",
            exception.Code, exception);
    }

    private void HandleValidationException(ExceptionContext context)
    {
        var exception = (ValidationException)context.Exception;

        var details = new ValidationProblemDetails(exception.Errors)
        {
            Title = _localizer.GetString(exception.GetType().Name),
            Detail = _localizer.GetString(exception.GetType().Name)
        };
        
        details.Extensions.Add("code", ErrorCode.ValidationError);

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }
    
    private void HandleNotFoundException(ExceptionContext context)
    {
        var exception = (NotFoundException)context.Exception;

        var details = new ProblemDetails()
        {
            Title = "Resource NotFound",
            Detail = _localizer.GetString(exception.GetType().Name)
        };

        details.Extensions.Add("code", exception.Code);

        context.Result = new NotFoundObjectResult(details);
        context.ExceptionHandled = true;
        
        _logger.LogError("NotFoundObjectResult(400) : Code: {code} {message}",
            exception.Code, exception);
    }

    private void HandleInvalidModelStateException(ExceptionContext context)
    {
        var details = new ValidationProblemDetails(context.ModelState)
        {
            Title = "ModelInvalid",
            Detail = _localizer.GetString("ModelInvalid")
        };
        
        details.Extensions.Add("code", ErrorCode.InvalidParameters);

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
        
        _logger.LogError("InvalidModelStateException(400) : Code: {code} {message}",
            ErrorCode.InvalidParameters, context?.Exception);
    }
    
    private void HandleHmacAuthorizationException(ExceptionContext context)
    {
        var exception = (AuthorizationException)context.Exception;
        
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = _localizer.GetString(exception.GetType().Name),
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };

        context.ExceptionHandled = true;
        
        _logger.LogError("Status401Unauthorized : {message}", context?.Exception);
    }
    
    private void HandleUnauthorizedAccessException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = _localizer.GetString("UnauthorizedAccess"),
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };

        context.ExceptionHandled = true;
        
        _logger.LogError("Status401Unauthorized : {message}", context?.Exception);
    }
    
    private void HandleForbiddenAccessException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = _localizer.GetString("ForbiddenAccessException"),
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };

        context.ExceptionHandled = true;
        
        _logger.LogError("Status403Forbidden : {message}", context?.Exception);
    }
    
    private void HandleUnknownException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = _localizer.GetString("InternalServerError"),
            Detail = _localizer.GetString("InternalServerError")
        };
        
        details.Extensions.Add("code", ErrorCode.InternalError);

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.ExceptionHandled = true;
        
        _logger.LogError("Status500InternalServerError : {message}", context?.Exception);
    }
}