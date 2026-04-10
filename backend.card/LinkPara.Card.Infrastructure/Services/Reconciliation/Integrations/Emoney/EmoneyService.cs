using System.Net.Http.Json;
using System.Text.Json;
using LinkPara.Card.Application.Commons.Extensions;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;

internal sealed class EmoneyService : IEmoneyService
{
    private readonly HttpClient _httpClient;
    private readonly IStringLocalizer _localizer;

    public EmoneyService(HttpClient httpClient, Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _httpClient = httpClient;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
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
                    _localizer.Get("Reconciliation.EmoneyLookupStatusFailed", (int)httpResponse.StatusCode, customerTransactionId));
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
                _localizer.Get("Reconciliation.EmoneyLookupForCustomerFailed", customerTransactionId),
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
                    _localizer.Get("Reconciliation.EmoneyLookupStatusFailedForTransaction", (int)httpResponse.StatusCode, transactionId));
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
                _localizer.Get("Reconciliation.EmoneyLookupForTransactionFailed", transactionId),
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

    private Task<EmoneyCommandResult> CreateStubPostResultAsync(
        string relativeUrl,
        object request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        return Task.FromResult(CreateStubCommandResult("POST", relativeUrl, request));
    }

    private Task<EmoneyCommandResult> CreateStubPutResultAsync(
        string relativeUrl,
        object request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        return Task.FromResult(CreateStubCommandResult("PUT", relativeUrl, request));
    }

    private EmoneyCommandResult CreateStubCommandResult(string method, string relativeUrl, object request)
    {
        var responseBody = JsonSerializer.Serialize(new
        {
            isStub = true,
            method,
            relativeUrl,
            request,
            message = _localizer.Get("Reconciliation.ExternalCommandStubbed")
        });

        return EmoneyCommandResult.Success(responseBody);
    }
}
