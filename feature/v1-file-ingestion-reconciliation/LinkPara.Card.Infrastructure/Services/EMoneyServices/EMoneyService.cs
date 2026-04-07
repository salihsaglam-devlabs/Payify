using System.Net.Http.Json;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Infrastructure.Services.EMoneyServices;

public class EMoneyService : IEMoneyService
{
    private readonly HttpClient _httpClient;

    public EMoneyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<EMoneyTransactionLookupResult> GetByCustomerTransactionIdAsync(
        string customerTransactionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerTransactionId))
        {
            return new EMoneyTransactionLookupResult
            {
                Status = EMoneyTransactionLookupStatus.NotFound,
                ExternalTransactionId = string.Empty,
                TransactionState = EMoneyLookupStates.NotFound
            };
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"v1/Transactions/getByCustomerTransactionId/{Uri.EscapeDataString(customerTransactionId)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"eMoney transaction lookup failed. StatusCode={(int)response.StatusCode}");
            }

            var transactions = await response.Content.ReadFromJsonAsync<List<EMoneyTransactionItem>>(cancellationToken: cancellationToken)
                               ?? new List<EMoneyTransactionItem>();

            var entity = transactions
                .OrderByDescending(x => x.TransactionDate)
                .ThenByDescending(x => x.CreateDate)
                .FirstOrDefault();

            if (entity is null)
            {
                return new EMoneyTransactionLookupResult
                {
                    Status = EMoneyTransactionLookupStatus.NotFound,
                    ExternalTransactionId = string.Empty,
                    TransactionState = EMoneyLookupStates.NotFound
                };
            }

            var state = entity.IsCancelled
                ? "CANCELLED"
                : ResolveState(entity.TransactionStatus);

            return new EMoneyTransactionLookupResult
            {
                Status = EMoneyTransactionLookupStatus.Found,
                ExternalTransactionId = entity.Id.ToString(),
                TransactionState = state,
                Amount = entity.Amount,
                CurrencyCode = entity.CurrencyCode,
                IsCancelled = entity.IsCancelled,
                IsSettlementReceived = entity.IsSettlementReceived,
                TransactionDate = entity.TransactionDate
            };
        }
        catch
        {
            return new EMoneyTransactionLookupResult
            {
                Status = EMoneyTransactionLookupStatus.Unavailable,
                ExternalTransactionId = string.Empty,
                TransactionState = EMoneyLookupStates.Unavailable
            };
        }
    }

    public async Task<bool> SetExpireStatusByCustomerTransactionIdAsync(
        string customerTransactionId,
        string operationIdempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerTransactionId))
        {
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "v1/Transactions/setExpireStatusByCustomerTransactionId",
                new
                {
                    CustomerTransactionId = customerTransactionId,
                    OperationIdempotencyKey = operationIdempotencyKey
                },
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CreateTransactionForCardReconciliationAsync(
        string customerTransactionId,
        string referenceCustomerTransactionId,
        decimal? amount,
        string currencyCode,
        string operationIdempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerTransactionId))
        {
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "v1/Transactions/createForCardReconciliation",
                new
                {
                    CustomerTransactionId = customerTransactionId,
                    ReferenceCustomerTransactionId = referenceCustomerTransactionId,
                    Amount = amount,
                    CurrencyCode = currencyCode,
                    OperationIdempotencyKey = operationIdempotencyKey
                },
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string ResolveState(string transactionStatus)
    {
        if (string.IsNullOrWhiteSpace(transactionStatus))
        {
            return "UNKNOWN";
        }

        return transactionStatus.Trim().ToUpperInvariant();
    }

    private sealed class EMoneyTransactionItem
    {
        public Guid Id { get; set; }
        public string TransactionStatus { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsSettlementReceived { get; set; }
    }
}
