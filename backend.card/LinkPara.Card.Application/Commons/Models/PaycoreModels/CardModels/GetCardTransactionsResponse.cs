using System.Text.Json.Serialization;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

public class GetCardTransactionsResponse : PaycoreResponse
{
    [JsonPropertyName("transactions")]
    public CardTransactions[] CardTransactions { get; set; }

    [JsonPropertyName("totalDebit")]
    public decimal TotalDebit { get; set; }

    [JsonPropertyName("totalCredit")]
    public decimal TotalCredit { get; set; }
}

public class CardTransactions
{
    [JsonPropertyName("txnGuid")]
    public long TransactionId { get; set; }

    [JsonPropertyName("localTransactionDate")]
    public DateTime TransactionDate { get; set; }

    [JsonPropertyName("requestTime")]
    public string TransactionTime { get; set; }

    [JsonPropertyName("txnName")]
    public string TransactionName { get; set; }

    [JsonPropertyName("txnDescription")]
    public string TransactionDescription { get; set; }

    [JsonPropertyName("billingAmount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("billingCurrency")]
    public int Currency { get; set; }

    [JsonPropertyName("txnEffect")]
    public string TransactionEffect { get; set; }

    [JsonPropertyName("isFinancial")]
    public bool IsFinancial { get; set; }

    [JsonPropertyName("isProvision")]
    public bool IsProvision { get; set; }

    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; set; }

    [JsonPropertyName("responseCodeDesc")]
    public string ResponseDescription { get; set; }

    [JsonPropertyName("provisionCode")]
    public string ProvisionCode { get; set; }

    [JsonPropertyName("rrn")]
    public string Rrn { get; set; }

    [JsonPropertyName("merchantName")]
    public string MerchantName { get; set; }

    [JsonPropertyName("merchantCity")]
    public string MerchantCity { get; set; }

    [JsonPropertyName("merchantCountry")]
    public string MerchantCountry { get; set; }

    [JsonPropertyName("mcc")]
    public string Mcc { get; set; }

    [JsonPropertyName("transactionType")]
    public string TransactionType { get; set; }

    [JsonPropertyName("acquirerBankName")]
    public string AcquirerBankName { get; set; }

    [JsonPropertyName("channelCode")]
    public string ChannelCode { get; set; }

    [JsonPropertyName("network")]
    public string Network { get; set; }

    [JsonPropertyName("valueDate")]
    public DateTime? ValueDate { get; set; }

    [JsonPropertyName("cancellationDate")]
    public DateTime? CancellationDate { get; set; }

    [JsonPropertyName("isDigitalSlip")]
    public bool IsDigitalSlip { get; set; }

    [JsonPropertyName("qrData")]
    public string QrData { get; set; }

    [JsonPropertyName("txnSource")]
    public string TxnSource { get; set; }

    [JsonPropertyName("terminalType")]
    public string TerminalType { get; set; }

    [JsonPropertyName("securityLevelIndicator")]
    public string SecurityLevelIndicator { get; set; }

   }