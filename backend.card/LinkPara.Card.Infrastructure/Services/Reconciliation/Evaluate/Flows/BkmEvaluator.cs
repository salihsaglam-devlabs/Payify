using System.Globalization;
using System.Text.Json;
using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using LinkPara.Card.Domain.Entities.FileIngestion.Schemas;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Execute.Core;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Flows;

/// <summary>
/// BKM kart işlemleri (card transaction) mutabakat değerlendiricisi.
///
/// Bu sınıf, BKM dosyasından gelen tek bir işlem satırını Payify (Emoney) tarafındaki
/// karşılığı ile karşılaştırır ve "Kural Kodu" dokümanındaki karar ağacını uygular.
/// Çıktı olarak her zaman bir <see cref="EvaluationResult"/> üretir; sonuç
/// aşağıdakilerden bir veya birden fazlasını içerebilir:
///   - Note      : İlgili case'i operatöre/loglara açıklayan metin
///   - AutoOp    : Sistem tarafından otomatik uygulanacak operasyon(lar)
///   - ManualOp  : Operatör onayı / reddi gerektiren manuel inceleme
///
/// Karar ağacındaki ana kontrol noktaları:
///   C1   -> Dosya satırının uzunluk / parse doğrulaması (file length validity).
///   C2   -> Aynı dosyada/işlem kümesinde duplicate (uniqueness) kontrolü.
///   C3   -> İşlemin cancel / reversal tipinde olup olmadığı (D7 yönlendirmesi).
///   C4   -> Cancel/reversal olduğunda orijinal işlemin daha önce iptal edilip edilmediği.
///   C5   -> Dosya tarafındaki işlem statüsünün ayrımı (Failed / Expired / Successful).
///   C19  -> Payify tarafındaki işlem statüsünün ayrımı (Failed / Missing / Successful).
///   C7   -> Expire branch & payify Failed -> işlemi sistemde de Expire'a çek.
///   C8   -> Expire branch & payify Missing -> işlemi yarat ve Expire'a çek.
///   C9   -> Expire branch & payify Successful & ACC tarafında bekleyen eşleşme var mı?
///   C10  -> ACC pending match varsa: yalnızca alarm (manuel müdahale Paycore'da).
///   C11  -> ACC pending match yoksa: D2 (Expire'a çek + bakiye etkisini geri al).
///   C12  -> Successful dosya işlemi & payify Failed -> D3 (response code düzelt + başarılıya çevir + bakiye geri al).
///   C13  -> Successful dosya işlemi & payify Missing -> önce işlemi yarat.
///   C14  -> C13 sonrası refund değilse D4 (orijinal etki / refund uygula).
///   C15  -> Successful & refund & matched (linked) refund.
///   C16  -> Successful & refund & matched değil -> manuel inceleme (D6).
///   C17  -> Matched refund -> D5 (linked refund otomatik uygula).
///   D1   -> Failed dosya & payify Successful -> response code düzelt + Failed'a çevir + bakiye geri al.
///   D7   -> Cancel/Reversal akışında orijinali ters çevir + IsCancelled=1.
///   D8   -> Successful & payify Successful & amount &lt; billing -> fark kadar shadow balance.
///
/// Davranışsal notlar:
///   - "NoAction" sadece Note ile sonlanan branch'tir; sistem hiçbir operasyon tetiklemez.
///   - "Alert" branch'lerinde otomatik düzeltme yapılmaz; insan dikkatine sunulur.
///   - "AutoOperation" branch'leri kuralın mekanik olarak güvenli kabul ettiği düzeltmelerdir.
///   - "ManualOperation" branch'leri operatör onayına bırakılır (Approve / Reject akışı tanımlıdır).
/// </summary>
internal sealed class BkmEvaluator : IEvaluator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly CardDbContext _dbContext;
    private readonly IEmoneyService _emoneyService;
    private readonly IStringLocalizer _localizer;

    public BkmEvaluator(
        CardDbContext dbContext,
        IEmoneyService emoneyService,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _emoneyService = emoneyService;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public bool CanEvaluate(FileContentType fileContentType) => fileContentType == FileContentType.Bkm;

    /// <summary>
    /// Tek bir BKM satırı için karar ağacının giriş noktası.
    ///
    /// Akış:
    ///   1) Root detail elde edilir; yoksa veri tutarsızlığı sayılır ve exception fırlatılır
    ///      (üst katman bunu kayda alır; sessiz geçilmez).
    ///   2) C1 - Dosya satırı parse / length doğrulamasını geçemediyse veriye güvenilmez,
    ///      sadece alarm üretilir ve akış sonlandırılır.
    ///   3) C2 - Duplicate kontrolü; conflict ya da secondary kayıtlar burada durdurulur.
    ///      Primary duplicate'lerde alarm üretilir ama akışa devam edilir.
    ///   4) C3 - İşlem cancel/reversal tipindeyse normal status branch'ine girilmez;
    ///      önce orijinal işlem üzerinden D7 değerlendirmesi yapılır.
    ///   5) Aksi halde C5 (file status) ve C19 (payify status) çözümlenir; ardından
    ///      ilgili Failed / Expired / Successful branch'i çağrılır.
    /// </summary>
    public async Task<EvaluationResult> EvaluateAsync(
        EvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        var bkmContext = (BkmEvaluationContext)context;
        var result = new EvaluationResult();

        // Root detail bulunamazsa: dosya parse edilmiş olsa bile değerlendirme imkânsız.
        // Sessiz dönmek yerine domain exception fırlatılır; üst katman bunu izlenebilir yapar.
        var detail = GetRootCardDetail(bkmContext)
            ?? throw new ReconciliationCurrentCardRowMissingException(
                _localizer.Get("Reconciliation.Bkm.CurrentCardRowMissing"));
        bkmContext.CachedRootDetail = detail;

        // C1* - Dosya satırı uzunluk / parse doğrulaması başarısız.
        // Veri güvenilmez kabul edilir; otomatik düzeltme YAPILMAZ, sadece alarm üretilir.
        if (HasFileLengthValidationFailure(bkmContext))
        {
            result.SetNote(_localizer.Get("Reconciliation.Bkm.FileLengthValidationNote"));
            result.AddAutoOperation(
                OperationCodes.RaiseAlert,
                _localizer.Get("Reconciliation.Bkm.FileLengthValidationOp"),
                BkmSnapshotBuilder.Create(bkmContext, OperationCodes.RaiseAlert, "C1"));
            return result;
        }

        // C2* - Duplicate / uniqueness değerlendirmesi.
        // Conflict ve Secondary case'lerinde akış burada sonlanır; Primary'de devam eder
        // (yine de operatör görsün diye alarm üretilir).
        if (HandleDuplicate(result, bkmContext) == DuplicateOutcome.Stop)
        {
            return result;
        }

        // CLEARING-PRESENCE GATE
        // ----------------------
        // İş kuralı: Bir kart işleminin doğru değerlendirilebilmesi için ilgili clearing
        // (ACC) kaydının da elimizde olması gerekir. Clearing dosyası henüz işlenmemişse
        // (ClearingDetails boşsa) işlemi şu an karara bağlamayız:
        //   1) EvaluationResult AwaitingClearing olarak işaretlenir;
        //      EvaluateService bu işareti görüp satırı ReconciliationStatus.AwaitingClearing'e taşır.
        //   2) Bilgilendirme amaçlı tek bir RaiseAlert üretilir; otomatik düzeltme veya manuel
        //      review YAPILMAZ - karar netleşmediği için operatör süreci gereksiz yere açılmaz.
        //   3) Clearing dosyası geldiğinde ClearingArrivalRequeueService satırı
        //      ReconciliationStatus.Ready'ye geri çeker ve değerlendirici tekrar çağrılır.
        // Bu sayede aynı işlem clearing gelmeden boşa karar üretmez; clearing gelince ise
        // karar ağacı baştan, eksiksiz veriyle çalışır.
        if (bkmContext.ClearingDetails.Count == 0)
        {
            return BuildAwaitingClearingResult(result, bkmContext, "AWAIT_CLEARING");
        }

        // C3* - İşlem iptal / reversal tipinde mi?
        // Eğer öyleyse normal status akışına girilmez; orijinal işlem bulunup
        // D7 (reverse + IsCancelled=1) kuralına göre karar verilir.
        if (IsCancelOrReversal(detail))
        {
            return await EvaluateCancelOrReversalAsync(result, bkmContext, detail, cancellationToken);
        }

        // C5* / C19* - Dosya ve Payify statüleri çözümlenir.
        // latestEmoney sadece Successful + amount karşılaştırması (D8) gereken case için kullanılır.
        var fileStatus = ResolveFileTransactionStatus(detail);
        var payifyStatus = ResolvePayifyStatus(bkmContext.EmoneyTransactions);
        var latestEmoney = GetLatestEmoneyTransaction(bkmContext.EmoneyTransactions);

        return fileStatus switch
        {
            FileTransactionStatus.Failed     => EvaluateFailedBranch(result, bkmContext, payifyStatus),
            FileTransactionStatus.Expired    => await EvaluateExpireBranchAsync(result, bkmContext, detail, payifyStatus, cancellationToken),
            FileTransactionStatus.Successful => EvaluateSuccessfulBranch(result, bkmContext, detail, payifyStatus, latestEmoney),
            _                                => result
        };
    }

    // ----------------------------------------------------------------------
    // C3* / C4* / D7 - Cancel & Reversal değerlendirmesi
    // ----------------------------------------------------------------------
    /// <summary>
    /// Dosyadaki işlem cancel/reversal tipinde olduğunda çağrılır.
    ///
    /// Karar noktaları:
    ///   - Orijinal işlem (referans verilen MainTxnGuid) bulunamadıysa: veri tutarsızlığı.
    ///     Kural Kodu bu olasılığı tanımlamadığı için defansif olarak SADECE alarm üretilir;
    ///     hiçbir bakiye / statü operasyonu yapılmaz (yanlış yöne düzeltme yapmamak için).
    ///   - C3: Orijinal işlem zaten iptal edilmişse (IsCancelled = true) tekrar reverse
    ///     edilmez -> NoAction (yalnızca açıklayıcı not).
    ///   - C4 + D7: Orijinal henüz iptal edilmemişse otomatik olarak ters çevrilir
    ///     ve IsCancelled=1 yapılır. İki adım da tek operasyon koduna sarılır;
    ///     sıralama executor handler içinde garanti edilir.
    /// </summary>
    private async Task<EvaluationResult> EvaluateCancelOrReversalAsync(
        EvaluationResult result,
        BkmEvaluationContext context,
        CardBkmDetail detail,
        CancellationToken cancellationToken)
    {
        var original = await ResolveOriginalTransactionAsync(detail, cancellationToken);

        // Defansif branch: Kural Kodu "original yok" varsayımı yapmaz; ancak gerçek dünyada
        // referans verilen işlem bulunamayabilir. Otomatik aksiyon almak yerine alarm üretilir
        // ki yanlış bir bakiye/statü düzeltmesi yapılmasın.
        if (original is null)
        {
            result.SetNote(_localizer.Get("Reconciliation.Bkm.OriginalTxnNotResolvedNote"));
            result.AddAutoOperation(
                OperationCodes.RaiseAlert,
                _localizer.Get("Reconciliation.Bkm.OriginalTxnNotResolvedOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C3"));
            return result;
        }

        // C3 -> "Orijinal işlem daha önce iptal edilmiş mi?"
        // EVET ise iş zaten tamamlanmış; tekrar reverse etmek mükerrer bakiye etkisi yaratır.
        // -> NoAction (sadece bilgi notu).
        if (original.IsCancelled)
        {
            result.SetNote(_localizer.Get("Reconciliation.Bkm.AlreadyCancelledNote"));
            return result;
        }

        // C4* + D7: Orijinal canlı; iptal/reversal etkisini uygula:
        //   1) ReverseOriginalTransaction -> orijinalin bakiye etkisini ters çevir
        //   2) Aynı operasyon handler'ı IsCancelled=1 işaretini de atar (sıra garantili).
        result.SetNote(_localizer.Get("Reconciliation.Bkm.ReversalNotCancelledNote"));
        result.AddAutoOperation(
            OperationCodes.ReverseOriginalTransaction,
            _localizer.Get("Reconciliation.Bkm.ReverseOriginalOp"),
            BkmSnapshotBuilder.Create(context, OperationCodes.ReverseOriginalTransaction, "D7",
                ("referenceTransactionId", original.Id)));
        return result;
    }

    // ----------------------------------------------------------------------
    // C5* - File status BAŞARISIZ branch (Failed)
    // ----------------------------------------------------------------------
    /// <summary>
    /// Dosya tarafında işlem Failed göründüğünde payify statüsüne göre karar verilir.
    ///
    /// Case dağılımı:
    ///   - payify Failed     : İki taraf da başarısız; tutarlı durum -> NoAction (yalnızca not).
    ///   - payify Missing    : Dosya başarısız, payify'da hiç kayıt yok; tutarlı -> NoAction.
    ///   - payify Successful : ÇELİŞKİ. Payify'da işlem başarılı sonuçlanmış ama dosya başarısız.
    ///                         D1 uygulanır:
    ///                           1) Response code'u doğru değere çek (CorrectResponseCode)
    ///                           2) İşlem statüsünü Failed'a çevir (ConvertTransactionToFailed)
    ///                           3) Bakiye etkisini geri al (ReverseByBalanceEffect)
    /// </summary>
    private EvaluationResult EvaluateFailedBranch(
        EvaluationResult result,
        BkmEvaluationContext context,
        PayifyStatus payifyStatus)
    {
        switch (payifyStatus)
        {
            case PayifyStatus.Failed:
                // İki taraf da Failed -> tutarlı, herhangi bir aksiyon gerekmez.
                result.SetNote(_localizer.Get("Reconciliation.Bkm.FailedBothNote"));
                return result;

            case PayifyStatus.Missing:
                // Payify'da hiç işlem yok ve dosya da başarısız -> tutarlı, aksiyon yok.
                result.SetNote(_localizer.Get("Reconciliation.Bkm.FailedMissingPayifyNote"));
                return result;

            case PayifyStatus.Successful:
                // C6* + D1 - Payify başarılı ama dosya başarısız: müşterinin bakiyesi yanlış olabilir.
                // 3 adımlı otomatik düzeltme uygulanır (sıralı): kod düzelt -> statü çevir -> bakiye geri al.
                result.SetNote(_localizer.Get("Reconciliation.Bkm.FailedSuccessfulPayifyNote"));
                result.AddAutoOperation(
                    OperationCodes.CorrectResponseCode,
                    _localizer.Get("Reconciliation.Bkm.CorrectResponseCodeOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.CorrectResponseCode, "D1"));
                result.AddAutoOperation(
                    OperationCodes.ConvertTransactionToFailed,
                    _localizer.Get("Reconciliation.Bkm.ConvertToFailedOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.ConvertTransactionToFailed, "D1"));
                result.AddAutoOperation(
                    OperationCodes.ReverseByBalanceEffect,
                    _localizer.Get("Reconciliation.Bkm.ReverseByBalanceOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.ReverseByBalanceEffect, "D1"));
                return result;

            default:
                // Tanımlı olmayan payify statüsü için defansif çıkış.
                return result;
        }
    }

    // ----------------------------------------------------------------------
    // C5* - File status EXPIRE branch
    // ----------------------------------------------------------------------
    /// <summary>
    /// Dosya tarafında işlem Expire (zaman aşımı) olarak geldiğinde uygulanır.
    ///
    /// Case dağılımı:
    ///   - payify Failed     : C7  -> Sistemdeki işlemi de Expire'a çek (otomatik).
    ///   - payify Missing    : C8  -> Önce işlemi yarat, sonra Expire'a çek (iki adımlı otomatik).
    ///   - payify Successful : C9  -> ACC tarafında bekleyen (ControlStat=P) bir eşleşme var mı?
    ///                           Var ise  C10: yalnızca ALARM (manuel müdahale Paycore tarafında).
    ///                           Yok ise  C11 + D2: Expire'a çek + bakiye etkisini geri al.
    /// </summary>
    private async Task<EvaluationResult> EvaluateExpireBranchAsync(
        EvaluationResult result,
        BkmEvaluationContext context,
        CardBkmDetail detail,
        PayifyStatus payifyStatus,
        CancellationToken cancellationToken)
    {
        switch (payifyStatus)
        {
            case PayifyStatus.Failed:
                // C7* - Payify zaten Failed; dosya Expire diyor. Sistemdeki işlemi Expire'a taşı.
                result.SetNote(_localizer.Get("Reconciliation.Bkm.ExpiredPayifyFailedNote"));
                result.AddAutoOperation(
                    OperationCodes.MoveTransactionToExpired,
                    _localizer.Get("Reconciliation.Bkm.MoveToExpiredOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.MoveTransactionToExpired, "C7"));
                return result;

            case PayifyStatus.Missing:
                // C8* - Payify'da kayıt yok ama dosya Expire diyor.
                // İki adımlı otomatik akış: önce işlem yaratılır, ardından Expire'a taşınır.
                result.SetNote(_localizer.Get("Reconciliation.Bkm.ExpiredMissingPayifyNote"));
                result.AddAutoOperation(
                    OperationCodes.CreateTransaction,
                    _localizer.Get("Reconciliation.Bkm.CreateTransactionOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.CreateTransaction, "C8"));
                result.AddAutoOperation(
                    OperationCodes.MoveCreatedTransactionToExpired,
                    _localizer.Get("Reconciliation.Bkm.MoveToExpiredOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.MoveCreatedTransactionToExpired, "C8"));
                return result;

            case PayifyStatus.Successful:
                // C9* - ACC tarafında ControlStat=P (Problem/Pending) bir clearing kaydı var mı?
                // Varsa eşleşme henüz operatör tarafında çözülmemiş demektir; otomatik düzeltme YAPMA.
                if (await HasAccPendingMatchAsync(detail, context, cancellationToken))
                {
                    // C10* - Yalnızca alarm; manuel kontrol Paycore mutabakat ekranı üzerinden yapılır.
                    result.SetNote(_localizer.Get("Reconciliation.Bkm.AccPendingMatchNote"));
                    result.AddAutoOperation(
                        OperationCodes.RaiseAlert,
                        _localizer.Get("Reconciliation.Bkm.AccPendingMatchOp"),
                        BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C10"));
                    return result;
                }

                // C11* + D2 - Bekleyen eşleşme yok: işlemi Expire'a taşı ve bakiye etkisini geri al.
                result.SetNote(_localizer.Get("Reconciliation.Bkm.ExpiredSuccessfulNoAccNote"));
                result.AddAutoOperation(
                    OperationCodes.MoveTransactionToExpired,
                    _localizer.Get("Reconciliation.Bkm.MoveToExpiredOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.MoveTransactionToExpired, "D2"));
                result.AddAutoOperation(
                    OperationCodes.ReverseByBalanceEffect,
                    _localizer.Get("Reconciliation.Bkm.ReverseByBalanceOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.ReverseByBalanceEffect, "D2"));
                return result;

            default:
                return result;
        }
    }

    // ----------------------------------------------------------------------
    // C5* - File status BAŞARILI branch (Successful)
    // ----------------------------------------------------------------------
    /// <summary>
    /// Dosya tarafında işlem Successful (ResponseCode=00 ve IsSuccessfulTxn=Successful) ise çağrılır.
    ///
    /// Önemli ön koşul:
    ///   - TxnSettle = Y değilse işlem henüz settle edilmemiş; mutabakat aksiyonu üretilmez (NoAction).
    ///     Bu kontrol gün-içi geçici farkları otomatik düzeltmeye dönüştürmemek için kritiktir.
    ///
    /// Case dağılımı (TxnSettle = Y sonrası):
    ///   - payify Failed     : C12 + D3 -> response code düzelt, başarılıya çevir, bakiye geri al.
    ///   - payify Missing    : C13 -> önce işlemi yarat. Sonra:
    ///                           refund değilse C14 + D4 (orijinal etkisini uygula)
    ///                           refund ise EvaluateRefund() (matched/unmatched ayrımı).
    ///   - payify Successful : refund ise EvaluateRefund(); değilse amount karşılaştırması yapılır:
    ///                           amount = billing -> NoAction
    ///                           amount &lt; billing -> D8 (fark kadar shadow balance entry)
    ///                           amount &gt; billing -> NoAction (Kural Kodu bu dalda EVET=END)
    ///
    /// Defansif: payify Successful ama latestEmoney null ise tutar karşılaştırılamaz; sadece not.
    /// </summary>
    private EvaluationResult EvaluateSuccessfulBranch(
        EvaluationResult result,
        BkmEvaluationContext context,
        CardBkmDetail detail,
        PayifyStatus payifyStatus,
        EmoneyCustomerTransactionDto? latestEmoney)
    {
        // TxnSettle = Y mi? Settle edilmemiş işlemler için aksiyon üretilmez.
        // Bu, gün-içi henüz netleşmemiş kayıtların yanlış düzeltilmesini engeller.
        if (!IsSettled(detail))
        {
            result.SetNote(_localizer.Get("Reconciliation.Bkm.NotSettledNote"));
            return result;
        }

        switch (payifyStatus)
        {
            case PayifyStatus.Failed:
                // C12* + D3 - Dosya başarılı ama payify Failed: müşteri lehine eksik kayıt.
                // 3 adımlı otomatik düzeltme: response code düzelt -> Successful'a çevir -> bakiye etkisini uygula.
                result.SetNote(_localizer.Get("Reconciliation.Bkm.SuccessfulFailedPayifyNote"));
                result.AddAutoOperation(
                    OperationCodes.CorrectResponseCode,
                    _localizer.Get("Reconciliation.Bkm.CorrectResponseCodeOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.CorrectResponseCode, "D3"));
                result.AddAutoOperation(
                    OperationCodes.ConvertTransactionToSuccessful,
                    _localizer.Get("Reconciliation.Bkm.ConvertToSuccessfulOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.ConvertTransactionToSuccessful, "D3"));
                result.AddAutoOperation(
                    OperationCodes.ReverseByBalanceEffect,
                    _localizer.Get("Reconciliation.Bkm.ReverseByBalanceOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.ReverseByBalanceEffect, "D3"));
                return result;

            case PayifyStatus.Missing:
                // C13* - Payify'da hiç işlem yok ama dosya başarılı diyor. Önce işlem yaratılır.
                result.AddAutoOperation(
                    OperationCodes.CreateTransaction,
                    _localizer.Get("Reconciliation.Bkm.CreateTransactionOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.CreateTransaction, "C13"));

                // C14* + D4 - Refund değilse: orijinal işlem etkisini bakiyeye uygula.
                if (!IsRefund(detail))
                {
                    result.SetNote(_localizer.Get("Reconciliation.Bkm.SuccessfulMissingNonRefundNote"));
                    result.AddAutoOperation(
                        OperationCodes.ApplyOriginalEffectOrRefund,
                        _localizer.Get("Reconciliation.Bkm.ApplyOriginalEffectOp"),
                        BkmSnapshotBuilder.Create(context, OperationCodes.ApplyOriginalEffectOrRefund, "D4"));
                    return result;
                }

                // Refund ise matched/unmatched ayrımı için ortak refund değerlendirmesine devam et.
                return EvaluateRefund(result, context, detail);

            case PayifyStatus.Successful:
                // Hem dosya hem payify başarılı.
                // Refund ise (linked / unlinked ayrımı için) refund değerlendirmesine git.
                if (IsRefund(detail))
                {
                    return EvaluateRefund(result, context, detail);
                }

                // Defansif: payify Successful dediği halde tutar bilgisini taşıyan kayıt elde edilemediyse
                // amount karşılaştırması yapamayız; sadece not bırakılır.
                if (latestEmoney is null)
                {
                    result.SetNote(_localizer.Get("Reconciliation.Bkm.PayifyNotResolvedNote"));
                    return result;
                }

                // Tutarlar eşitse mutabakat zaten sağlanmış demektir -> NoAction.
                if (AreAmountsEqual(latestEmoney.Amount, detail.BillingAmount))
                {
                    result.SetNote(_localizer.Get("Reconciliation.Bkm.AmountsEqualNote"));
                    return result;
                }

                // D8 - Payify tutarı billing tutarından küçükse müşteri lehine eksik tahsilat var.
                // Sadece FARK kadar gölge bakiye (shadow balance) etkisi oluşturulur; gerçek bakiye dokunulmaz.
                if (IsTransactionAmountLessThanBilling(latestEmoney.Amount, detail.BillingAmount))
                {
                    var difference = decimal.Round(detail.BillingAmount - latestEmoney.Amount, 2,
                        MidpointRounding.AwayFromZero);
                    result.SetNote(_localizer.Get("Reconciliation.Bkm.AmountLessThanBillingNote"));
                    result.AddAutoOperation(
                        OperationCodes.InsertShadowBalanceEntry,
                        _localizer.Get("Reconciliation.Bkm.InsertShadowBalanceOp"),
                        BkmSnapshotBuilder.Create(context, OperationCodes.InsertShadowBalanceEntry, "D8",
                            ("differenceAmount", difference)));
                    result.AddAutoOperation(
                        OperationCodes.RunShadowBalanceProcess,
                        _localizer.Get("Reconciliation.Bkm.RunShadowBalanceOp"),
                        BkmSnapshotBuilder.Create(context, OperationCodes.RunShadowBalanceProcess, "D8",
                            ("differenceAmount", difference)));
                    return result;
                }

                // amount > billing: Kural Kodu'nda EVET dalı boş bırakılmıştır -> END (NoAction).
                // Bu durum müşteri aleyhine fazladan tahsilat ifade etmez (zaten payify daha az tahsil etmiş demektir);
                // dolayısıyla otomatik aksiyon tanımlı değildir, sadece not düşülür.
                result.SetNote(_localizer.Get("Reconciliation.Bkm.AmountGreaterNote"));
                return result;

            default:
                return result;
        }
    }

    // ----------------------------------------------------------------------
    // C15 / C16 / C17 - Refund değerlendirmesi (D5 / D6)
    // ----------------------------------------------------------------------
    /// <summary>
    /// Refund / ReferenceRefund tipindeki işlemler için ayrı değerlendirme.
    ///
    /// Karar:
    ///   - Matched (linked) refund (C17 + D5): Orijinal işlem referansı (MainTxnGuid) dolu ve
    ///     mevcut işlemden farklı -> bağlı refund otomatik uygulanır (ApplyLinkedRefund).
    ///   - Unmatched (C16 + D6): Refund ama bir orijinal işleme bağlanamamış. Bu, ya yanlış kayıt
    ///     ya da kara para / suistimal şüphesi taşıyabilir; bu nedenle OTOMATİK aksiyon yapılmaz,
    ///     manuel inceleme açılır:
    ///       Approve -> ApplyUnlinkedRefundEffect (etkisi bakiyeye yansıtılır)
    ///       Reject  -> StartChargeback (Paycore tarafına chargeback başlatılır)
    /// </summary>
    private EvaluationResult EvaluateRefund(
        EvaluationResult result,
        BkmEvaluationContext context,
        CardBkmDetail detail)
    {
        if (IsMatchedRefund(detail))
        {
            // C17* + D5 - Orijinal işleme bağlı refund; güvenli, otomatik uygulanabilir.
            result.SetNote(_localizer.Get("Reconciliation.Bkm.LinkedRefundNote"));
            result.AddAutoOperation(
                OperationCodes.ApplyLinkedRefund,
                _localizer.Get("Reconciliation.Bkm.LinkedRefundOp"),
                BkmSnapshotBuilder.Create(context, OperationCodes.ApplyLinkedRefund, "D5"));
            return result;
        }

        // C16* + D6 - Eşleniksiz iade. Otomatik aksiyon güvenli değildir; manuel review oluşturulur.
        // FlowComments C16 referansı: Approve -> bakiyeye etki uygula; Reject -> Paycore'a chargeback başlat.
        result.SetNote(_localizer.Get("Reconciliation.Bkm.UnlinkedRefundNote"));
        result.AddManualOperation(
            OperationCodes.CreateManualReview,
            _localizer.Get("Reconciliation.Bkm.UnlinkedRefundManualOp"),
            code => BkmSnapshotBuilder.Create(context, code, "D6"),
            approveCode: OperationCodes.ApplyUnlinkedRefundEffect,
            approveNote: _localizer.Get("Reconciliation.Bkm.ApplyEffectAfterApproval"),
            rejectCode: OperationCodes.StartChargeback,
            rejectNote: _localizer.Get("Reconciliation.Bkm.StartChargebackAfterRejection"));
        return result;
    }

    // ----------------------------------------------------------------------
    // Clearing-presence gate - AwaitingClearing sonucu üretimi
    // ----------------------------------------------------------------------
    /// <summary>
    /// Clearing dosyası henüz ulaşmadığı için kararı erteleyen sonuç tipini üretir.
    ///
    /// Davranış:
    ///   - Sadece bilgilendirme amaçlı tek bir RaiseAlert üretilir (manual review YOKTUR;
    ///     henüz operatöre yönlendirilecek bir karar yoktur).
    ///   - <see cref="EvaluationResult"/> AwaitingClearing olarak işaretlenir; üst katman
    ///     (EvaluateService) bu işareti görüp satırı <c>ReconciliationStatus.AwaitingClearing</c>
    ///     statüsüne çeker.
    ///   - Clearing dosyası ileride başarıyla ingestion edildiğinde
    ///     <c>ClearingArrivalRequeueService</c> aynı korelasyondaki satırları
    ///     <c>ReconciliationStatus.Ready</c>'ye geri çeker ve değerlendirici tekrar koşar.
    ///
    /// Bu pattern sayesinde clearing gelmeden hatalı/boşa kararlar üretilmez; clearing
    /// ulaşır ulaşmaz aynı satır eksiksiz veriyle baştan değerlendirilir.
    /// </summary>
    private EvaluationResult BuildAwaitingClearingResult(
        EvaluationResult result,
        BkmEvaluationContext context,
        string decisionPoint)
    {
        result.SetNote(_localizer.Get("Reconciliation.Bkm.AwaitingClearingNote"));
        result.MarkAwaitingClearing(decisionPoint);
        result.AddAutoOperation(
            OperationCodes.RaiseAlert,
            _localizer.Get("Reconciliation.Bkm.AwaitingClearingAlertOp"),
            BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, decisionPoint));
        return result;
    }

    // ----------------------------------------------------------------------
    // C2 - Duplicate handling
    // ----------------------------------------------------------------------
    /// <summary>
    /// Aynı işlemin dosyada birden fazla kez geldiği durumlar için karar üretir.
    ///
    /// Statüler:
    ///   - Conflict  : Aynı anahtar fakat ALAN farklı -> hangisi doğru bilinemez.
    ///                 İki kayıt da işlenmez; sadece ALARM ve akış durur (Stop).
    ///   - Secondary : Tüm alanlar Primary ile birebir aynı; mükerrer kayıt.
    ///                 Primary işlenecek; bu kayıt skip + alarm. Akış durur (Stop).
    ///   - Primary   : Tüm alanlar aynı; bu kayıt asıl işlenen. Yine de operatöre görünürlük için
    ///                 ALARM üretilir, ardından akış normal şekilde devam eder (Continue).
    ///   - None      : Duplicate değil; herhangi bir aksiyon üretilmez, akış devam eder.
    /// </summary>
    private DuplicateOutcome HandleDuplicate(EvaluationResult result, BkmEvaluationContext context)
    {
        switch (ResolveDuplicateStatus(context.RootRow))
        {
            case DuplicateStatus.Conflict:
                // FlowComments C2: anahtar aynı ama alanlar farklı -> hiçbiri otomatik işlenmez.
                result.SetNote(_localizer.Get("Reconciliation.Bkm.DuplicateConflictNote"));
                result.AddAutoOperation(
                    OperationCodes.RaiseAlert,
                    _localizer.Get("Reconciliation.Bkm.DuplicateConflictOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C2"));
                return DuplicateOutcome.Stop;

            case DuplicateStatus.Secondary:
                // Tüm alanlar primary ile aynı; bu kayıt skip edilir, primary işlenecek.
                result.SetNote(_localizer.Get("Reconciliation.Bkm.DuplicateEquivalentNote"));
                result.AddAutoOperation(
                    OperationCodes.RaiseAlert,
                    _localizer.Get("Reconciliation.Bkm.DuplicateEquivalentOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C2"));
                return DuplicateOutcome.Stop;

            case DuplicateStatus.Primary:
                // Primary: işlenmesi gereken asıl kayıt. Yine de duplicate olduğunu işaretlemek
                // için alarm üretilir; ana karar ağacına devam edilir.
                result.SetNote(_localizer.Get("Reconciliation.Bkm.DuplicateEquivalentNote"));
                result.AddAutoOperation(
                    OperationCodes.RaiseAlert,
                    _localizer.Get("Reconciliation.Bkm.DuplicateEquivalentOp"),
                    BkmSnapshotBuilder.Create(context, OperationCodes.RaiseAlert, "C2"));
                return DuplicateOutcome.Continue;

            default:
                // Duplicate değil veya bilinmeyen statü -> akışa devam.
                return DuplicateOutcome.Continue;
        }
    }


    // ----------------------------------------------------------------------
    // D7 destek - Cancel/Reversal için orijinal işlemi bul
    // ----------------------------------------------------------------------
    /// <summary>
    /// Cancel/Reversal işlemine ait orijinal payify işlemini Emoney servisi üzerinden çözümler.
    /// MainTxnGuid 0 / negatif ise sorgu yapılmaz (geçersiz referans).
    /// Birden fazla kayıt dönerse en yenisi (TransactionDate, Id desc) seçilir.
    /// </summary>
    private async Task<EmoneyCustomerTransactionDto?> ResolveOriginalTransactionAsync(
        CardBkmDetail detail,
        CancellationToken cancellationToken)
    {
        if (detail.OceanMainTxnGuid <= 0)
        {
            return null;
        }

        var emoneyTransactions = await _emoneyService.GetByCustomerTransactionIdAsync(
            detail.OceanMainTxnGuid.ToString(CultureInfo.InvariantCulture),
            cancellationToken);

        return GetLatestEmoneyTransaction(emoneyTransactions.ToList());
    }

    // ----------------------------------------------------------------------
    // C9 - ACC tarafında bekleyen (ControlStat=P) eşleşme kontrolü
    // ----------------------------------------------------------------------
    /// <summary>
    /// Expire branch'inde C9 kararını verir: clearing detayları arasında ControlStat = Problem
    /// olan VE alan bazında işlemle uyuşan bir kayıt var mı?
    /// True dönerse: ACC operatörü henüz incelemekte; otomatik düzeltme yerine alarm üretilmesi gerekir.
    /// </summary>
    private static Task<bool> HasAccPendingMatchAsync(
        CardBkmDetail detail,
        BkmEvaluationContext context,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var match = context.ClearingDetails.Any(c =>
            c.ControlStat == ClearingBkmControlStat.Problem && IsAccFieldMatch(detail, c));
        return Task.FromResult(match);
    }

    /// <summary>
    /// FlowComments C9 alan eşleşme kuralı:
    /// RRN (her iki tarafta da doluysa), CardNo, ProvisionCode, ARN, MCC,
    /// SourceAmount (2 hane yuvarlama) ve SourceCurrency birebir tutmalı.
    /// RRN nullable kabul edilir; sadece her iki taraf da doluysa karşılaştırılır.
    /// </summary>
    private static bool IsAccFieldMatch(CardBkmDetail card, ClearingBkmDetail clearing)
    {
        // RRN: yalnızca her iki taraf da doluyken karşılaştırılır (nullable tolerans).
        if (!string.IsNullOrWhiteSpace(card.Rrn) && !string.IsNullOrWhiteSpace(clearing.Rrn) &&
            !string.Equals(card.Rrn.Trim(), clearing.Rrn.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(card.CardNo?.Trim(), clearing.CardNo?.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.Equals(card.ProvisionCode?.Trim(), clearing.ProvisionCode?.Trim(),
                StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.Equals(card.Arn?.Trim(), clearing.Arn?.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        // MCC clearing tarafında string; parse edilemiyorsa veya sayısal eşitlik yoksa eşleşme yok.
        if (!int.TryParse(clearing.MccCode?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                out var clearingMcc) || card.Mcc != clearingMcc)
            return false;

        // Tutar karşılaştırması decimal yuvarlama farklarını ezmek için 2 haneye yuvarlanır.
        if (decimal.Round(card.CardHolderBillingAmount, 2, MidpointRounding.AwayFromZero)
            != decimal.Round(clearing.SourceAmount, 2, MidpointRounding.AwayFromZero))
            return false;

        if (card.CardHolderBillingCurrency != clearing.SourceCurrency)
            return false;

        return true;
    }

    // ----------------------------------------------------------------------
    // Yardımcı metotlar
    // ----------------------------------------------------------------------

    /// <summary>
    /// Root card detail önce in-memory cache'den (CardDetails[0]) alınır;
    /// yoksa raw parsed JSON'dan deserialize edilir. Her iki kaynak da yetersizse null döner
    /// (üst katman bunu exception'a çevirir).
    /// </summary>
    private static CardBkmDetail? GetRootCardDetail(BkmEvaluationContext context)
    {
        if (context.CardDetails.Count > 0)
        {
            return context.CardDetails[0];
        }

        return DeserializeDetail(context.RootRow.ParsedContent);
    }

    private static CardBkmDetail? DeserializeDetail(string? parsedData)
    {
        if (string.IsNullOrWhiteSpace(parsedData))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CardBkmDetail>(parsedData, JsonOptions);
        }
        catch
        {
            // Parse hatasını burada yutuyoruz; üst akış null'ı C1 alarmına yönlendirebilir.
            return null;
        }
    }

    /// <summary>En güncel emoney işlemini (TransactionDate, Id desc) döner.</summary>
    private static EmoneyCustomerTransactionDto? GetLatestEmoneyTransaction(
        IReadOnlyList<EmoneyCustomerTransactionDto> transactions)
    {
        return transactions
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();
    }

    /// <summary>
    /// C19 - Payify tarafındaki gerçek işlem statüsünü çözümler.
    ///   - Hiç kayıt yok -> Missing
    ///   - "Failed"      -> Failed
    ///   - "Completed" / "Success" -> Successful
    ///   - Bilinmeyen statü       -> defansif olarak Missing kabul edilir
    ///     (Kural Kodu yalnızca 3 statü tanımlıyor; tanımsızı "yok" gibi ele alıp güvenli tarafta kalıyoruz).
    ///
    /// Birden fazla kayıt varsa en yenisi seçilir (FlowComments C19: son 20 gün arasında arama).
    /// </summary>
    private static PayifyStatus ResolvePayifyStatus(
        IReadOnlyList<EmoneyCustomerTransactionDto> transactions)
    {
        if (transactions.Count == 0) return PayifyStatus.Missing;

        var tx = transactions
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .First();

        var status = tx.TransactionStatus?.Trim();

        if (string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase))
            return PayifyStatus.Failed;

        if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Success", StringComparison.OrdinalIgnoreCase))
            return PayifyStatus.Successful;

        // Defansif: tanımsız statüyü Missing olarak değerlendirip yanlış otomatik düzeltmeyi engelliyoruz.
        return PayifyStatus.Missing;
    }

    /// <summary>
    /// C5 - Dosya tarafındaki işlem statüsünü kategorize eder:
    ///   - TxnStat = Expired                                  -> Expired
    ///   - IsSuccessfulTxn = Successful AND ResponseCode = 00 -> Successful
    ///   - Diğer her şey                                      -> Failed
    /// Her iki kontrolün ŞART olarak birlikte aranması, "00 olmayan başarılı" gibi
    /// tutarsız satırların yanlışlıkla Successful sayılmasını engeller.
    /// </summary>
    private static FileTransactionStatus ResolveFileTransactionStatus(CardBkmDetail detail)
    {
        if (detail.TxnStat == CardBkmTxnStat.Expired)
            return FileTransactionStatus.Expired;

        if (detail.IsSuccessfulTxn == CardBkmIsSuccessfulTxn.Successful && detail.ResponseCode == "00")
            return FileTransactionStatus.Successful;

        return FileTransactionStatus.Failed;
    }

    /// <summary>C3 - İşlem cancel/reversal tipinde mi? (TxnStat = Reverse veya Void).</summary>
    private static bool IsCancelOrReversal(CardBkmDetail detail)
        => detail.TxnStat is CardBkmTxnStat.Reverse or CardBkmTxnStat.Void;

    /// <summary>Successful branch ön koşulu: işlemin TxnSettle = Settled olması gerekir.</summary>
    private static bool IsSettled(CardBkmDetail detail)
        => detail.IsTxnSettle == CardBkmIsTxnSettle.Settled;

    /// <summary>
    /// İşlemin refund tipinde olup olmadığını döner.
    /// BankingTxnCode "Refund" veya "ReferenceRefund" değerleri refund kabul edilir.
    /// </summary>
    private static bool IsRefund(CardBkmDetail detail)
        => string.Equals(detail.BankingTxnCode?.Trim(), "Refund", StringComparison.OrdinalIgnoreCase)
           || string.Equals(detail.BankingTxnCode?.Trim(), "ReferenceRefund", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// C15 - Matched (linked) refund kontrolü:
    /// MainTxnGuid dolu (NULL/0 değil) ve mevcut işlemden farklı bir orijinal işleme işaret ediyorsa
    /// bu refund "bağlı" kabul edilir ve D5 ile otomatik uygulanır.
    /// </summary>
    private static bool IsMatchedRefund(CardBkmDetail detail)
        => detail.OceanMainTxnGuid > 0 && detail.OceanMainTxnGuid != detail.OceanTxnGuid;

    /// <summary>İki tutarın 2 hane yuvarlamadan sonra eşitliğini döner (decimal precision koruması).</summary>
    private static bool AreAmountsEqual(decimal payifyAmount, decimal billingAmount)
        => decimal.Round(payifyAmount, 2, MidpointRounding.AwayFromZero)
           == decimal.Round(billingAmount, 2, MidpointRounding.AwayFromZero);

    /// <summary>D8 ön kontrolü: payify tutarı billing tutarından küçük mü? (Yine 2 hane yuvarlamayla).</summary>
    private static bool IsTransactionAmountLessThanBilling(decimal payifyAmount, decimal billingAmount)
        => decimal.Round(payifyAmount, 2, MidpointRounding.AwayFromZero)
           < decimal.Round(billingAmount, 2, MidpointRounding.AwayFromZero);

    /// <summary>C1 - Dosya satırı parse / length validasyonunu geçemediyse (FileRowStatus.Failed) true döner.</summary>
    private static bool HasFileLengthValidationFailure(BkmEvaluationContext context)
        => context.RootRow.Status == FileRowStatus.Failed;

    /// <summary>
    /// FileIngestion tarafından üretilen DuplicateStatus string değerini içsel enum'a çevirir.
    /// Tanımsız / boş / parse edilemeyen değerler None kabul edilir (akış normal devam eder).
    /// </summary>
    private static DuplicateStatus ResolveDuplicateStatus(IngestionFileLine row)
    {
        if (string.IsNullOrWhiteSpace(row.DuplicateStatus))
        {
            return DuplicateStatus.None;
        }

        if (Enum.TryParse<Domain.Enums.FileIngestion.DuplicateStatus>(row.DuplicateStatus, true, out var parsed))
        {
            return parsed switch
            {
                Domain.Enums.FileIngestion.DuplicateStatus.Conflict  => DuplicateStatus.Conflict,
                Domain.Enums.FileIngestion.DuplicateStatus.Secondary => DuplicateStatus.Secondary,
                Domain.Enums.FileIngestion.DuplicateStatus.Primary   => DuplicateStatus.Primary,
                _                                                    => DuplicateStatus.None
            };
        }

        return DuplicateStatus.None;
    }

    // Kural Kodu'ndaki FlowInput ile birebir karşılıklar.
    // Bu enum'lar yalnızca bu sınıfa özel karar ağacı ifadelerini temsil eder; dışarı sızdırılmaz.
    private enum FileTransactionStatus { Failed, Expired, Successful }
    private enum PayifyStatus { Missing, Successful, Failed }
    private enum DuplicateStatus { None, Primary, Secondary, Conflict }
    private enum DuplicateOutcome { Continue, Stop }
}

