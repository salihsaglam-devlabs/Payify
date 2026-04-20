namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Execute.Core;

/// <summary>
/// BKM reconciliation operation kodları.
/// Set, <c>docs/temp/Kural Kodu - Card İş Kuralıdır.cs</c> akışından doğan
/// minimum operasyonları içerir; eski custom approve/reject çiftleri kaldırılmıştır.
/// Eşleşme:
///  - Alert            : RaiseAlert (C1*, C2*, C10*, defansif)
///  - ManualReview     : CreateManualReview (sadece D6 - eşleniksiz iade)
///  - State correction : CorrectResponseCode, ConvertTransactionToFailed,
///                       ConvertTransactionToSuccessful  (D1, D3)
///  - Balance reverse  : ReverseByBalanceEffect (D1, D2, D3)
///  - Reverse original : ReverseOriginalTransaction (D7)
///  - Expire           : MoveTransactionToExpired (C7, D2)
///                       MoveCreatedTransactionToExpired (C8)
///  - Creation         : CreateTransaction (C8, C13)
///  - Refund           : ApplyOriginalEffectOrRefund (D4),
///                       ApplyLinkedRefund (D5),
///                       ApplyUnlinkedRefundEffect (D6 approve, FlowComments C16),
///                       StartChargeback (D6 reject, FlowComments C16)
///  - Difference       : InsertShadowBalanceEntry, RunShadowBalanceProcess (D8)
/// </summary>
internal static class OperationCodes
{
    /// <summary>Reconciliation alert kaydı oluşturur.</summary>
    public const string RaiseAlert = "RaiseAlert";

    /// <summary>Manuel review gate'i. Executor tarafında finansal etki yoktur;
    /// review kararına göre approve/reject branch'ine geçilir.</summary>
    public const string CreateManualReview = "CreateManualReview";

    /// <summary>D7: Orijinal işlemi tersine çevirir; aynı handler içinde
    /// IsCancelled=1 işaretlemesini de yapar.</summary>
    public const string ReverseOriginalTransaction = "ReverseOriginalTransaction";

    /// <summary>D1/D3: İşlemin response code bilgisini düzeltir.</summary>
    public const string CorrectResponseCode = "CorrectResponseCode";

    /// <summary>D1: İşlemi başarısız (Failed) statüsüne çeker.</summary>
    public const string ConvertTransactionToFailed = "ConvertTransactionToFailed";

    /// <summary>D3: İşlemi başarılı (Completed) statüsüne çeker.</summary>
    public const string ConvertTransactionToSuccessful = "ConvertTransactionToSuccessful";

    /// <summary>D1/D2/D3: İşlemin bakiye etkisini ters kayıtla geri alır.</summary>
    public const string ReverseByBalanceEffect = "ReverseByBalanceEffect";

    /// <summary>C7/D2: İşlemi Expired statüsüne taşır.</summary>
    public const string MoveTransactionToExpired = "MoveTransactionToExpired";

    /// <summary>C8/C13: Eksik olan Payify işlemini oluşturur.</summary>
    public const string CreateTransaction = "CreateTransaction";

    /// <summary>C8: CreateTransaction sonrasında oluşan işlemi expire eder.</summary>
    public const string MoveCreatedTransactionToExpired = "MoveCreatedTransactionToExpired";

    /// <summary>D4: Orijinaline göre effect veya refund uygular.</summary>
    public const string ApplyOriginalEffectOrRefund = "ApplyOriginalEffectOrRefund";

    /// <summary>D5: Eşlenikli iade işlemini uygular.</summary>
    public const string ApplyLinkedRefund = "ApplyLinkedRefund";

    /// <summary>D6 approve: Eşleniksiz iade için onay sonrası effect uygular.</summary>
    public const string ApplyUnlinkedRefundEffect = "ApplyUnlinkedRefundEffect";

    /// <summary>D6 reject: Reddedilen iade için chargeback sürecini başlatır.</summary>
    public const string StartChargeback = "StartChargeback";

    /// <summary>D8: Gölge bakiye için fark kaydı oluşturur (borç/alacak).</summary>
    public const string InsertShadowBalanceEntry = "InsertShadowBalanceEntry";

    /// <summary>D8: Gölge bakiye işleme sürecini çalıştırır.</summary>
    public const string RunShadowBalanceProcess = "RunShadowBalanceProcess";
}

