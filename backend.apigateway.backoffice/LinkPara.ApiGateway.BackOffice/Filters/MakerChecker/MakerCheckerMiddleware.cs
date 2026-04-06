using LinkPara.ApiGateway.BackOffice.Commons.Models.MakerCheckerModels;
using LinkPara.ApiGateway.BackOffice.Services.Approval.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Approval.Models;
using LinkPara.ApiGateway.BackOffice.Services.Approval.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.Approval.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.IO;
using System.Net;
using System.Security.Claims;

namespace LinkPara.ApiGateway.BackOffice.Filters.MakerChecker;

public class MakerCheckerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICaseContainer _caseContainer;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly IHashGenerator _hashGenerator;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MakerCheckerMiddleware> _logger;
    private readonly IVaultClient _vaultClient;

    public MakerCheckerMiddleware(RequestDelegate next,
        ICaseContainer caseContainer,
        IHashGenerator hashGenerator,
        IServiceProvider serviceProvider,
        ILogger<MakerCheckerMiddleware> logger,
        IVaultClient vaultClient)
    {
        _next = next;
        _caseContainer = caseContainer;
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        _hashGenerator = hashGenerator;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _vaultClient = vaultClient;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var authorization = context.Request.Headers["Authorization"];

            if (authorization.Count == 0)
            {
                await _next(context);
                return;
            }

            var contextEndpoint = context.GetEndpoint();

            if (contextEndpoint is null)
            {
                _logger.LogError($"Error MakerCheckerMiddleware contextEndpoint is null");
                await _next(context);
                return;
            }

            var controllerActionDescriptor = contextEndpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (controllerActionDescriptor is null)
            {
                _logger.LogError($"Error MakerCheckerMiddleware controllerActionDescriptor is null");
                await _next(context);
                return;
            }

            var caseCheckerRequest = await PrepareCaseCheckerRequestAsync(context, controllerActionDescriptor);

            bool isCallingFromApprovedOperation = await IsCallingFromApprovedOperationAsync(context, caseCheckerRequest);

            if (isCallingFromApprovedOperation)
            {
                await _next(context);
                return;
            }

            var approvalNeeded = await _caseContainer.IsApprovalNeededAsync(caseCheckerRequest);

            if (!approvalNeeded)
            {
                await _next(context);
                return;
            }

            var approvalRequest = await PrepareApprovalRequestAsync(context, caseCheckerRequest);

            var approvalHttpClient = (IApprovalHttpClient)_serviceProvider.GetRequiredService(typeof(IApprovalHttpClient));

            var response = await approvalHttpClient.SaveRequestAsync(approvalRequest);

            if (!response.IsSuccess)
            {
                _logger.LogError($"MakerCheckerMiddleware the request continues message:{response.Message}");
                await _next(context);
                return;
            }

            var saveApprovalResponse = new SaveApprovalResponse
            {
                ReferenceId = response.Request.ReferenceId,
                Description = "ThisRequestRequiresApproval"
            };

            context.Response.Clear();

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.WriteAsJsonAsync(saveApprovalResponse);
        }
        catch(ApiException apiException)
        {
            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            ProblemDetails problemDetails = new ProblemDetails
            {
                Detail = apiException.Message,
            };
            problemDetails.Extensions.Add("code", apiException.Code);
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
        catch(GenericException genericException)
        {
            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            ProblemDetails problemDetails = new ProblemDetails
            {
                Detail = genericException.Message,
            };
            problemDetails.Extensions.Add("code", genericException.Code);
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fatal MakerCheckerMiddleware Exception:\n{ex}");
            throw;
        }
    }

    private async Task<ApprovalRequest> PrepareApprovalRequestAsync(HttpContext context, CaseCheckerRequest caseCheckerRequest)
    {

        var body = await GetRequestBodyAsync(context);

        var queryParameters = context.Request.QueryString.Value;

        var actionType = GetMethod(caseCheckerRequest.Action);
        var @case = _caseContainer.GetCase(caseCheckerRequest);

        var contextUserId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(contextUserId, out Guid userId))
        {
            throw new InvalidCastException();
        }

        var _userHttpClient = (IUserHttpClient)_serviceProvider.GetRequiredService(typeof(IUserHttpClient));

        var makerRoles = await _userHttpClient.GetUserRolesAsync(userId);
        var makerRoleIds = makerRoles.Any()
            ? makerRoles.Select(s => s.Id).ToList()
            : new List<Guid>();

        var user = await _userHttpClient.GetUserByIdAsync(userId);

        return new ApprovalRequest
        {
            ActionType = actionType,
            Body = body,
            CaseId = @case.Id,
            DisplayName = @case.DisplayName,
            MakerRoleIds = makerRoleIds,
            QueryParameters = queryParameters,
            Url = caseCheckerRequest.Path,
            UserId = userId,
            Resource = caseCheckerRequest.ControllerName,
            MakerFullName = user.FullName
        };
    }

    private async Task<bool> IsCallingFromApprovedOperationAsync(HttpContext context, CaseCheckerRequest caseCheckerRequest)
    {
        var secureHashkey = await _vaultClient.GetSecretValueAsync<string>("SharedSecrets", "ApprovalConfiguration", "SecureHashKey");
        var hostHash = _hashGenerator.Generate(caseCheckerRequest.Path + caseCheckerRequest.ControllerName, secureHashkey);
        var clientHash = context.Request.Headers["HashFromBackOffice"].ToString();

        bool isCallingFromApprovedOperation = !string.IsNullOrEmpty(clientHash) && hostHash == clientHash;
        return isCallingFromApprovedOperation;
    }

    private async Task<string> GetRequestBodyAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        using var requestStream = _recyclableMemoryStreamManager.GetStream();
        await context.Request.Body.CopyToAsync(requestStream);
        context.Request.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Seek(0, SeekOrigin.Begin);
        return body;
    }

    private Task<CaseCheckerRequest> PrepareCaseCheckerRequestAsync(HttpContext context, ControllerActionDescriptor controllerActionDescriptor)
    {
        var path = context.Request.Path.Value;
        var action = context.Request.Method;
        var controllerName = controllerActionDescriptor.ControllerName;
        var actionName = controllerActionDescriptor.ActionName;

        return Task.FromResult(new CaseCheckerRequest
        {
            Action = action,
            ActionName = actionName,
            ControllerName = controllerName,
            Path = path
        });
    }



    private static ActionType GetMethod(string method)
    {
        return method.ToUpper() switch
        {
            "POST" => ActionType.Post,
            "PUT" => ActionType.Put,
            "DELETE" => ActionType.Delete,
            "PATCH" => ActionType.Patch,
            _ => throw new InvalidOperationException(),
        };
    }
}