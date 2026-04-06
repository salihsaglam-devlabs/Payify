namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

/// <summary>
/// Represents the balance details of an account.
/// </summary>
public class BalanceDto
{
    /// <summary>
    /// Gets or sets the account reference.
    /// </summary>
    public string AccountRef { get; set; }

    /// <summary>
    /// Gets or sets the balance amount.
    /// </summary>
    public string BalanceAmount { get; set; }

    /// <summary>
    /// Gets or sets the blocked amount.
    /// </summary>
    public string BlockedAmount { get; set; }

    /// <summary>
    /// Gets or sets the credit amount.
    /// </summary>
    public string CreditAmount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether credit is included. Enum 0-1.
    /// </summary>
    public string IsCreditIncluded { get; set; }

    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// Gets or sets the query time.
    /// </summary>
    public string QueryTime { get; set; }
}