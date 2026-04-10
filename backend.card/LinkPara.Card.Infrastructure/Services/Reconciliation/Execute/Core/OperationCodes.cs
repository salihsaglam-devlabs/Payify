namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Execute.Core;

internal static class OperationCodes
{
    // Reconciliation alert kaydı oluşturur.
    // Etki:
    // - ReconciliationAlerts tablosuna insert yapar.
    // - RelatedOperationId, RelatedEvaluationId, Severity, AlertType, Message alanlarını yazar.
    public const string RaiseAlert = "RaiseAlert";
    // Orijinal işlemin IsCancelled alanını işaretler.
    // Girdi:
    // - referenceTransactionId
    // Etki:
    // - hedef transaction statü servisine gider.
    // - IsCancelled = true olacak şekilde güncelleme ister.
    public const string MarkOriginalTransactionCancelled = "MarkOriginalTransactionCancelled";
    // Orijinal işlemin bakiye etkisini tersine çevirir.
    // Girdi:
    // - referenceTransactionId
    // - currentTransactionId
    // - txnEffect
    // - billingAmount
    // - currencyCode
    // Etki:
    // - orijinal işlem için ters yönlü finansal effect oluşturur.
    public const string ReverseOriginalTransaction = "ReverseOriginalTransaction";
    // İşlemin response code bilgisini düzeltir.
    // Girdi:
    // - currentTransactionId
    // - responseCode
    // Etki:
    // - transaction üzerindeki responseCode değerini düzeltir.
    // - responseCode = 00 ise isSuccessful=true, değilse false olarak günceller.
    public const string CorrectResponseCode = "CorrectResponseCode";
    // İşlemi başarısız statüsüne çeker.
    // Girdi:
    // - currentTransactionId
    // - isSettlementReceived
    // Etki:
    // - transaction targetStatus = Failed olacak şekilde güncellenir.
    public const string ConvertTransactionToFailed = "ConvertTransactionToFailed";
    // İşlemi başarılı statüsüne çeker.
    // Girdi:
    // - currentTransactionId
    // - isSettlementReceived
    // Etki:
    // - transaction targetStatus = Completed olacak şekilde güncellenir.
    public const string ConvertTransactionToSuccessful = "ConvertTransactionToSuccessful";
    // İşlemin bakiye etkisini ters kayıtla geri alır.
    // Girdi:
    // - currentTransactionId
    // - optional referenceTransactionId
    // - txnEffect
    // - billingAmount
    // - currencyCode
    // Etki:
    // - mevcut işlemin effect yönünü tersleyerek düzeltme kaydı üretir.
    public const string ReverseByBalanceEffect = "ReverseByBalanceEffect";
    // İşlemi expire statüsüne taşır.
    // Girdi:
    // - currentTransactionId
    // Etki:
    // - transaction expire endpoint'ine gider.
    // - hedef işlemin statüsünü Expired yapar.
    public const string MoveTransactionToExpired = "MoveTransactionToExpired";
    // Eksik olan Payify işlemini oluşturur.
    // Girdi:
    // - currentTransactionId
    // - optional referenceTransactionId
    // - cardNo
    // - billingAmount
    // - currencyCode
    // - externalReferenceId
    // - merchantId
    // - description
    // Etki:
    // - cardNo üzerinden wallet binding çözülür.
    // - userId ve walletNumber bulunursa transaction create çağrısı yapılır.
    public const string CreateTransaction = "CreateTransaction";
    // Yeni oluşturulan işlemi expire statüsüne taşır.
    // Etki:
    // - CreateTransaction sonrası oluşan transaction için expire adımını uygular.
    public const string MoveCreatedTransactionToExpired = "MoveCreatedTransactionToExpired";
    // Orijinal işleme göre effect veya refund uygular.
    // Girdi:
    // - currentTransactionId
    // - optional referenceTransactionId
    // - txnEffect
    // - billingAmount
    // - currencyCode
    // - cardNo
    // Etki:
    // - refund/effect endpoint'ine gider.
    // - orijinal işleme bağlı effect veya refund kaydı üretir.
    public const string ApplyOriginalEffectOrRefund = "ApplyOriginalEffectOrRefund";
    // Eşlenikli iade işlemini uygular.
    // Girdi:
    // - currentTransactionId
    // - referenceTransactionId
    // - txnEffect
    // - billingAmount
    // - currencyCode
    // Etki:
    // - orijinal işlem ile bağlı linked refund kaydını uygular.
    public const string ApplyLinkedRefund = "ApplyLinkedRefund";
    // Manuel karar gerektiren review kaydı için gate adımıdır.
    // Etki:
    // - execution tarafında doğrudan finansal işlem yapmaz.
    // - ReconciliationReview kaydının kararını okur.
    // - approve/reject branch'lerinden hangisinin açılacağını belirler.
    public const string CreateManualReview = "CreateManualReview";
    // Eşleniksiz iade için onay sonrası effect uygular.
    // Girdi:
    // - currentTransactionId
    // - optional referenceTransactionId
    // - txnEffect
    // - billingAmount
    // - currencyCode
    // Etki:
    // - manual approve sonrası unlinked refund effect'ini uygular.
    public const string ApplyUnlinkedRefundEffect = "ApplyUnlinkedRefundEffect";
    // Reddedilen iade için chargeback sürecini başlatır.
    // Girdi:
    // - currentTransactionId
    // - optional referenceTransactionId
    // - cardNo
    // - merchantId
    // - billingAmount
    // - currencyCode
    // Etki:
    // - chargeback başlatma isteği üretir.
    public const string StartChargeback = "StartChargeback";
    // ACC problemli expire kaydını onaylayıp akışı kapatır.
    // Etki:
    // - manuel branch sonucu olarak bilgi amaçlı kapanış üretir.
    // - finansal düzeltme yapmaz, review kararını finalize eder.
    public const string ApprovePendingAccReview = "ApprovePendingAccReview";
    // ACC problemli expire kaydını reddedip akışı kapatır.
    // Etki:
    // - manuel branch sonucu olarak bilgi amaçlı kapanış üretir.
    // - finansal düzeltme yapmaz, review kararını finalize eder.
    public const string RejectPendingAccReview = "RejectPendingAccReview";
    // Gölge bakiye için fark kaydı oluşturur.
    // Girdi:
    // - currentTransactionId
    // - differenceAmount
    // - currencyCode
    // - txnEffect
    // Etki:
    // - borç/alacak tablosuna fark kadar gölge bakiye kaydı yazdırır.
    public const string InsertShadowBalanceEntry = "InsertShadowBalanceEntry";
    // Gölge bakiye işleme sürecini çalıştırır.
    // Girdi:
    // - currentTransactionId
    // - differenceAmount
    // Etki:
    // - önceden yazılan gölge bakiye kaydını işleme alır.
    public const string RunShadowBalanceProcess = "RunShadowBalanceProcess";
    // Eksik card row vakasını düzelterek yeniden işleme alır.
    // Etki:
    // - ilgili IngestionFileLine kaydını ReconciliationStatus = Ready yapar.
    // - case yeniden evaluation kuyruğuna girer.
    public const string RecoverMissingCardRow = "RecoverMissingCardRow";
    // Eksik card row vakasını reddedip kapatır.
    // Etki:
    // - manuel branch'i bilgi amaçlı alert ile kapatır.
    // - yeniden kuyruğa almaz.
    public const string DropMissingCardRow = "DropMissingCardRow";
    // Belirsiz Payify eşleşmesini onaylayıp akışı sürdürür.
    // Etki:
    // - ilgili row'u yeniden evaluation için Ready durumuna alır.
    // - seçilen Payify eşleşmesi ile akışın tekrar kurulması beklenir.
    public const string ApproveAmbiguousPayifyRecord = "ApproveAmbiguousPayifyRecord";
    // Belirsiz Payify eşleşmesini reddedip kapatır.
    // Etki:
    // - manuel branch'i bilgi amaçlı alert ile kapatır.
    public const string RejectAmbiguousPayifyRecord = "RejectAmbiguousPayifyRecord";
    // Kuralsız veya eşleşmeyen akışı onaylayıp kapatır.
    // Etki:
    // - unsupported/unmatched flow için manuel kararı onaylar.
    // - otomatik finansal düzeltme yapmadan vakayı kapatır.
    public const string ApproveUnmatchedFlow = "ApproveUnmatchedFlow";
    // Kuralsız veya eşleşmeyen akışı reddedip kapatır.
    // Etki:
    // - unsupported/unmatched flow'u reddederek manuel branch'i kapatır.
    public const string RejectUnmatchedFlow = "RejectUnmatchedFlow";
    // Reversal için orijinal işlemi bağlayıp akışı yeniden sürdürür.
    // Etki:
    // - ilgili row'u yeniden evaluation için Ready durumuna alır.
    // - bir sonraki değerlendirmede reversal akışının çözülebilmesi beklenir.
    public const string BindOriginalTransactionAndContinue = "BindOriginalTransactionAndContinue";
    // Reversal kaydını reddedip kapatır.
    // Etki:
    // - manuel branch'i finansal düzeltme yapmadan kapatır.
    public const string RejectReversalRecord = "RejectReversalRecord";
    // Eksik Payify işlem vakasını onaylayıp kapatır.
    // Etki:
    // - eksik Payify transaction vakasını manuel olarak kabul eder.
    // - otomatik finansal düzeltme yapmadan branch'i kapatır.
    public const string ApproveMissingPayifyTransaction = "ApproveMissingPayifyTransaction";
    // Eksik Payify işlem vakasını reddedip kapatır.
    // Etki:
    // - eksik Payify transaction vakasını manuel olarak reddeder.
    // - otomatik finansal düzeltme yapmadan branch'i kapatır.
    public const string RejectMissingPayifyTransaction = "RejectMissingPayifyTransaction";
}
