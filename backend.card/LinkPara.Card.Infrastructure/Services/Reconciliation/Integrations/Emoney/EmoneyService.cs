using System.Net.Http.Json;
using System.Text.Json;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;

internal sealed class EmoneyService : IEmoneyService
{
    private readonly HttpClient _httpClient;

    public EmoneyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<EmoneyCustomerTransactionDto>> GetByCustomerTransactionIdAsync(
        string customerTransactionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerTransactionId))
        {
            return [];
        }

        try
        {
            var httpResponse = await _httpClient.GetAsync(
                $"v1/Transactions/getByCustomerTransactionId/{Uri.EscapeDataString(customerTransactionId)}",
                cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new EmoneyIntegrationException(
                    $"Emoney transaction lookup failed with status code {(int)httpResponse.StatusCode} for customerTransactionId '{customerTransactionId}'.");
            }

            var customerTransactions = await httpResponse.Content.ReadFromJsonAsync<List<EmoneyCustomerTransactionDto>>(cancellationToken: cancellationToken);
            return customerTransactions ?? [];
        }
        catch (EmoneyIntegrationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new EmoneyIntegrationException(
                $"Emoney transaction lookup failed for customerTransactionId '{customerTransactionId}'.",
                ex);
        }
    }

    public async Task<EmoneyTransactionDto?> GetByTransactionIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        if (transactionId == Guid.Empty)
        {
            return null;
        }

        try
        {
            var httpResponse = await _httpClient.GetAsync($"v1/Transactions/{transactionId}", cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new EmoneyIntegrationException(
                    $"Emoney transaction lookup failed with status code {(int)httpResponse.StatusCode} for transactionId '{transactionId}'.");
            }

            return await httpResponse.Content.ReadFromJsonAsync<EmoneyTransactionDto>(cancellationToken: cancellationToken);
        }
        catch (EmoneyIntegrationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new EmoneyIntegrationException(
                $"Emoney transaction lookup failed for transactionId '{transactionId}'.",
                ex);
        }
    }

    public Task<EmoneyCommandResult> UpdateTransactionStatusAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        return CreateStubPostResultAsync("v1/Reconciliation/transactions/status", request, cancellationToken);
    }

    public Task<EmoneyCommandResult> ReverseBalanceEffectAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        return CreateStubPostResultAsync("v1/Reconciliation/transactions/reverse-balance-effect", request, cancellationToken);
    }

    public Task<EmoneyCommandResult> CorrectResponseCodeAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        return CreateStubPostResultAsync("v1/Reconciliation/transactions/correct-response-code", request, cancellationToken);
    }

    public Task<EmoneyCommandResult> ExpireTransactionAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        return CreateStubPostResultAsync("v1/Reconciliation/transactions/expire", request, cancellationToken);
    }

    public Task<EmoneyCommandResult> CreateTransactionAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        return CreateStubPostResultAsync("v1/Reconciliation/transactions/create", request, cancellationToken);
    }

    public Task<EmoneyCommandResult> RefundTransactionAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        return CreateStubPostResultAsync("v1/Reconciliation/transactions/refund", request, cancellationToken);
    }

    public Task<EmoneyCommandResult> InitChargebackAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        return CreateStubPostResultAsync("v1/Chargeback/init", request, cancellationToken);
    }

    public Task<EmoneyCommandResult> ApproveChargebackAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        return CreateStubPutResultAsync("v1/Chargeback/approve", request, cancellationToken);
    }

    public Task<EmoneyCommandResult> CreateShadowBalanceDebtCreditAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        return CreateStubPostResultAsync("v1/Reconciliation/shadow-balance/debt-credit", request, cancellationToken);
    }

    public Task<EmoneyCommandResult> RunShadowBalanceProcessAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        return CreateStubPostResultAsync("v1/Reconciliation/shadow-balance/run", request, cancellationToken);
    }

    private static Task<EmoneyCommandResult> CreateStubPostResultAsync(
        string relativeUrl,
        object request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        return Task.FromResult(CreateStubCommandResult("POST", relativeUrl, request));
    }

    private static Task<EmoneyCommandResult> CreateStubPutResultAsync(
        string relativeUrl,
        object request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        return Task.FromResult(CreateStubCommandResult("PUT", relativeUrl, request));
    }

    private static EmoneyCommandResult CreateStubCommandResult(string method, string relativeUrl, object request)
    {
        var responseBody = JsonSerializer.Serialize(new
        {
            isStub = true,
            method,
            relativeUrl,
            request,
            message = "External command was stubbed and accepted."
        });

        return EmoneyCommandResult.Success(responseBody);
    }
}
