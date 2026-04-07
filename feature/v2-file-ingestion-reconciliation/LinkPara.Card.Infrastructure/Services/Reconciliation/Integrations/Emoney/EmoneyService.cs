using System.Net.Http.Json;

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
}
