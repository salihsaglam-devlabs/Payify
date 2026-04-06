using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Requests;

/// <summary>
/// Represents a request to get account transactions.
/// </summary>
public class GetAccountTransactionsRequest
{
    /// <summary>
    /// Gets or sets the account reference.
    /// </summary>
    public string AccountRef { get; set; }

    /// <summary>
    /// Gets or sets the start time of the transaction.
    /// </summary>
    public string TransactionStartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the transaction.
    /// </summary>
    public string TransactionEndTime { get; set; }

    /// <summary>
    /// Gets or sets the minimum transaction amount.
    /// </summary>
    public string MinTransactionAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum transaction amount.
    /// </summary>
    public string MaxTransactionAmount { get; set; }

    /// <summary>
    /// Gets or sets the transaction type.
    /// </summary>
    /// <remarks>
    /// Possible values: in, out, all (0, 1, 2).
    /// </remarks>
    public string TransactionType { get; set; }

    /// <summary>
    /// Gets or sets the page number.
    /// </summary>
    public string Page { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public string Size { get; set; }

    /// <summary>
    /// Gets or sets the order by.
    /// </summary>
    /// <remarks>
    /// Possible values: A (desc), Y (asc).
    /// </remarks>
    public string OrderBy { get; set; }

    /// <summary>
    /// Gets or sets the sort by.
    /// </summary>
    public string SortBy { get; set; }
}
