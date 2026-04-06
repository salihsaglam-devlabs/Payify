using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using LinkPara.PF.Pos.ApiGateway.Filters;
using LinkPara.PF.Pos.ApiGateway.Services.HttpClients;

namespace LinkPara.PF.Pos.ApiGateway.Authentication.SignaturePolicy;

public class SignatureRequirementHandler  : AuthorizationHandler<SignatureRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMerchantDeviceHttpClient _merchantDeviceHttpClient;
    private readonly IEncryptionService _encryptionService;
    private readonly ISignatureValidator _signatureValidator;
    private readonly IBus _bus;
    private readonly ILogger<PfExceptionFilterAttribute> _logger;
    private readonly IVaultClient _vaultClient;

    public SignatureRequirementHandler(IHttpContextAccessor httpContextAccessor, 
        IMerchantDeviceHttpClient merchantDeviceHttpClient, 
        IEncryptionService encryptionService, 
        ISignatureValidator signatureValidator, IBus bus, ILogger<PfExceptionFilterAttribute> logger,
        IVaultClient vaultClient)
    {
        _httpContextAccessor = httpContextAccessor;
        _merchantDeviceHttpClient = merchantDeviceHttpClient;
        _encryptionService = encryptionService;
        _signatureValidator = signatureValidator;
        _bus = bus;
        _logger = logger;
        _vaultClient = vaultClient;
    }
    
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SignatureRequirement requirement)
    {
        var headers = _httpContextAccessor.HttpContext!.Request.Headers;

        if (!headers.ContainsKey("publicKey")
            || !headers.ContainsKey("nonce")
            || !headers.ContainsKey("signature")
            || !headers.ContainsKey("conversationId")
            || !headers.ContainsKey("clientIpAddress")
            || !headers.ContainsKey("serialNumber"))
        {
            context.Fail(new AuthorizationFailureReason(this, "InvalidHeader"));
            return;
        }

        var validationParameters = new SignatureValidationParameters
        {
            PublicKey = headers["publicKey"],
            Nonce = headers["nonce"],
            ConversationId = headers["conversationId"], 
            Signature = headers["signature"]   
        };
        var apiKeys = await _merchantDeviceHttpClient.GetDeviceApiKeysAsync(validationParameters.PublicKey);

        if (apiKeys.SerialNumber != headers["serialNumber"])
        {
            var logModel = new AuthenticationErrorLog();
            logModel.PublicKey = headers["publicKey"];
            logModel.MerchantId = apiKeys.MerchantId;
            await SendAuthenticationErrorLogAsync(logModel);
            
            context.Fail(new AuthorizationFailureReason(this, "InvalidSerialNumber"));
            return;
        }

        var keyConstant = _vaultClient.GetSecretValue<string>("SharedSecrets", "SignatureKeyConstant", "PfApiKeyEncryptionKey");
        validationParameters.PrivateKey = 
            _encryptionService.Decrypt(apiKeys.PrivateKeyEncrypted, keyConstant);
        
        var validationResponse = await _signatureValidator.ValidateAsync(validationParameters);

        if (!validationResponse.IsSucceed)
        {
            var logModel = new AuthenticationErrorLog();
            logModel.PublicKey = headers["publicKey"];
            logModel.MerchantId = apiKeys.MerchantId;

            await SendAuthenticationErrorLogAsync(logModel);
            context.Fail(new AuthorizationFailureReason(this, validationResponse.ErrorMessage));
            return;
        }

        context.Succeed(requirement);
    }
    private async Task SendAuthenticationErrorLogAsync(AuthenticationErrorLog logModel)
    {
        try
        {
            logModel.TransactionType = _httpContextAccessor.HttpContext!.Request.Path;
            logModel.ClientIpAddress = _httpContextAccessor.HttpContext!.Connection.RemoteIpAddress?.ToString();

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var busEndpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.SignatureAuthentication"));
            await busEndpoint.Send(logModel, cancellationToken.Token);
            cancellationToken.Dispose();
        }
        catch (Exception exception)
        {
            _logger.LogError("UnauthorizedAccessException(401) : {message}", exception.Message);
        }
    }
}