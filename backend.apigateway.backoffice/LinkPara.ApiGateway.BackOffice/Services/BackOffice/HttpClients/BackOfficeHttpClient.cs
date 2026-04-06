using LinkPara.ApiGateway.BackOffice.Services.Approval.Models;
using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.Approval.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using System.Text;

namespace LinkPara.ApiGateway.BackOffice.Services.BackOffice.HttpClients;

public class BackOfficeHttpClient : HttpClientBase, IBackOfficeHttpClient
{
    private readonly HttpClient _client;
    private readonly IHashGenerator _hashGenerator;
    private readonly ILogger<BackOfficeHttpClient> _logger;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IVaultClient _vaultClient;

    public BackOfficeHttpClient(HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IHashGenerator hashGenerator,
        ILogger<BackOfficeHttpClient> logger,
        IUserHttpClient userHttpClient,
        IVaultClient vaultClient) : base(client, httpContextAccessor)
    {
        _client = client;
        _hashGenerator = hashGenerator;
        _logger = logger;
        _userHttpClient = userHttpClient;
        _vaultClient = vaultClient;
    }

    public async Task<string> RecallApprovedRequestAsync(RequestDto request)
    {
        await ImpersonateMakerTokenAsync(request);

        if (!string.IsNullOrEmpty(request.QueryParameters))
        {
            request.Url = string.Concat(request.Url, request.QueryParameters);
        }

        switch (request.ActionType)
        {
            case ActionType.Post:
                return await PostApprovedRequestAsync(request);
            case ActionType.Put:
                return await PutApprovedRequest(request);
            case ActionType.Delete:
                return await DeleteApprovedRequest(request);
            case ActionType.Patch:
                return await PatchApprovedRequest(request);
            default:
                throw new InvalidOperationException();
        }
    }

    private async Task ImpersonateMakerTokenAsync(RequestDto request)
    {
        var tokenHashKey = await _vaultClient.GetSecretValueAsync<string>("SharedSecrets", "ApprovalConfiguration", "TokenHashKey");
        var userTokenSecureHash = _hashGenerator.Generate(request.MakerFullName + request.MakerUserId.ToString(), tokenHashKey);

        var makerToken = await _userHttpClient.GetUserTokenAsync(request.MakerUserId, userTokenSecureHash);

        if (_client.DefaultRequestHeaders.Contains("Authorization"))
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
        }

        var secureHashkey = await _vaultClient.GetSecretValueAsync<string>("SharedSecrets", "ApprovalConfiguration", "SecureHashKey");

        var secureHash = _hashGenerator.Generate(request.Url + request.Resource, secureHashkey);

        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {makerToken.Token}");
        _client.DefaultRequestHeaders.Add("HashFromBackOffice", secureHash);
    }

    private async Task<string> PatchApprovedRequest(RequestDto request)
    {
        try
        {
            var response = await PatchAsJsonAsync(request.Url, request.Body);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error On PatchApprovedRequest exception : {ex}");
            throw;
        }
    }

    private async Task<string> DeleteApprovedRequest(RequestDto request)
    {
        try
        {
            var response = await DeleteAsync(request.Url);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error On DeleteApprovedRequest exception : {ex}");
            throw;
        }
    }

    private async Task<string> PutApprovedRequest(RequestDto request)
    {
        try
        {
            var httpContent = new StringContent(request.Body, Encoding.UTF8, "application/json");
            var response = await PutAsync(request.Url, httpContent);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error On PutApprovedRequest exception : {ex}");
            throw;
        }
    }

    private async Task<string>PostApprovedRequestAsync(RequestDto request)
    {
        var headers = new Dictionary<string, string>
        {
            { "ApprovalRequestId", request.Id.ToString() }
        };

        try
        {
            var response = await PostAsync(request.Url, request.Body, headers);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error On PostApprovedRequestAsync exception : {ex}");
            throw;
        }
    }
}
