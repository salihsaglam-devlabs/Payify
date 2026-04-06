using LinkPara.Emoney.ApiGateway.Filters;
using LinkPara.Emoney.ApiGateway.Models.Responses;
using LinkPara.Emoney.ApiGateway.Services.HttpClients;
using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;

namespace LinkPara.Emoney.ApiGateway.Authentication.PrivateKey;

public class PrivateKeyRequirementHandler : AuthorizationHandler<PrivateKeyRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApiKeyHttpClient _provisionHttpClient;
    private readonly IVaultClient _vaultClient;
    private readonly IPrivateKeyValidator _privateKeyValidator;
    private readonly IBus _bus;
    private readonly ILogger<EmoneyExceptionFilterAttribute> _logger;
    private readonly IEncryptionService _encryptionService;

    public PrivateKeyRequirementHandler(IHttpContextAccessor httpContextAccessor,
        IApiKeyHttpClient provisionHttpClient,
        IVaultClient vaultClient,
        IPrivateKeyValidator privateKeyValidator,
        IBus bus,
        ILogger<EmoneyExceptionFilterAttribute> logger,
        IEncryptionService encryptionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _provisionHttpClient = provisionHttpClient;
        _vaultClient = vaultClient;
        _privateKeyValidator = privateKeyValidator;
        _bus = bus;
        _logger = logger;
        _encryptionService = encryptionService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PrivateKeyRequirement requirement)
    {
        var headers = _httpContextAccessor.HttpContext!.Request.Headers;

        if (!headers.ContainsKey("publicKey")
            || !headers.ContainsKey("nonce")
            || !headers.ContainsKey("signature")
            || !headers.ContainsKey("clientIpAddress"))
        {
            context.Fail(new AuthorizationFailureReason(this, "InvalidHeader"));
            return;
        }

        var validationParameters = new PrivateKeyValidationParameters
        {
            PublicKey = headers["publicKey"],
            Nonce = headers["nonce"],
            Signature = headers["signature"]
        };
        var apiKey = await _provisionHttpClient.GetApiKeyAsync(validationParameters.PublicKey);

        var keyConstant = _vaultClient.GetSecretValue<string>("SharedSecrets", "SignatureKeyConstant", "EmoneyApiKeyEncryptionKey");

        validationParameters.PrivateKey =
            _encryptionService.Decrypt(apiKey.PrivateKey, keyConstant);

        PrivateKeyValidationResponse validationResponse = await _privateKeyValidator.ValidateAsync(validationParameters);

        if (!validationResponse.IsSucceed)
        {
            _logger.LogError("UnauthorizedAccessException(401) : {publickey}, {validationMessage}", validationResponse.ErrorMessage);
            context.Fail(new AuthorizationFailureReason(this, validationResponse.ErrorMessage));
            return;
        }

        headers.TryAdd("partnerId", new StringValues(apiKey.PartnerId.ToString()));

        context.Succeed(requirement);
    }
}
