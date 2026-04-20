# LinkPara Card — Konsolide Teknik Dokümantasyon

**Oluşturulma Tarihi:** 16 Nisan 2026  
**Son Güncelleme:** 20 Nisan 2026  
**Kaynak:** `backend.card` repository kaynak kodu + SQL view analizi  
**Dil:** Türkçe (teknik terimler İngilizce ile)  
**Kapsam:** Archive, FileIngestion, Reconciliation, Reporting — tüm endpoint, enum, view, iş kuralı ve operasyonel rehber

> **20 Nisan 2026 değişiklik özeti:**
> - `BkmEvaluator` karar ağacına **clearing-presence gate** eklendi: clearing dosyası ulaşmadıysa karar ertelenir; satır `ReconciliationStatus.AwaitingClearing` statüsüne taşınır ve clearing geldiğinde `ClearingArrivalRequeueService` tarafından otomatik `Ready`'e çevrilerek yeniden değerlendirilir (bkz. §16.3).
> - Operation kodları konsolide edildi: artık yalnızca **16 kanonik kod** vardır; eski draft `RecoverMissingCardRow`, `ApproveAmbiguousPayifyRecord`, `RejectUnmatchedFlow` vb. kodlar kaldırıldı (§4.6).
> - `OperationExecutor`'da ölü kod (`AddInfoAlertAsync`, `ExecuteApplyRefundInternalAsync` wrapper) temizlendi; `MoveCreatedTransactionToExpired` artık ayrı `SUCCESS_MOVE_CREATED_TRANSACTION_TO_EXPIRED` result code'u döner.
> - 34 yetim localization key resx dosyalarından silindi; eksik `Reconciliation.Bkm.SuccessfulMissingNonRefundNote` key'i eklendi.
> - §6 ve §22 enum sözlükleri gerçek `.cs` enum tanımlarıyla bire bir hizalandı.

> **20 Nisan 2026 — yeni view ve endpoint:**
> - **F1 — `reporting.vw_card_clearing_correlation` / `[Reporting].[VwCardClearingCorrelation]`** eklendi: tüm kart detay tabloları (BKM/Visa/MSC, LIVE+ARCHIVE) `UNION ALL` ile birleştirilir; her satır için clearing eşleşmesi `ocean_txn_guid → rrn → arn` öncelik sırasına göre `LEFT JOIN LATERAL` (PostgreSQL) / `OUTER APPLY` (MSSQL) ile aranır. 6 nullable clearing ID döner (BKM/Visa/MSC × LIVE/ARCHIVE).
> - Yeni endpoint: **D28** `GET /v1/Reporting/Reconciliation/CardClearingCorrelation` — filtreler: `DataScope`, `CardTable`, `FileId`, `FileLineId`, `CardId` + pagination. DTO: `CardClearingCorrelationDto`. Handler: `GetCardClearingCorrelationQueryHandler`. Resx: `Handler.Reporting.CardClearingCorrelationFailed`.
>
> **20 Nisan 2026 — ek tutarlılık düzeltmeleri:**
> - `EvaluateService.PersistSuccessfulEvaluationsAsync` **fallback (per-item) yolunda** late-arriving clearing redirect kontrolü eksikti; artık fallback'te de `ResolveRowsWithLateArrivingClearingAsync` çalıştırılıyor ve clearing tamamlanmış satırlar `AwaitingClearing` yerine `Ready`'e çevriliyor (HIGH).
> - `ReviewService.SetDecisionAsync` artık operation lease'ini yalnızca `Status == Blocked || Status == Planned` iken alıyor; daha önce `Executing` durumdaki bir operasyonun lease'ini de paralel olarak alabiliyordu (race düzeltildi, MEDIUM).
> - `ArchiveService` `Limit` ve `MaxFiles` clamp üst sınırı **1.000 → 10.000** olarak güncellendi (validator: `Validation.ArchivePreviewLimitRange` / `ArchiveMaxFilesRange` ile uyumlu).
> - `ArchiveAggregateReader.GetSnapshotAsync` artık `LastUpdate` değerini sadece `IngestionFile.UpdateDate`'ten değil; ilgili `IngestionFileLine` / `ReconciliationEvaluation` / `Operation` / `Review` / `OperationExecution` / `Alert` tablolarındaki en yeni `UpdateDate` ile birleşerek hesaplıyor. Böylece `MIN_LAST_UPDATE_AGE_NOT_REACHED` kuralı çocuk değişikliklerini de görüyor.
> - `ClearingArrivalRequeueService` artık satır `Message` alanını sabit İngilizce string yerine yeni `Reconciliation.RequeuedByClearingArrival` resx key'i üzerinden lokalize ediyor.
> - **Bilinen kısıt:** `Archive` modülünde `Restore` (archive → live geri taşıma) akışı **uygulanmamıştır**; sadece `Preview` ve `Run` mevcut. `FileRowStatus.Processing` enum üyesi tanımlı ancak hiçbir kod yolu bu değeri set etmiyor (claim aşaması doğrudan `Failed`/`Success` yazıyor).

---

## İçindekiler

1. [Zihinsel Model — Sistem Nasıl Çalışır?](#1-zihinsel-model)
2. [Genel Mimari](#2-genel-mimari)
3. [Endpoint Listesi ve Yetki Politikaları](#3-endpoint-listesi)
4. [Endpoint Bazlı Detaylı Analizler](#4-endpoint-bazlı-detaylı-analizler)
- 4.1 [Archive/Preview](#41-archivepreview)
- 4.2 [Archive/Run](#42-archiverun)
- 4.3 [FileIngestion (Body)](#43-fileingestion-body)
- 4.4 [FileIngestion (Route)](#44-fileingestion-route)
- 4.5 [Reconciliation/Evaluate](#45-reconciliationevaluate)
- 4.6 [Reconciliation/Operations/Execute](#46-reconciliationoperationsexecute)
- 4.7 [Reconciliation/Reviews/Approve](#47-reconciliationreviewsapprove)
- 4.8 [Reconciliation/Reviews/Reject](#48-reconciliationreviewsreject)
- 4.9 [Reconciliation/Reviews/Pending](#49-reconciliationreviewspending)
- 4.10 [Reconciliation/Alerts](#410-reconciliationalerts)
- 4.11 [Reporting Endpoint'leri (D1–D28)](#411-reporting-endpointleri)
5. [SQL View Analizi](#5-sql-view-analizi)
6. [Status / Enum Sözlüğü](#6-status--enum-sözlüğü)
7. [State Transition (Durum Geçişi) Diyagramları](#7-state-transition-diyagramları)
8. [DTO → View Mapping](#8-dto--view-mapping)
9. [Business Kuralları ve Teknik Kurallar Kataloğu](#9-business-ve-teknik-kurallar-kataloğu)
10. [Request Parametre Sözlüğü](#10-request-parametre-sözlüğü)
11. [Response Alanları Sözlüğü](#11-response-alanları-sözlüğü)
12. [Rapor Karar Kartları](#12-rapor-karar-kartları)
13. [En Sık Yapılan Hatalar](#13-en-sık-yapılan-hatalar)
14. [Uçtan Uca Senaryo](#14-uçtan-uca-senaryo)
15. [Akış Diyagramları ve Hızlı Referans](#15-akış-diyagramları-ve-hızlı-referans)
16. [Eksik / Doğrulanamayan Noktalar](#16-eksik--doğrulanamayan-noktalar)
17. [Operasyonel Öneriler](#17-operasyonel-öneriler)
18. [Sözlük](#18-sözlük)
19. [Parametre & Konfigürasyon Rehberi](#19-parametre--konfigürasyon-rehberi)
20. [Consumer Yapısı, Tetikleyiciler ve Alert/Notification Mekanizması](#20-consumer-yapısı-tetikleyiciler-ve-alertnotification-mekanizması)
21. [DuplicateStatus — Unique / Primary / Secondary / Conflict](#21-duplicatestatus--unique--primary--secondary--conflict)
22. [Raporlama Referans Kılavuzu](#22-raporlama-referans-kılavuzu)

---

## 1. Zihinsel Model

### 1.1 Büyük Resim

Bu sistem bir **kart ödeme mutabakat platformu**dur. Visa, Mastercard ve BKM ağlarından gelen kart işlem dosyaları ile kliring dosyalarını karşılaştırır, uyuşmazlıkları tespit eder, gerekli aksiyonları planlar ve uygular.

```
📁 DOSYA GELİR         → Kart veya kliring dosyası sisteme yüklenir
     ↓
⚙️  İŞLENİR            → Dosya parse edilir, satırlar veritabanına yazılır
     ↓
🔍 KARAR VERİLİR       → Her satır değerlendirilir: eşleşme var mı? Ne yapılmalı?
     ↓
▶️  UYGULANIR           → Planlanan operasyonlar sırayla çalıştırılır
     ↓
👤 İNSAN GİRER (bazen) → Riskli/belirsiz durumlar için manuel onay istenir
     ↓
📦 ARŞİVLENİR          → Tamamlanan veriler arşiv şemasına taşınır
     ↓
📊 RAPORLANIR          → Tüm süreç 25 farklı raporla izlenir
```

### 1.2 Doğru Endpoint Sırası

```
1. POST /FileIngestion (Card dosyası)
2. POST /FileIngestion (Clearing dosyası)
3. POST /Reconciliation/Evaluate
4. GET  /Reconciliation/Reviews/Pending  (manuel review varsa)
5. POST /Reconciliation/Reviews/Approve veya Reject (manuel review varsa)
6. POST /Reconciliation/Operations/Execute
7. POST /Archive/Preview (kontrol)
8. POST /Archive/Run    (veya AutoArchiveAfterExecute=true ise otomatik)
9. GET  /Reporting/*    (izleme ve raporlama)
```

**Kısa akış:**  
`File → Ingestion → Ready → Evaluate → Operations → Execute → Completed → Archive → Reporting`

### 1.3 Veri Şema Geçişleri

```
LIVE VERİ
  ingestion.file → ingestion.file_line → ingestion.card_*_detail
                                       → ingestion.clearing_*_detail
       │                     ↓
       │           reconciliation.evaluation
       │           reconciliation.operation
       │           reconciliation.operation_execution
       │           reconciliation.review
       │           reconciliation.alert
       │
ARŞİVLEME (Archive/Run)
       ↓
  archive.ingestion_file → archive.ingestion_file_line → archive.card_*_detail
                                                        → archive.clearing_*_detail
  archive.reconciliation_evaluation
  archive.reconciliation_operation
  archive.reconciliation_operation_execution
  archive.reconciliation_review
  archive.reconciliation_alert
  archive.archive_log

RAPORLAMA
  reporting.vw_* (25 SQL view: LIVE + ARCHIVE UNION + data_scope ayrımı)
```

---

## 2. Genel Mimari

```
Controller (API katmanı)
  └─ MediatR → Command/Query
       └─ Handler (Application katmanı)
            └─ Service (Infrastructure katmanı)
                 ├─ CardDbContext (Entity Framework)
                 ├─ Validator (FluentValidation - Pipeline)
                 ├─ AuditStampService
                 ├─ ErrorMapper
                 └─ Dış servisler (EmoneyService, NotificationEmailService)
```

**Temel servis bileşenleri:**

| Bileşen | Sınıf | Katman |
|---------|-------|--------|
| Dosya alımı | `FileIngestionOrchestrator` | Infrastructure |
| Mutabakat değerlendirme | `EvaluateService` | Infrastructure |
| Mutabakat yürütme | `ExecuteService` | Infrastructure |
| Manuel inceleme | `ReviewService` | Infrastructure |
| Uyarı servisi | `AlertService` | Infrastructure |
| Uyarı sorgu servisi | `GetAlertsService` | Infrastructure |
| Geç gelen clearing requeue | `ClearingArrivalRequeueService` | Infrastructure |
| Arşiv servisi | `ArchiveService` | Infrastructure |
| Arşiv yürütücü | `ArchiveExecutor` | Infrastructure |
| Uygunluk değerlendiricisi | `ArchiveEligibilityEvaluator` | Infrastructure |
| Operasyon yürütücü | `OperationExecutor` | Infrastructure |
| Raporlama servisi | `ReportingService` | Infrastructure |

**DB Context:** `CardDbContext` → `Infrastructure/Persistence/CardDbContext.cs`

**DB Şemaları:**
- `ingestion` → file, file_line, card_visa_detail, card_msc_detail, card_bkm_detail, clearing_visa_detail, clearing_msc_detail, clearing_bkm_detail
- `reconciliation` → evaluation, operation, operation_execution, review, alert
- `archive` → tüm yukarıdaki tablolar + archive_log
- `reporting` → 25 SQL view (V1_0_4__ReportingViews.sql)

---

## 3. Endpoint Listesi

| # | Controller | HTTP | Route | Yetki Politikası |
|---|-----------|------|-------|-----------------|
| 1 | ArchiveController | POST | `Archive/Preview` | `Reconciliation:ReadAll` |
| 2 | ArchiveController | POST | `Archive/Run` | `Reconciliation:Delete` |
| 3 | FileIngestionController | POST | `FileIngestion` | `FileIngestion:Create` |
| 4 | FileIngestionController | POST | `FileIngestion/{fileSourceType}/{fileType}/{fileContentType}` | `FileIngestion:Create` |
| 5 | ReconciliationController | POST | `Reconciliation/Evaluate` | `Reconciliation:Create` |
| 6 | ReconciliationController | POST | `Reconciliation/Operations/Execute` | `Reconciliation:Create` |
| 7 | ReconciliationController | POST | `Reconciliation/Reviews/Approve` | `Reconciliation:Update` |
| 8 | ReconciliationController | POST | `Reconciliation/Reviews/Reject` | `Reconciliation:Update` |
| 9 | ReconciliationController | GET | `Reconciliation/Reviews/Pending` | `Reconciliation:ReadAll` |
| 10 | ReconciliationController | GET | `Reconciliation/Alerts` | `Reconciliation:ReadAll` |
| 11–38 | ReportingController | GET | `v1/Reporting/...` (28 endpoint) | `ReportingPolicies.Read` (= `Reconciliation:ReadAll`) |

---

## 4. Endpoint Bazlı Detaylı Analizler

---

### 4.1 Archive/Preview

#### Kimlik

| Özellik | Değer |
|---------|-------|
| HTTP Method | POST |
| Route | `Archive/Preview` |
| Authorize Policy | `Reconciliation:ReadAll` |
| Query | `PreviewArchiveQuery` |
| Handler | `PreviewArchiveQueryHandler` |
| Service | `ArchiveService.PreviewAsync` |

#### Amaç

- **Teknik:** Arşivlenmeye aday dosyaların uygunluk durumunu ve toplam kayıt sayılarını ön izleme olarak döndürür.
- **Business:** Arşiv operasyonu öncesinde hangi dosyaların arşivlenebileceğini ve hangilerinin neden arşivlenemeyeceğini görmek.
- **Süreçteki yeri:** `Archive/Run` öncesi kontrol. Salt okunur (dry-run), veri değiştirmez.

#### Request — `ArchivePreviewRequest`

| Alan | Tip | Zorunlu | Açıklama | Boş gelirse | Yanlış gelirse |
|------|-----|---------|----------|-------------|----------------|
| `IngestionFileIds` | `Guid[]?` | Hayır | Belirli dosyaları hedefler | `null` → tüm adaylar otomatik bulunur | Var olmayan GUID → o aday dönmez, hata vermez |
| `BeforeDate` | `DateTime?` | Hayır | Bu tarihten önce oluşturulanlar | `ArchiveOptions.DefaultBeforeDateStrategy` | `UseConfiguredBeforeDateOnly=true` ise request değeri yok sayılır |
| `Limit` | `int?` | Hayır | Maks aday sayısı. `Math.Clamp(1,10000)` | `DefaultPreviewLimit` (5000) | Negatif → muhtemelen 0 aday |

**Validator:** `PreviewArchiveQueryValidator`
- Request not null; IngestionFileIds: Guid.Empty yok, distinct; Limit: 0–10.000; BeforeDate: gelecek tarih olamaz.
- Controller: `request ?? new ArchivePreviewRequest()` — null request güvenli.

#### Response — `ArchivePreviewResponse`

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Message` | `string?` | null = başarılı; dolu = uyarı/hata |
| `Candidates` | `List<ArchiveCandidateResult>` | Her aday dosya için uygunluk sonucu |
| `Errors` | `List<ArchiveErrorDetail>` | Teknik hatalar |
| `ErrorCount` | `int` | Errors.Count ile eşit |

**`ArchiveCandidateResult`:**

| Alan | Açıklama |
|------|----------|
| `IngestionFileId` | Dosya kimliği |
| `IsEligible` | Arşivlenmeye uygun mu? |
| `FailureReasons` | Uygun değilse neden kodları |
| `Counts` | `ArchiveAggregateCounts` — 13 entity sayısı |

**`ArchiveAggregateCounts` alanları:**  
`IngestionFileCount`, `IngestionFileLineCount`, `IngestionCardVisaDetailCount`, `IngestionCardMscDetailCount`, `IngestionCardBkmDetailCount`, `IngestionClearingVisaDetailCount`, `IngestionClearingMscDetailCount`, `IngestionClearingBkmDetailCount`, `ReconciliationEvaluationCount`, `ReconciliationOperationCount`, `ReconciliationReviewCount`, `ReconciliationOperationExecutionCount`, `ReconciliationAlertCount`

#### Gerçek Çalışma Akışı

1. Controller → `PreviewArchiveQuery` → MediatR
2. `PreviewArchiveQueryHandler` → `ArchiveService.PreviewAsync`
3. `ResolveBeforeDate`: konfigürasyon `RetentionDays` veya request değeri
4. `ResolveEffectiveLimit`: `Math.Clamp(1,10000)`
5. `ArchiveAggregateReader.ResolveCandidateFileIdsAsync` → aday ID'ler
6. Her aday için `GetSnapshotAsync` → `ArchiveEligibilityEvaluator.Evaluate`
7. Response döner

#### Uygunluk Kuralları (ArchiveEligibilityEvaluator)

| Kontrol | Failure Reason |
|---------|----------------|
| `ArchiveOptions.Enabled != true` | `ARCHIVE_DISABLED` |
| Snapshot null | `INGESTION_FILE_NOT_FOUND` |
| `snapshot.ExistsInArchive == true` | `ALREADY_ARCHIVED` |
| `FileCreateDate > now - RetentionDays` (varsayılan 90 gün) | `RETENTION_WINDOW_NOT_REACHED` |
| `LastUpdate > now - MinLastUpdateAgeHours` (varsayılan 72 saat) | `MIN_LAST_UPDATE_AGE_NOT_REACHED` |
| IngestionFile terminal değil | `INGESTION_FILE_NOT_TERMINAL` |
| IngestionFileLine terminal değil | `INGESTION_FILE_LINE_NOT_TERMINAL` |
| FileLine ReconciliationStatus terminal değil | `INGESTION_FILE_LINE_RECONCILIATION_NOT_TERMINAL` |
| ReconciliationEvaluation terminal değil | `RECONCILIATION_EVALUATION_NOT_TERMINAL` |
| ReconciliationOperation terminal değil | `RECONCILIATION_OPERATION_NOT_TERMINAL` |
| ReconciliationReview terminal değil | `RECONCILIATION_REVIEW_NOT_TERMINAL` |
| ReconciliationOperationExecution terminal değil | `RECONCILIATION_OPERATION_EXECUTION_NOT_TERMINAL` |
| ReconciliationAlert terminal değil | `RECONCILIATION_ALERT_NOT_TERMINAL` |

**Terminal Status Varsayılanları:**

| Entity | Terminal Status'lar |
|--------|-------------------|
| IngestionFile | `Success`, `Failed` |
| IngestionFileLine | `Success`, `Failed` |
| IngestionFileLineReconciliation | `Success`, `Failed` |
| ReconciliationEvaluation | `Completed`, `Failed` |
| ReconciliationOperation | `Completed`, `Failed`, `Cancelled` |
| ReconciliationReview | `Approved`, `Rejected`, `Cancelled` |
| ReconciliationOperationExecution | `Completed`, `Failed`, `Skipped` |
| ReconciliationAlert | `Consumed`, `Failed`, `Ignored` |

#### Senaryolar

| Senaryo | Sonuç |
|---------|-------|
| Tüm alanlar boş | RetentionDays stratejisiyle tüm adaylar, varsayılan limit kadar |
| Belirli dosya ID'leri | Sadece o dosyalar değerlendirilir |
| 90 günlük olmayan dosya | `RETENTION_WINDOW_NOT_REACHED` → IsEligible=false |
| İşlemi devam eden satırlar | `*_NOT_TERMINAL` → IsEligible=false |
| Snapshot alınamazsa | `IsEligible=false`, `FailureReasons=["PREVIEW_EVALUATION_FAILED"]` |
| Yetkisiz erişim | 403 |

---

### 4.2 Archive/Run

#### Kimlik

| Özellik | Değer |
|---------|-------|
| HTTP Method | POST |
| Route | `Archive/Run` |
| Authorize Policy | `Reconciliation:Delete` |
| Command | `RunArchiveCommand` |
| Handler | `RunArchiveCommandHandler` |
| Service | `ArchiveService.RunAsync` |

#### Amaç

- **Teknik:** Uygun dosyaları arşiv tablolarına kopyalar ve canlı tablolardan siler.
- **Business:** Saklama süresi dolan, tüm işlemleri tamamlanmış dosyaları arşive taşır.
- **Süreçteki yeri:** `Archive/Preview` sonrası. `AutoArchiveAfterExecute=true` ise Execute sonrası otomatik tetiklenir.
- **⚠️ Geri dönülemez işlemdir.**

#### Request — `ArchiveRunRequest`

| Alan | Tip | Varsayılan | Açıklama |
|------|-----|------------|----------|
| `IngestionFileIds` | `Guid[]?` | null → tüm adaylar | Arşivlenecek belirli dosyalar |
| `BeforeDate` | `DateTime?` | Konfigürasyondan | Bu tarihten önce oluşturulanlar |
| `MaxFiles` | `int?` | `DefaultMaxRunCount` (50000), `Math.Clamp(1,10000)` | İşlenecek max dosya |
| `ContinueOnError` | `bool?` | `false` | Hata sonrası devam edilsin mi? |

**Validator:** `RunArchiveCommandValidator`
- Request not null; IngestionFileIds: no Empty, distinct; MaxFiles: 0–10.000; BeforeDate: gelecek olamaz.

#### Response — `ArchiveRunResponse`

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Message` | `string?` | Özet mesaj |
| `ProcessedCount` | `int` | Archived + Skipped + Failed |
| `ArchivedCount` | `int` | Gerçek başarı |
| `SkippedCount` | `int` | Eligible olmayan / atlanan |
| `FailedCount` | `int` | Teknik başarısızlık |
| `Items` | `List<ArchiveRunItemResult>` | Her dosya için detay |
| `Errors` | `List<ArchiveErrorDetail>` | Genel teknik hatalar |
| `ErrorCount` | `int` | — |

**`ArchiveRunItemResult`:** `IngestionFileId`, `Status` ("Archived" / "Skipped" / "Failed"), `Message`, `FailureReasons`

**`ArchiveErrorDetail`:** `Code`, `Message`, `Detail?`, `Step?`, `IngestionFileId?`, `Severity`

#### Gerçek Çalışma Akışı

1. `AuditStampService.EnsureAuditContext()` — audit bilgisi zorunlu
2. `ArchiveAggregateReader.ResolveCandidateFileIdsAsync` → aday ID'ler
3. Her aday için `ArchiveExecutor.ExecuteAsync` (RepeatableRead isolation):
- `SNAPSHOT_LOAD` → snapshot
- `ELIGIBILITY_CHECK` → Preview ile aynı kurallar
- `LIVE_COUNT_SNAPSHOT` → canlı tablolardaki sayılar
- `ARCHIVE_COPY` → 13 tablo kopyalanır
- `ARCHIVE_COPY_VERIFICATION` → sayı eşleşme kontrolü
- `LIVE_DELETE` → canlı tablolardan ters FK sırasında silme
- `LIVE_DELETE_VERIFICATION` → canlıda kayıt kalmadı mı?
- Transaction commit
4. `ArchiveExecutor.InsertArchiveLogAsync` → `ArchiveLogs` tablosuna log
5. Başarısız + `ContinueOnError=false` → döngü durur
6. Başarısız dosya → `MaxRetryPerFile+1` deneme, `RetryDelaySeconds` bekleme

**Kopyalanan 13 tablo (sırayla):**  
IngestionFile, IngestionFileLine, CardVisaDetail, CardMscDetail, CardBkmDetail, ClearingVisaDetail, ClearingMscDetail, ClearingBkmDetail, ReconciliationEvaluation, ReconciliationOperation, ReconciliationReview, ReconciliationOperationExecution, ReconciliationAlert

#### Hata Davranışları

| Adım | Failure Reason | Davranış |
|------|----------------|----------|
| SNAPSHOT_LOAD | `SNAPSHOT_NOT_FOUND` | Status="Skipped", rollback |
| ELIGIBILITY_CHECK | `ELIGIBILITY_FAILED` | Status="Skipped", rollback |
| ARCHIVE_COPY | `SQL_GENERATION_FAILED` | Status="Failed", rollback |
| ARCHIVE_COPY_VERIFICATION | `ARCHIVE_COPY_COUNT_MISMATCH` | Status="Failed", rollback |
| LIVE_DELETE | `LIVE_DELETE_NOT_CLEARED` | Status="Failed", rollback |
| LIVE_DELETE_VERIFICATION | `LIVE_DELETE_NOT_CLEARED` | Status="Failed", rollback |

#### Status Değişimi

| Kaynak | Önceki | Sonraki |
|--------|--------|---------|
| `ingestion.file.is_archived` | false | true |
| Archive tablolarına | — | INSERT |
| Canlı tablolardan | — | DELETE |

#### Senaryolar

| Senaryo | Sonuç |
|---------|-------|
| Tüm alanlar boş | RetentionDays stratejisiyle çalışır |
| `ContinueOnError=true` | Hatalı atlanır, diğerlerine devam |
| Zaten arşivlenmiş dosya | `ALREADY_ARCHIVED` → Skipped |
| Reconciliation devam eden | Terminal status yok → Skipped |
| Execute sonrası otomatik | `AutoArchiveAfterExecute==true && TotalSucceeded>0` → arka planda (Task.Run) |

---

### 4.3 FileIngestion (Body)

#### Kimlik

| Özellik | Değer |
|---------|-------|
| HTTP Method | POST |
| Route | `FileIngestion` |
| Authorize Policy | `FileIngestion:Create` |
| Command | `FileIngestionCommand` |
| Handler | `FileIngestionCommandHandler` |
| Service | `FileIngestionOrchestrator.IngestAsync` |

#### Amaç

- **Teknik:** Dosyayı okur, parse eder ve veritabanına yazar.
- **Business:** Tüm mutabakat sürecinin başlangıcı. Dosya alınmadan evaluate başlamaz.
- **Süreçteki yeri:** Sürecin ilk adımı.

#### Request — `FileIngestionRequest`

| Alan | Tip | Zorunlu | Açıklama | Boş gelirse | Yanlış gelirse |
|------|-----|---------|----------|-------------|----------------|
| `FileSourceType` | `FileSourceType` | Evet | Remote(1)=FTP/SFTP, Local(2)=disk | Validator reddeder | Deserialize hatası |
| `FileType` | `FileType` | Evet | Card(1) veya Clearing(2) | Validator reddeder | Yanlış detay tablosuna yazılır |
| `FileContentType` | `FileContentType` | Evet | Bkm(1), Msc(2), Visa(3) | Validator reddeder | Parser yanlış çalışır → satırlar Failed |
| `FilePath` | `string` | Koşullu | Local ise zorunlu, Remote ise boş/null | Remote ise config default | Local'de boş → validator reddeder |

**Validator:** `FileIngestionCommandValidator`
- Tüm enum alanları geçerli olmalı; Remote ise FilePath boş/null; Local ise FilePath not empty.
- `FlexibleEnumJsonConverter`: JSON'dan gelen enum değerleri (string veya int) esnek deserialize edilir.

#### Response — `List<FileIngestionResponse>`

(Remote modda birden fazla dosya işlenebilir)

| Alan | Açıklama | Yanlış Yorumlama |
|------|----------|------------------|
| `FileId` | Dosyanın DB kimliği | Hata durumunda Guid.Empty olabilir |
| `FileKey` | Benzersiz anahtar (hash/path bazlı) | FileId ile karıştırılmamalı |
| `FileName` | Dosya adı | — |
| `Status` | Processing(1), Failed(2), Success(3) | Status=Success, satır bazlı hata olabilir (ErrorCount'a bakılmalı) |
| `StatusName` | Status.ToString() | — |
| `Message` | İşlem sonuç mesajı | — |
| `TotalCount` | Toplam satır sayısı | "Başarılı" değil, sadece toplam |
| `SuccessCount` | Başarılı satır | — |
| `ErrorCount` | Hatalı satır + genel hata | Errors.Count ile farklı olabilir (liste kısaltılmış) |
| `Errors` | `List<IngestionErrorDetail>` | — |

**`IngestionErrorDetail`:** `Code`, `Message`, `Detail?`, `Step?`, `LineNumber?`, `FileName?`, `FieldName?`, `RecordType?`, `Severity`

#### Gerçek Çalışma Akışı

1. Controller → `FileIngestionCommand` → MediatR
2. `FileIngestionCommandValidator` (FluentValidation pipeline)
3. `FileIngestionCommandHandler` → `IFileIngestionService.IngestAsync`
4. `FileIngestionOrchestrator.IngestAsync`:
- Remote → FTP/SFTP; Local → dosya yolu
- Sabit genişlikli kayıt ayrıştırma (`IFixedWidthRecordParser`)
- Kayıtlar modele eşlenir (`IParsedRecordModelMapper`)
- Entity'lere dönüştürülür (`IIngestionDetailEntityMapper`)
- Bulk insert ile DB'ye yazılır
5. Handler exception → Status=Failed

#### Veri ve Status Değişimi

| Entity | Önceki | Sonraki |
|--------|--------|---------|
| `ingestion.file` | yok | Processing → Success/Failed |
| `ingestion.file_line` | yok | Processing → Success/Failed |
| `ingestion.file_line.reconciliation_status` | yok | Ready (başarılı satırlar) |
| `ingestion.file_line.duplicate_status` | yok | Unique/Primary/Secondary/Conflict |
| Detail tablolar | yok | INSERT (network'e göre card veya clearing) |

---

### 4.4 FileIngestion (Route)

#### Kimlik

| Özellik | Değer |
|---------|-------|
| HTTP Method | POST |
| Route | `FileIngestion/{fileSourceType}/{fileType}/{fileContentType}` |
| Authorize Policy | `FileIngestion:Create` |

#### Farklılık

Parametreler URL route'undan alınır. Body'de sadece `FileIngestionRouteRequest` (tek alan: `FilePath`) gönderilir.

| Route Alanı | Boş | Yanlış |
|-------------|-----|--------|
| `fileSourceType` | 404 | 400 Bad Request |
| `fileType` | 404 | 400 Bad Request |
| `fileContentType` | 404 | 400 Bad Request |

Davranış, response ve validator: Body endpoint (4.3) ile aynı.

---

### 4.5 Reconciliation/Evaluate

#### Kimlik

| Özellik | Değer |
|---------|-------|
| HTTP Method | POST |
| Route | `Reconciliation/Evaluate` |
| Authorize Policy | `Reconciliation:Create` |
| Command | `EvaluateCommand` |
| Handler | `EvaluateCommandHandler` |
| Service | `ReconciliationService.EvaluateAsync` → `EvaluateService.EvaluateAsync` |

#### Amaç

- **Teknik:** `Ready` durumdaki dosya satırlarını değerlendirir, her satır için operasyon planı oluşturur.
- **Business:** Kart ile kliring eşleştirmesi yapar, uyuşmazlıkları tespit eder, düzeltme operasyonları planlar.
- **Süreçteki yeri:** FileIngestion sonrası, Execute öncesi. Sadece `Ready` satırları hedefler.

#### Request — `EvaluateRequest`

| Alan | Tip | Varsayılan | Açıklama |
|------|-----|------------|----------|
| `IngestionFileIds` | `Guid[]` | Boş → tüm `Ready` satırlar | Değerlendirilecek dosya ID'leri |
| `Options` | `EvaluateOptions?` | Konfigürasyondan | Override parametreleri |

**`EvaluateOptions`:**

| Alan | Varsayılan | Aralık | Açıklama |
|------|------------|--------|----------|
| `ChunkSize` | 50.000 | 100–10.000 | Her seferde işlenecek satır |
| `ClaimTimeoutSeconds` | 1.800 (30dk) | 30–3.600 | Claim zaman aşımı |
| `ClaimRetryCount` | 5 | 1–10 | Claim başarısızsa kaç kez denenecek |
| `OperationMaxRetries` | 5 | 0–50 | Oluşturulan operasyonların max retry'ı |

**Validator:** `EvaluateCommandValidator` — Request not null; IngestionFileIds: no Empty, distinct; Options aralıkları.  
**Null request:** `request ?? new EvaluateRequest()` — güvenli.

#### Response — `EvaluateResponse`

| Alan | Açıklama |
|------|----------|
| `EvaluationRunId` | Bu çalıştırmanın benzersiz kimliği (= GroupId) |
| `CreatedOperationsCount` | Oluşturulan operasyon sayısı |
| `SkippedCount` | Değerlendirilemeyen satır sayısı |
| `Message` | Sonuç mesajı |
| `ErrorCount` / `Errors` | Teknik hatalar |

#### Gerçek Çalışma Akışı

1. **Hedef çözümü:** `IngestionFileIds` boşsa `ReconciliationStatus=Ready` olan tüm dosyalar
2. **Chunk bazlı işleme:** Her dosya için:
- `ClaimReadyChunkAsync` → `Serializable` isolation, `EVAL_CLAIM:{GUID}` marker ile satırlar Processing'e çekilir
3. **Context oluşturma:** `IContextBuilder.BuildManyAsync` — her satır için kart, takas, Emoney bağlamı
4. **Değerlendirme:** `EvaluatorResolver.Resolve(contentType)` → BKM/Visa/MSC evaluator → `EvaluationResult`
5. **Persistence:** `PersistSuccessfulEvaluationsAsync`:
- `ReconciliationEvaluation` (Status=Completed)
- `ReconciliationOperation` (her operasyon)
- `ReconciliationReview` (manuel operasyonlar için, Decision=Pending)
- Satır → ReconciliationStatus=Success
- Batch başarısız → tek tek kayıt (fallback)
6. **Hata:** Evaluation=Failed, Alert oluşturulur, satır=Failed, skippedCount++

#### Operasyon Planlama Detayları

- İlk operasyon → `SequenceIndex=0`, `Status=Planned`
- Sonraki → `Status=Blocked`
- `Branch="Approve"/"Reject"` → `ParentSequenceIndex` manuel gate'e işaret eder
- `IsManual=true` → `ReconciliationReview` oluşturulur: `Decision=Pending`, `ExpiresAt=Now+ReviewTimeout`, `ExpirationAction=Cancel`, `ExpirationFlowAction=Continue`
- `IdempotencyKey` = `{Code}:{FileLineId}:{SequenceIndex}:{correlationValue}:...`

#### Claim Mekanizması

- `Serializable` isolation → concurrent evaluate çakışması önlenir
- Stale claim: `UpdateDate <= now - ClaimTimeoutSeconds` olan Processing satırlar yeniden alınabilir
- Claim yenileme: `RefreshClaimAsync` ile `UpdateDate` güncellenir
- Claim retry: `DbUpdateException`/`InvalidOperationException` → `ClaimRetryCount` kadar tekrar

#### Veri Etkisi

| Tablo | İşlem |
|-------|-------|
| `IngestionFileLines` | UPDATE: `ReconciliationStatus`, `Message`, `UpdateDate` |
| `ReconciliationEvaluations` | INSERT |
| `ReconciliationOperations` | INSERT |
| `ReconciliationReviews` | INSERT (sadece manuel) |
| `ReconciliationAlerts` | INSERT (sadece hata) |
| `IngestionFile` | UPDATE: `LastProcessedLineNumber`, `LastProcessedByteOffset` |

#### Senaryolar

| Senaryo | Sonuç |
|---------|-------|
| Boş request | Tüm Ready satırlar |
| Belirli dosyalar | Sadece o dosyalardaki Ready satırlar |
| Hiç Ready yok | `CreatedOperationsCount=0`, `SkippedCount=0` |
| Concurrent çağrılar | Serializable isolation ile çakışma önlenir |
| Stale claim kurtarma | ClaimTimeoutSeconds sonra başka çağrı devralır |

---

### 4.6 Reconciliation/Operations/Execute

#### Kimlik

| Özellik | Değer |
|---------|-------|
| HTTP Method | POST |
| Route | `Reconciliation/Operations/Execute` |
| Authorize Policy | `Reconciliation:Create` |
| Command | `ExecuteCommand` |
| Handler | `ExecuteCommandHandler` |
| Service | `ReconciliationService.ExecuteAsync` → `ExecuteService.ExecuteAsync` |

#### Amaç

- **Teknik:** Evaluate aşamasında oluşturulan operasyonları sırayla yürütür.
- **Business:** Otomatik düzeltme işlemlerini gerçekleştirmek ve manuel kararları işlemek.
- **Süreçteki yeri:** Evaluate sonrası.

#### Request — `ExecuteRequest`

| Alan | Tip | Açıklama |
|------|-----|----------|
| `GroupIds` | `Guid[]` | Evaluate run ID bazında filtreleme |
| `EvaluationIds` | `Guid[]` | Belirli evaluation'lar |
| `OperationIds` | `Guid[]` | En dar kapsam: belirli operasyonlar |
| `Options` | `ExecuteOptions?` | Override parametreleri |

**Seçim önceliği:** `OperationIds` > `EvaluationIds` > `GroupIds` > All (hepsi boşsa tüm Planned)

**`ExecuteOptions`:**

| Alan | Varsayılan | Aralık |
|------|------------|--------|
| `MaxEvaluations` | 500.000 | 1–100.000 (validator) |
| `LeaseSeconds` | 900 (15dk) | 1–3.600 (validator) |

#### Response — `ExecuteResponse`

| Alan | Açıklama |
|------|----------|
| `TotalAttempted` | Denenen toplam |
| `TotalSucceeded` | Completed + Skipped (idempotent atlamalar dahil) |
| `TotalFailed` | Başarısız |
| `Results` | `List<OperationExecutionResult>` — sadece başarısız sonuçlar |

**`OperationExecutionResult`:** `OperationId`, `Status` ("Completed"/"Skipped"/"Failed"/"Blocked"), `Message`

#### Gerçek Çalışma Akışı

1. `ResolveTargetEvaluationIdsAsync` → Planned/Blocked/Executing operasyonu olan evaluation'lar
2. Her evaluation → `ExecuteEvaluationAsync`:
- `LoadEvaluationOperationWindowAsync`
- `GetNextOperation` → çalıştırılacak operasyonu belirler
- `TryClaimOperationAsync` → lease ile kilitleme
- `ExecuteOperationAsync` → operasyon koduna göre dallanır

3. **GetNextOperation mantığı:**
- Completed/Cancelled/Failed → atlanır
- Executing + lease dolmamış → null (bekle)
- Executing + lease dolmuş → devralnabilir
- Blocked + parent Completed + öncekiler terminal → promote
- Planned + NextAttemptAt ve LeaseExpiresAt → kontrol

4. **Manuel Gate (CreateManualReview):**
- Decision=Pending + expire → `ApplyExpirationDecision`
- Decision=Pending → operasyon Blocked, execution Skipped (WAITING_MANUAL_DECISION)
- Decision=Approved → gate Completed; Approve branch → Blocked (çalıştırılabilir); Reject branch → Cancelled
- Decision=Rejected → gate Completed; Reject branch → Blocked; Approve branch → Cancelled
- Decision=Cancelled → gate Cancelled; `CancelRemaining` ise tüm branch'ler Cancelled

5. **Retry:** `RetryCount += 1`. `RetryCount >= MaxRetries` → Failed. Değilse Planned + `NextAttemptAt = now + (30s × 2^RetryCount)` (exponential backoff)

6. **Idempotency:** Aynı `IdempotencyKey` ile başka operasyon Completed → Skipped (`SKIPPED_ALREADY_APPLIED`)

7. **Alert:** Operasyon Failed → `ReconciliationAlert` (`AlertType="OperationExecutionFailed"`)

8. **Alert Servisi:** `AlertService.ExecuteAsync` → pending/failed alert'ler e-posta

9. **Auto Archive:** `AutoArchiveAfterExecute==true && TotalSucceeded>0` → arka planda `RunArchiveCommand`

#### Operasyon Kodları

> Tüm operasyon kodlarının kanonik tanımı: [`OperationCodes.cs`](LinkPara.Card.Infrastructure/Services/Reconciliation/Execute/Core/OperationCodes.cs).
> Her kod, `OperationExecutor.ExecuteAsync` switch içinde **tek bir handler**'a karşılık gelir.

| Kod | Branch / Karar Noktası | Açıklama | Dış Servis |
|-----|------------------------|----------|-----------|
| `RaiseAlert` | C1, C2, C10, AwaitingClearing, defansif | Reconciliation alert kaydı oluşturur | Hayır |
| `CreateManualReview` | D6 (eşleniksiz iade) | Manuel review gate; finansal etki yok | Hayır |
| `ReverseOriginalTransaction` | D7 (cancel/reversal) | Orijinal işlemi iptal eder + bakiye etkisini ters çevirir (tek handler içinde sıralı) | `EmoneyService.UpdateTransactionStatusAsync` + `ReverseBalanceEffectAsync` |
| `CorrectResponseCode` | D1, D3 | İşlemin response code'unu düzeltir | `EmoneyService.CorrectResponseCodeAsync` |
| `ConvertTransactionToFailed` | D1 | İşlem statüsünü Failed'a çeker | `EmoneyService.UpdateTransactionStatusAsync` |
| `ConvertTransactionToSuccessful` | D3 | İşlem statüsünü Completed'a çeker | `EmoneyService.UpdateTransactionStatusAsync` |
| `ReverseByBalanceEffect` | D1, D2, D3 | Bakiye etkisini ters kayıtla geri alır | `EmoneyService.ReverseBalanceEffectAsync` |
| `MoveTransactionToExpired` | C7, D2 | Mevcut işlemi Expired statüsüne taşır | `EmoneyService.ExpireTransactionAsync` |
| `CreateTransaction` | C8, C13 | Eksik Payify işlemini oluşturur (wallet binding zorunlu) | `EmoneyService.CreateTransactionAsync` |
| `MoveCreatedTransactionToExpired` | C8 | `CreateTransaction` sonrası oluşturulan işlemi Expired'e taşır | `EmoneyService.ExpireTransactionAsync` |
| `ApplyOriginalEffectOrRefund` | D4 | Orijinaline göre effect veya refund uygular | `EmoneyService.RefundTransactionAsync` |
| `ApplyLinkedRefund` | D5 (matched refund) | Eşlenikli iade işlemini uygular | `EmoneyService.RefundTransactionAsync` |
| `ApplyUnlinkedRefundEffect` | D6 approve | Manuel onaydan sonra eşleniksiz iade etkisini uygular | `EmoneyService.RefundTransactionAsync` |
| `StartChargeback` | D6 reject | Chargeback sürecini başlatır | `EmoneyService.InitChargebackAsync` + `ApproveChargebackAsync` |
| `InsertShadowBalanceEntry` | D8 (amount < billing) | Gölge bakiye için fark kaydı | `EmoneyService.CreateShadowBalanceDebtCreditAsync` |
| `RunShadowBalanceProcess` | D8 | Gölge bakiye işleme sürecini çalıştırır | `EmoneyService.RunShadowBalanceProcessAsync` |

> **Not:** Önceki tasarımda yer alan `RecoverMissingCardRow`, `DropMissingCardRow`, `ApproveAmbiguousPayifyRecord`, `RejectAmbiguousPayifyRecord`, `ApproveUnmatchedFlow`, `RejectUnmatchedFlow`, `BindOriginalTransactionAndContinue`, `RejectReversalRecord`, `ApprovePendingAccReview`, `RejectPendingAccReview`, `ApproveMissingPayifyTransaction`, `RejectMissingPayifyTransaction`, `MarkOriginalTransactionCancelled` kodları **kaldırılmıştır**. Manuel review yalnızca **D6 eşleniksiz iade** branch'inde üretilir; satır seviyesinde "queue'ya geri al" gereksinimi `EvaluationResult.MarkAwaitingClearing` ile karşılanır ve `ClearingArrivalRequeueService` tarafından clearing dosyası geldiğinde otomatik olarak `Ready`'ye çevrilir (bkz. §16.3 ve §6).

#### Veri Etkisi

| Tablo | İşlem |
|-------|-------|
| `ReconciliationOperations` | UPDATE: Status, RetryCount, NextAttemptAt, LeaseOwner, LeaseExpiresAt, LastError |
| `ReconciliationOperationExecutions` | INSERT |
| `ReconciliationReviews` | UPDATE: expire durumunda Decision |
| `ReconciliationAlerts` | INSERT |
| `IngestionFileLines` | UPDATE: ReconciliationStatus=Ready (RecoverMissingCardRow vb.) |

---

### 4.7 Reconciliation/Reviews/Approve

#### Kimlik

| Özellik | Değer |
|---------|-------|
| HTTP Method | POST |
| Route | `Reconciliation/Reviews/Approve` |
| Authorize Policy | `Reconciliation:Update` |
| Command | `ApproveCommand` → `ReviewService.ApproveAsync` |

#### Request — `ApproveRequest`

| Alan | Zorunlu | Açıklama |
|------|---------|----------|
| `OperationId` | Evet | Onaylanacak operasyon. Guid.Empty olamaz |
| `ReviewerId` | Hayır | İnceleyici kimliği; null ise audit context'ten türetilir |
| `Comment` | Hayır | Max 2000 karakter |

#### Response — `ApproveResponse`

| Alan | Olası Değerler |
|------|---------------|
| `OperationId` | İşlenen operasyon kimliği |
| `Result` | `"Approved"`, `"Failed"`, `"NotFound"`, `"Invalid"` |
| `Message` | — |

#### Gerçek Çalışma Akışı

1. `ReviewService.ApproveAsync` → `SetDecisionAsync(operationId, reviewerId, Approved, comment)`
2. Transaction açılır
3. `ReconciliationReviews`'de `OperationId` + `Decision=Pending` kaydı → `Decision=Approved`, `ReviewerId`, `Comment`, `DecisionAt`
4. Güncellenen satır yoksa:
- Review hiç yok → `Result="NotFound"`
- Review var ama Pending değil → `Result="Invalid"` (REVIEW_ALREADY_FINALIZED)
5. Review güncellenidiyse: `ReconciliationOperation.NextAttemptAt=now`, `LeaseExpiresAt=null`
6. Operation güncellenemezse (terminal) → `Result="Invalid"` (REVIEW_OPERATION_NOT_REQUEUEABLE)
7. Transaction commit

#### Status Değişimi

| Entity | Önceki | Sonraki |
|--------|--------|---------|
| `reconciliation.review` | Decision=Pending | Decision=Approved, DecisionAt=now |
| `reconciliation.operation` | Blocked | Planned |

---

### 4.8 Reconciliation/Reviews/Reject

Approve ile aynı yapı; `ReviewDecision.Rejected` çalışır.

**Kritik fark:** Reject'te `Comment` **zorunludur** (NotEmpty, max 2000 karakter).

#### Status Değişimi

| Entity | Önceki | Sonraki |
|--------|--------|---------|
| `reconciliation.review` | Decision=Pending | Decision=Rejected, DecisionAt=now |
| `reconciliation.operation` | Blocked | Cancelled |
| Reject branch operasyonları | yok | Planned |

---

### 4.9 Reconciliation/Reviews/Pending

#### Kimlik

| Özellik | Değer |
|---------|-------|
| HTTP Method | GET |
| Route | `Reconciliation/Reviews/Pending` |
| Authorize Policy | `Reconciliation:ReadAll` |
| Query | `GetPendingReviewsQuery` |
| Service | `ReviewService.GetPendingAsync` |

#### Request

| Alan | Açıklama |
|------|----------|
| `Date` | `DateOnly?` — review oluşturma tarihi filtresi. Gelecek tarih olamaz. |
| `Page` | `Math.Max(1)` |
| `Size` | `Math.Clamp(1,1000)` |
| `SortBy` | Sıralama alanı |

#### Response — `GetPendingReviewsResponse`

`Data: PaginatedList<ManualReview>`

**`ManualReview` alanları:**

| Alan | Açıklama |
|------|----------|
| `OperationId` | Approve/Reject bu ID ile yapılır |
| `FileLineId` | İlişkili dosya satırı |
| `OperationCode` | Operasyon kodu (örn: `CreateManualReview`) |
| `OperationPayload` | Branch operasyonları JSON |
| `CreatedAt` | Oluşturma zamanı |
| `ApproveBranchOperations` | Onay sonrası çalışacak branch'ler |
| `RejectBranchOperations` | Red sonrası çalışacak branch'ler |
| `ExpiresAt` | Süre sonu; null = expire etmez |
| `ExpirationAction` | Cancel/Approve/Reject |
| `ExpirationFlowAction` | Continue/CancelRemaining |
| `ApprovalMessage` | Onay mesaj şablonu |
| `RejectionMessage` | Red mesaj şablonu |

#### Gerçek Çalışma Akışı

1. `ReconciliationReviews`'den `Decision=Pending` filtrelenir
2. `Date` filtresi → `CreateDate` gün bazında
3. `ReconciliationOperations` ile JOIN
4. Branch operasyonları ayrıca yüklenir (`ParentSequenceIndex` bazında)
5. `CreateDate` ASC sayfalanır

---

### 4.10 Reconciliation/Alerts

#### Kimlik

| Özellik | Değer |
|---------|-------|
| HTTP Method | GET |
| Route | `Reconciliation/Alerts` |
| Authorize Policy | `Reconciliation:ReadAll` |
| Service | `GetAlertsService.GetAsync` |

#### Request

| Alan | Açıklama |
|------|----------|
| `Date` | `DateOnly?` — alert oluşturma tarihi. Gelecek olamaz |
| `AlertStatus` | Pending=0, Processing=1, Consumed=2, Failed=3, Ignored=4 |
| `Page` / `Size` | Max size: 1000 |

#### Response — `GetAlertsResponse`

`Data: PaginatedList<Alert>`

**`Alert` alanları:**

| Alan | Açıklama |
|------|----------|
| `Id` | Alert kimliği |
| `FileLineId` | İlişkili dosya satırı |
| `GroupId` | Evaluate run ID'si |
| `EvaluationId` | İlişkili evaluation |
| `OperationId` | İlişkili operasyon — `Guid.Empty` olabilir (evaluation seviyesi alert) |
| `Severity` | `"Error"` veya `"Info"` |
| `AlertType` | `"EvaluationFailed"`, `"OperationExecutionFailed"` veya operasyon kodu |
| `Message` | Alert mesajı |
| `CreatedAt` | Oluşturma zamanı |

**Sıralama:** `CreateDate` DESC. Sayfalama uygulanır.

---

### 4.11 Reporting Endpoint'leri

Tüm reporting endpoint'leri:
- **Controller:** `ReportingController` (`Route: v1/Reporting`)
- **Policy:** `Reconciliation:ReadAll`
- **HTTP:** GET
- **Veri:** `ReportingService` → `CardDbContext.Set<TDto>()` — her DTO bir DB view'ına eşlenir
- **Veri etkisi:** Hiçbiri — tümü salt okunur
- **Hata:** Exception → handler yakalanır, `ErrorCount=1`, mesaj döner

> **ÖNEMLİ:** View SQL'leri kaynak kodda değil, veritabanı migration'larında (`V1_0_4__ReportingViews.sql`) tanımlıdır.

---

#### D1. GET /Reporting/Ingestion/FileOverview

**Amaç:** Dosya bazında alım durumu ve satır başarı oranları.  
**View:** `reporting.vw_ingestion_file_overview`

**Filtreler:** `DataScope`, `ContentType`, `FileType`, `FileStatus`, `DateFrom`/`DateTo` (`FileCreatedAt`), pagination  
**Sıralama:** `FileCreatedAt` DESC

**Kritik DTO Alanları:**

| Alan | Kaynak | Uyarı |
|------|--------|-------|
| `LineSuccessRatePct` | file tablosu sayaçları | `actual_success_line_count` (file_line sayımı) ile farklı olabilir |
| `CompletenessPct` | `processed/expected` | expected=0 → null/0 döner; "başarılı" değil, "henüz güncellenmedi" |
| `ProcessingDurationSeconds` | `update_date - create_date` | update_date yoksa 0 döner; "hızlı" değil, "henüz güncellenmedi" |
| `DataScope` | 'LIVE'/'ARCHIVE' | DataScope filtresiz sorguda aynı dosya iki kez görünebilir |
| `ReconReadyCount` | reconciliation_status=Ready | — |
| `DuplicateLineCount` | Secondary+Conflict | — |

---

#### D2. GET /Reporting/Ingestion/FileQuality

**Amaç:** Dosya kalite analizi — duplicate oranı, hata oranı, retry sayıları.  
**View:** `reporting.vw_ingestion_file_quality`

**Filtreler:** D1 ile aynı

**Kritik DTO Alanları:**

| Alan | Kaynak | Uyarı |
|------|--------|-------|
| `ErrorRatePct` | `vw_ingestion_file_quality` | D1'deki `LineFailRatePct` file tablosu sayacından → farklı sonuç verebilir |
| `DuplicateImpactPct` | (Secondary+Conflict)/total | Primary dahil değil; "sorunlu duplicate oranı" |
| `DuplicatePrimaryCount` | duplicate_status=Primary | İşleme alınan kopya |
| `TotalRetryCount` | tüm satırların retry toplamı | `LinesWithRetryCount` ile karıştırılmamalı |
| `LinesWithRetryCount` | en az 1 retry olan satır sayısı | — |

---

#### D3. GET /Reporting/Ingestion/DailySummary

**Amaç:** Günlük bazda ingestion trend analizi.  
**View:** `reporting.vw_ingestion_daily_summary`  
**Filtreler:** `DataScope`, `ContentType`, `FileType`, `DateFrom`/`DateTo` (ReportDate). Pagination **yok**.

| Alan | Uyarı |
|------|-------|
| `ReportDate` | `create_date`'in günü — işlenme günü değil |
| `ProcessedLineSuccessRatePct` | expected ile değil, processed ile oranlanır |
| `ProcessingFileCount > 0` | sayılar henüz final değil |

---

#### D4. GET /Reporting/Ingestion/NetworkMatrix

**Amaç:** ContentType × FileType matris özeti. Tüm zamanları kapsar.  
**View:** `reporting.vw_ingestion_network_matrix`  
**Filtre:** Sadece `DataScope`. Pagination **yok**.

| Alan | Uyarı |
|------|-------|
| `LastFileAt` | Çok eski → o network'ten dosya gelmiyor olabilir |
| `FileCount` | Tarih yok → tüm zamanların toplamı. Trend göstermez |

---

#### D5. GET /Reporting/Ingestion/ExceptionHotspots

**Amaç:** En sorunlu dosyalar — severity seviyesiyle.  
**View:** `reporting.vw_ingestion_exception_hotspots`  
**Filtreler:** `DataScope`, `ContentType`, `FileType`, `SeverityLevel`, `DateFrom`/`DateTo`, pagination

**Severity Hesabı (SQL CASE):**
- `CRITICAL` → file.status='Failed'
- `HIGH` → hata oranı ≥ %20
- `MEDIUM` → en az 1 hatalı satır
- `LOW` → neredeyse imkansız (WHERE filtresi zaten `failed_line_count>0` veya `status=Failed` bekliyor)

| Alan | Uyarı |
|------|-------|
| `SeverityLevel` | Sabit değil — eşik değerleri SQL'de hardcoded; %20 değişirse SQL değişmeli |
| `MaxRetryCount` | Tek satırın retry'ı; `TotalRetryCount` ile karıştırılmamalı |
| `DistinctErrorMessageCount` | Sadece status='Failed' satırların mesajları |

---

#### D6. GET /Reporting/Reconciliation/DailyOverview

**Amaç:** Günlük mutabakat süreç özeti.  
**View:** `reporting.vw_recon_daily_overview`  
**Filtreler:** `DataScope`, `DateFrom`, `DateTo`. Pagination **yok**.

| Alan | Uyarı |
|------|-------|
| `OperationSuccessRatePct` | Completed/Total — Blocked ve Planned paydada. %100 = "hiç iş kalmadı" değil |
| `AvgExecutionDurationSeconds` | Sadece `finished_at IS NOT NULL` olanlar dahil |
| Execution tarihi | `started_at`'e göre; diğerleri `create_date`'e göre — aynı gün farklı anlam |

---

#### D7. GET /Reporting/Reconciliation/OpenItems

**Amaç:** Henüz tamamlanmamış (Planned/Blocked/Executing) operasyonlar.  
**View:** `reporting.vw_recon_open_items` (sadece LIVE)  
**Filtreler:** `OperationStatus`, `Branch`, `IsManual`, pagination

| Alan | Uyarı |
|------|-------|
| `AgeHours` | `NOW()` bazlı → her sorguda farklı |
| `IsManual=true + Blocked` | Normal — onay bekliyor |
| `IsManual=false + Blocked` | Bağımlılık sorunu — farklı bir sorun |

---

#### D8. GET /Reporting/Reconciliation/OpenItemAging

**Amaç:** Açık operasyonların yaş bucket dağılımı.  
**View:** `reporting.vw_recon_open_item_aging` (sadece LIVE, Planned/Blocked/Executing)  
**Parametre yok.**

Bucket'lar: `0-1h`, `1-4h`, `4-24h`, `1-3d`, `3-7d`, `7d+`

| Alan | Uyarı |
|------|-------|
| `ManualCount` | Yüksek = "sorun" değil — sadece insan kararı bekliyor |
| `ManualCount + BlockedCount` | Kesişebilir — her ikisi aynı operasyona sayılabilir |

---

#### D9. GET /Reporting/Reconciliation/ManualReviewQueue

**Amaç:** Manuel onay kuyruğu — reviewer'ın çalışma ekranı. En büyük ve detaylı view.  
**View:** `reporting.vw_recon_manual_review_queue`  
**Filtreler:** `UrgencyLevel`, `OperationBranch`, pagination

**Urgency Hesabı (SQL CASE, öncelik sırası):**
- `EXPIRED` → expires_at < NOW()
- `EXPIRING_SOON` → expires_at < NOW() + 4 saat
- `OVERDUE` → 24 saatten fazla bekliyor
- `NORMAL` → diğerleri

| Alan | Uyarı |
|------|-------|
| `EffectiveError` | `COALESCE(son execution hatası, operation hatası, NULL)` — gerçek kök neden alt seviyede olabilir |
| `CardOriginalAmount` / `ClearingSourceAmount` | Farklı para birimleri olabilir — doğrudan karşılaştırılamaz |
| `ExpiresAt` yakın | Otomatik expire → `ExpirationAction` istenmeyen kararı alabilir |

---

#### D10. GET /Reporting/Reconciliation/AlertSummary

**Amaç:** Alert'ler severity × AlertType × Status bazında.  
**View:** `reporting.vw_recon_alert_summary`  
**Filtreler:** `DataScope`, `Severity` (string), `AlertType` (string), `AlertStatus`

| Alan | Uyarı |
|------|-------|
| `AlertCount` | Etkilenen operasyon sayısı değil — bir operasyonda birden fazla alert olabilir |
| `DistinctGroupCount` | Etkilenen farklı evaluation grubu — yaygınlık tespiti için kullanılır |

---

#### D11. GET /Reporting/Reconciliation/LiveCardContentDaily

**Amaç:** LIVE kart satırlarının günlük içerik analizi.  
**View:** `reporting.vw_recon_live_card_content_daily`  
**Filtreler:** `Network`, `DateFrom`, `DateTo`, pagination

**⚠️ INNER JOIN LATERAL:** Card detayı olmayan file_line'lar view'da görünmez.  
15 boyutlu GROUP BY (tarih, network, line_status, recon_status, financial_type, txn_effect, txn_source, txn_region, terminal_type, channel_code, is_txn_settle, txn_stat, response_code, is_successful_txn, original_currency).

| Alan | Uyarı |
|------|-------|
| `IsTxnSettle` / `IsSuccessfulTxn` | String "Y"/"N", boolean değil |
| `OriginalCurrency` | int (949=TRY, 840=USD vb.), string değil |

---

#### D12. GET /Reporting/Reconciliation/LiveClearingContentDaily

**Amaç:** LIVE kliring satırlarının günlük içerik analizi.  
**View:** `reporting.vw_recon_live_clearing_content_daily`  
**Filtreler:** `Network`, `DateFrom`, `DateTo`, pagination

GROUP BY: tarih, network, line_status, recon_status, txn_type, io_flag, control_stat, source_currency  
**⚠️ INNER JOIN LATERAL** — clearing detayı yoksa satır düşer.

---

#### D13. GET /Reporting/Reconciliation/ArchiveCardContentDaily

D11'in archive versiyonu. Kaynak: `reporting.vw_recon_archive_card_content_daily`

---

#### D14. GET /Reporting/Reconciliation/ArchiveClearingContentDaily

D12'nin archive versiyonu. Kaynak: `reporting.vw_recon_archive_clearing_content_daily`

---

#### D15. GET /Reporting/Reconciliation/ContentDaily

**Amaç:** Card + Clearing birleşik, LIVE+ARCHIVE, günlük.  
**View:** `reporting.vw_recon_content_daily` (4 ayrı UNION ALL: LIVE Card + LIVE Clearing + ARCHIVE Card + ARCHIVE Clearing)  
**Filtreler:** `DataScope`, `Network`, `Side` (Card=1/Clearing=2), `DateFrom`, `DateTo`, pagination

| Alan | Uyarı |
|------|-------|
| `TotalCardOriginalAmount` | Side=Clearing ise null |
| `TotalClearingSourceAmount` | Side=Card ise null |
| İki taraf karşılaştırması | Aynı gün + network + Card ve Clearing satırları yan yana getirilmeli |

---

#### D16. GET /Reporting/Reconciliation/ClearingControlStatAnalysis

**Amaç:** Clearing ControlStat × IoFlag bazında eşleşme analizi.  
**View:** `reporting.vw_recon_clearing_controlstat_analysis`  
**Filtreler:** `DataScope`, `Network`

| Alan | Uyarı |
|------|-------|
| `ControlStat` | Network-spesifik değerler; Visa/Msc/Bkm farklı |
| `UnmatchedRatePct` | Clearing tarafına bakıyor — card tarafı farklı olabilir |

---

#### D17. GET /Reporting/Reconciliation/FinancialSummary

**Amaç:** Kart işlemlerinin finansal özeti.  
**View:** `reporting.vw_recon_financial_summary`  
**Filtreler:** `DataScope`, `Network`, `FinancialType`, `TxnEffect`, `OriginalCurrency`

| Alan | Uyarı |
|------|-------|
| `DebitAmount` / `CreditAmount` | Sadece card tarafı — clearing tutarları bu raporda yok |
| `TxnEffect` D/C dışı değer | Ne debit ne credit'e dahil → kayıp tutar riski |

---

#### D18. GET /Reporting/Reconciliation/ResponseStatusAnalysis

**Amaç:** ResponseCode × ReconciliationStatus analizi.  
**View:** `reporting.vw_recon_response_status_analysis`  
**Filtreler:** `DataScope`, `Network`, `ReconciliationStatus`

| Alan | Uyarı |
|------|-------|
| Sadece card tarafı | Clearing'in response code'u yok |
| `IsSuccessfulTxn="Y"` + `ResponseCode!="00"` | Bazı network'lerde farklı başarı kodları var |

---

#### D19. GET /Reporting/Archive/RunOverview

**Amaç:** Arşivleme çalışmalarının geçmişi.  
**View:** `reporting.vw_archive_run_overview`  
**Filtreler:** `ArchiveStatus`, `ContentType`, `FileType`, `DateFrom`, `DateTo`, pagination

| Alan | Uyarı |
|------|-------|
| `ArchiveStatus` | String, enum değil |
| `FailureReasonsJson` | Ham JSON — parse edilmeli |
| `ArchiveDurationSeconds` | Yüksek = büyük dosya olabilir; Failed + duration=0 → başlamadan hata |

---

#### D20. GET /Reporting/Archive/Eligibility

**Amaç:** Dosyaların arşivlenmeye uygunluk durumu.  
**View:** `reporting.vw_archive_eligibility`  
**Filtreler:** `ContentType`, `FileType`, `ArchiveEligibilityStatus`, pagination

**Eligibility Status CASE (öncelik sırası):**
1. `ALREADY_ARCHIVED` → archive tablosunda var
2. `FILE_NOT_COMPLETE` → status ≠ 'Success'
3. `RECON_PENDING` → Ready/Processing/Failed recon satırı var
4. `ELIGIBLE` → arşivlenebilir

| Alan | Uyarı |
|------|-------|
| `ELIGIBLE` | Arşivlenebilir ama "arşivlenmeli" değil — yaş kontrolü burada yok |
| `recon_open_line_count` | Ready+Processing+**Failed** dahil — Failed recon'u olan dosya arşivlenmez |

---

#### D21. GET /Reporting/Archive/BacklogTrend

**Amaç:** Günlük arşivleme aktivite trendi.  
**View:** `reporting.vw_archive_backlog_trend`  
**Filtreler:** `DateFrom`, `DateTo`

| Alan | Uyarı |
|------|-------|
| `ArchiveRunCount=0` olan gün | Arşivleme çalışmıyor, dosyalar birikiyor |
| `OtherRunCount` | Success ve Failed dışındakiler (Skipped vb.) |

---

#### D22. GET /Reporting/Archive/RetentionSnapshot

**Amaç:** Anlık arşiv envanter özeti. **Tek satır döner.**  
**View:** `reporting.vw_archive_retention_snapshot`  
**Parametre yok.**

| Alan | Uyarı |
|------|-------|
| `ActiveFileCount` | `is_archived=false` dosyalar |
| `ArchivedMarkedFileCount` | `is_archived=true` (ingestion'da) |
| `ArchiveTableFileCount` | archive.file tablosundaki kayıtlar |
| `ArchivedMarkedFileCount ≠ ArchiveTableFileCount` | Arşiv süreci yarıda kalmış olabilir; veri kaybolmamıştır |
| `OldestUnarchivedFileDate` | null → tüm dosyalar arşivlenmiş |

---

#### D23. GET /Reporting/Reconciliation/FileReconSummary

**Amaç:** Dosya bazında mutabakat özeti — match oranı, tutarlar.  
**View:** `reporting.vw_file_recon_summary`  
**Filtreler:** `DataScope`, `ContentType`, `FileType`, `DateFrom`, `DateTo`, pagination

| Alan | Uyarı |
|------|-------|
| `MatchRatePct` | `matched * 100 / NULLIF(total, 0)` |
| `TotalOriginalAmount` | Card → original_amount, Clearing → source_amount — farklı anlamlar aynı sütunda |
| `TotalSettlementAmount` | Clearing'de hardcoded 0 |
| `ReconNotApplicableCount` | reconciliation_status IS NULL — recon sürecine hiç girmemiş (duplicate secondary vb.) |

---

#### D24. GET /Reporting/Reconciliation/MatchRateTrend

**Amaç:** Günlük eşleşme oranı trendi — network ve side bazında.  
**View:** `reporting.vw_recon_match_rate_trend` (4 UNION ALL: LIVE Card + LIVE Clearing + ARCHIVE Card + ARCHIVE Clearing)  
**Filtreler:** `DataScope`, `Network`, `Side`, `DateFrom`, `DateTo`

| Alan | Uyarı |
|------|-------|
| `MatchRatePct` | Gün bazında, kümülatif değil |
| INNER JOIN LATERAL | Detaysız satırlar düşer → total_line_count gerçek sayıdan düşük olabilir |
| Card ve Clearing oranları | Bağımsız — aynı olmak zorunda değil |

---

#### D25. GET /Reporting/Reconciliation/GapAnalysis

**Amaç:** Card ve Clearing arasındaki fark analizi. **En kritik mutabakat raporu.**  
**View:** `reporting.vw_recon_gap_analysis` (FULL OUTER JOIN, CROSS JOIN LATERAL ile LIVE/ARCHIVE)  
**Filtreler:** `DataScope`, `Network`, `DateFrom`, `DateTo`

| Alan | Açıklama | Uyarı |
|------|----------|-------|
| `LineCountDifference` | card - clearing satır farkı | Pozitif = card fazla, negatif = clearing fazla |
| `AmountDifference` | card original_amount - clearing source_amount | Farklı para birimleri dahilse anlamsız olabilir |
| `CardMatchRatePct` | Kart tarafı eşleşme | — |
| `ClearingMatchRatePct` | Clearing tarafı eşleşme | Kart ile farklı olabilir — normal |
| FULL OUTER JOIN | NULL taraflar COALESCE(x,0) ile kapatılır | "Veri yok" durumu gizlenebilir |

---

#### D26. GET /Reporting/Reconciliation/UnmatchedTransactionAging

**Amaç:** Eşleşmemiş işlemlerin yaş dağılımı.  
**View:** `reporting.vw_unmatched_transaction_aging`  
**Filtreler:** `DataScope`, `Network`, `Side`

Bucket'lar: `0-1 days`, `1-3 days`, `3-7 days`, `7-30 days`, `30+ days`

| Alan | Uyarı |
|------|-------|
| `PctOfTotalUnmatched` | Global toplam (LIVE+ARCHIVE) üzerinden — ayrı ayrı değil |
| Yaş hesabı | `f.create_date` bazlı — işlemin gerçek tarihine göre değil |
| `UnmatchedAmount` | LEFT JOIN LATERAL; detay yoksa amount=0 — gerçekten düşük görünebilir |

---

#### D27. GET /Reporting/Reconciliation/NetworkScorecard

**Amaç:** Network bazında kapsamlı skor kartı.  
**View:** `reporting.vw_network_recon_scorecard` (CROSS JOIN LATERAL ile LIVE/ARCHIVE)  
**Filtreler:** `DataScope`, `Network`

| Alan | Açıklama | Uyarı |
|------|----------|-------|
| `OverallMatchRatePct` | `matched / (card_line + clearing_line) * 100` | Paydada card+clearing toplamı → eşleşme tek taraflı olduğundan **%50'yi geçemeyebilir** — önemli metrik tasarım sorunu |
| `ReconSuccessRatePct` | `recon_success / total * 100` | Match rate ile karıştırılmamalı — farklı metrikler |
| `NetAmountDifference` | card - clearing tutar | "Eşleşmeyen tutar" değil — genel fark |
| Performans | Her file_line için 12 scalar subquery | Büyük veri setlerinde risk |

---

#### D28. GET /Reporting/Reconciliation/CardClearingCorrelation

**Amaç:** Her kart kaydı (BKM/Visa/MSC, LIVE+ARCHIVE) için olası clearing eşleşmelerinin (BKM/Visa/MSC × LIVE/ARCHIVE) ID'lerini öncelik sırasıyla (`ocean_txn_guid → rrn → arn`) tek bir satırda gösterir. Cross-network ve cross-scope (LIVE↔ARCHIVE) eşleşme tespiti için kullanılır.
**View:** `reporting.vw_card_clearing_correlation` / `[Reporting].[VwCardClearingCorrelation]`
**Filtreler:** `DataScope`, `CardTable` (`card_bkm_detail` / `card_visa_detail` / `card_msc_detail` / `archive.ingestion_card_*_detail`), `FileId`, `FileLineId`, `CardId`, pagination
**Handler:** `GetCardClearingCorrelationQueryHandler` (resx: `Handler.Reporting.CardClearingCorrelationFailed`)

**Teknik:**
- Tüm 6 kart detay tablosu (3 network × LIVE/ARCHIVE) `UNION ALL` ile birleşir
- Her satır için 6 ayrı `LEFT JOIN LATERAL` (PostgreSQL) / `OUTER APPLY` (MSSQL) bloğu — clearing tablosunda `ocean_txn_guid = c.ocean_txn_guid` (rank 1) → `rrn = c.rrn` (rank 2) → `arn = c.arn` (rank 3) öncelik sırasıyla **ilk** eşleşen kayıt çekilir (`LIMIT 1` / `TOP 1`)
- ROW_NUMBER `(data_scope, card_table, card_id)` ASC sırasıyla atanır

**DTO Alanları (`CardClearingCorrelationDto`):**

| Alan | Açıklama | Uyarı |
|------|----------|-------|
| `RowNumber` | Sıralama numarası | Stable değil; sorgu/sayfa değişince yeniden hesaplanır |
| `DataScope` | LIVE / ARCHIVE | Kart kaydının yaşadığı ortam |
| `CardTable` | Kart detay tablosunun adı | Network'ü ima eder ama enum değil — string |
| `CardId` | Kart kaydı PK | İlgili `card_*_detail.id` |
| `FileLineId` / `FileId` | Kart kaydının dosya bağlamı | Filtreleme için indeks dostu |
| `ClearingBkmLiveId` | LIVE BKM clearing eşleşmesi | null = eşleşme yok |
| `ClearingBkmArchiveId` | ARCHIVE BKM clearing eşleşmesi | null = eşleşme yok |
| `ClearingVisaLiveId` / `ClearingVisaArchiveId` | Visa eşleşmeleri | Cross-network eşleşme tespiti için kritik |
| `ClearingMastercardLiveId` / `ClearingMastercardArchiveId` | MSC eşleşmeleri | — |

**Yorumlama:**
- Aynı satırda birden fazla `Clearing*Id` dolu → kart kaydı için **birden fazla network/scope'ta** clearing eşleşmesi var (cross-network çakışma şüphesi)
- Tüm 6 alan null → henüz eşleşmeyen kart (D26 `UnmatchedTransactionAging` ile çapraz okunmalı)
- `LIVE` kartın yalnızca `ARCHIVE` clearing eşleşmesi → geç gelen clearing arşivde; `ClearingArrivalRequeueService` müdahalesi gerekebilir
- Match priority `ocean_txn_guid → rrn → arn` sırasıyla en güçlü eşleşmeyi tek tek seçer; ikincil eşleşmeler view'da görünmez

---

## 5. SQL View Analizi

### Özet Tablo

| View | Bölüm | Temel Teknik Özellik |
|------|-------|---------------------|
| `vw_ingestion_file_overview` | A1 | LEFT JOIN LATERAL, UNION ALL (LIVE+ARCHIVE) |
| `vw_ingestion_file_quality` | A2 | LEFT JOIN LATERAL, duplicate_status bazlı COUNT |
| `vw_ingestion_daily_summary` | A3 | GROUP BY DATE_TRUNC |
| `vw_ingestion_network_matrix` | A4 | GROUP BY content_type × file_type, tarih yok |
| `vw_ingestion_exception_hotspots` | A5 | WHERE failed>0, CASE severity, sadece LIVE |
| `vw_recon_daily_overview` | B1 | 5 ayrı LEFT JOIN, CTE dates |
| `vw_recon_open_items` | B2 | Sadece LIVE, Planned/Blocked/Executing |
| `vw_recon_open_item_aging` | B3 | CASE yaş bucket'ları, saat bazlı |
| `vw_recon_manual_review_queue` | B4 | 3 LATERAL, urgency CASE, WHERE decision=Pending |
| `vw_recon_alert_summary` | B5 | GROUP BY severity × type × status |
| `vw_recon_live_card_content_daily` | C1 | **INNER JOIN LATERAL**, 15 boyutlu GROUP BY |
| `vw_recon_live_clearing_content_daily` | C2 | INNER JOIN LATERAL, clearing detayları |
| `vw_recon_archive_card_content_daily` | C3 | C1'in archive versiyonu |
| `vw_recon_archive_clearing_content_daily` | C4 | C2'nin archive versiyonu |
| `vw_recon_content_daily` | C5 | 4 UNION ALL, bağımsız sorgu |
| `vw_recon_clearing_controlstat_analysis` | C6 | GROUP BY control_stat × io_flag |
| `vw_recon_financial_summary` | C7 | Sadece card, CASE debit/credit |
| `vw_recon_response_status_analysis` | C8 | ResponseCode × TxnStat × ReconciliationStatus |
| `vw_archive_run_overview` | D1 | archive_log JOIN ingestion/archive file |
| `vw_archive_eligibility` | D2 | CASE eligibility (4 durum) |
| `vw_archive_backlog_trend` | D3 | archive_log GROUP BY DATE_TRUNC |
| `vw_archive_retention_snapshot` | D4 | Scalar subquery'ler, tek satır |
| `vw_file_recon_summary` | E1 | Çift LATERAL, card+clearing tutar karışımı |
| `vw_recon_match_rate_trend` | E2 | 4 UNION ALL, INNER JOIN LATERAL |
| `vw_recon_gap_analysis` | E3 | **FULL OUTER JOIN**, CROSS JOIN LATERAL |
| `vw_unmatched_transaction_aging` | E4 | WHERE matched_clearing_line_id IS NULL, yaş bucket |
| `vw_network_recon_scorecard` | E5 | CROSS JOIN LATERAL, 12 scalar subquery — performans riski |
| `vw_card_clearing_correlation` | F1 | UNION ALL (3 network × LIVE/ARCHIVE = 6 kart kaynağı), 6 LEFT JOIN LATERAL, ocean_txn_guid → rrn → arn öncelikli match |

### Kritik SQL Tasarım Notları

- **INNER JOIN LATERAL vs LEFT JOIN LATERAL:** C1/C2/C5 INNER JOIN kullanır → detaysız satırlar düşer. B4/E1 LEFT JOIN → satır kaybolmaz.
- **FULL OUTER JOIN (E3 Gap Analysis):** Bir tarafın olmadığı günler NULL → COALESCE(x,0) gizler.
- **NetworkScorecard (E5) metriki:** `overall_match_rate_pct` paydası card+clearing toplamı. Eşleşme tek taraflıdır (1 card 1 clearing ile eşleşir), bu yüzden matematiksel olarak %50'yi geçmesi güç.
- **RetentionSnapshot (D4):** `archived_marked_file_count` ve `archive_table_file_count` SQL'de identik → her zaman aynı değeri döner. Biri `ingestion.file.is_archived=true` saymalıydı.

---

## 6. Status / Enum Sözlüğü

### FileIngestion Enum'ları

| Enum | Değerler |
|------|----------|
| `FileSourceType` | Remote(1), Local(2) |
| `FileType` | Card(1), Clearing(2) |
| `FileContentType` | Bkm(1), Msc(2), Visa(3) |
| `FileStatus` | Processing(1), Failed(2), Success(3) |
| `FileRowStatus` | Processing(1), Failed(2), Success(3) |
| `DuplicateStatus` | Unique(1), Primary(2), Secondary(3), Conflict(4) |
| `ReconciliationStatus` | Ready(1), Failed(2), Success(3), Processing(4), AwaitingClearing(5) |

### Reconciliation Enum'ları

| Enum | Değerler |
|------|----------|
| `EvaluationStatus` | Pending(0), Evaluating(1), Planned(2), Failed(3), Completed(4) |
| `OperationStatus` | Planned(0), Blocked(1), Executing(2), Completed(3), Failed(4), Cancelled(5) |
| `ExecutionStatus` | Started(0), Completed(1), Failed(2), Skipped(3) |
| `AlertStatus` | Pending(0), Processing(1), Consumed(2), Failed(3), Ignored(4) |
| `ReviewDecision` | Pending(0), Approved(1), Rejected(2), Cancelled(3) |
| `ReviewExpirationAction` | Cancel(0), Approve(1), Reject(2) |
| `ReviewExpirationFlowAction` | Continue(0), CancelRemaining(1) |

### Reporting Enum'ları

| Enum | Değerler |
|------|----------|
| `DataScope` | LIVE(1), ARCHIVE(2) |
| `ReconSide` | Card(1), Clearing(2) |
| `SeverityLevel` | LOW(1), MEDIUM(2), HIGH(3), CRITICAL(4) |
| `UrgencyLevel` | NORMAL(1), OVERDUE(2), EXPIRING_SOON(3), EXPIRED(4) |
| `ArchiveEligibilityStatus` | ELIGIBLE(1), ALREADY_ARCHIVED(2), FILE_NOT_COMPLETE(3), RECON_PENDING(4) |

### Status Detayları (İnsan Dili)

**FileStatus:**

| Değer | Terminal? | Anlamı |
|-------|-----------|--------|
| Processing(1) | Hayır | Dosya henüz tamamlanmadı |
| Failed(2) | Evet | Dosya hatalı; manuel müdahale gerekebilir |
| Success(3) | Evet | Dosya başarıyla işlendi (satır bazlı hata olabilir) |

**OperationStatus:**

| Değer | Anlamı |
|-------|--------|
| Planned(0) | Çalıştırılmaya hazır |
| Blocked(1) | Bağımlılık veya manuel karar bekleniyor (genellikle hata değil) |
| Executing(2 | Şu an çalışıyor (lease aktif) |
| Completed(3) | Başarıyla tamamlandı |
| Failed(4) | Başarısız (retry mümkün olabilir) |
| Cancelled(5) | İptal edildi (Reject sonrası) |

**DuplicateStatus:**

| Değer | Anlamı |
|-------|--------|
| Unique(1) | Tekil kayıt |
| Primary(2) | Duplicate grubunun işleme alınan kaydı |
| Secondary(3) | Duplicate grubunun tekrar kaydı (atlanır) |
| Conflict(4) | Belirsiz — manuel müdahale gerekebilir |

**ReconciliationStatus:**

| Değer | Terminal? | Anlamı |
|-------|-----------|--------|
| Ready(1) | Hayır | Değerlendirici tarafından alınmaya hazır |
| Failed(2) | Evet | Değerlendirme/persist hatası |
| Success(3) | Evet | Değerlendirme tamamlandı, planlanan operasyonlar oluşturuldu |
| Processing(4) | Hayır | EvaluateService satırı claim etti, değerlendiriliyor |
| AwaitingClearing(5) | Hayır (geri dönebilir) | İlgili clearing kaydı henüz dosya olarak gelmemiş; karar ertelendi. Clearing dosyası başarıyla ingestion edildiğinde `ClearingArrivalRequeueService` satırı tekrar `Ready`'e çeker (bkz. §16.3 — clearing-presence gate). |

---

## 7. State Transition Diyagramları

### File Status
```
(yeni dosya) → Processing → Success
                           → Failed
```

### File Line Status
```
(yeni satır) → Processing → Success
                           → Failed
```

### Reconciliation Status (satır seviyesi)
```
(yeni satır) → Ready ─→ Processing ─→ Success
                  ↑                   ─→ Failed
                  │                   ─→ AwaitingClearing  ──┐
                  └────────────────────────────────────────── ┘
                     (ClearingArrivalRequeueService:
                      ilgili clearing dosyası gelince
                      AwaitingClearing satırlar Ready'e çevrilir
                      ve aynı satır yeniden değerlendirilir)
```

### Evaluation Status
```
Pending → Evaluating → Planned → Completed
                     → Failed
```

### Operation Status
```
Planned ──→ Executing ──→ Completed
   ↑                   ──→ Failed
   │
Blocked ──→ (Approve) ──→ Planned
         ──→ (Reject)  ──→ Cancelled
         ──→ (Expire)  ──→ (ExpirationAction'a göre)
```

### Execution Status
```
Started → Completed
        → Failed
        → Skipped
```

### Review Decision
```
Pending → Approved
        → Rejected
        → Cancelled (expire/sistem)
```

### Alert Status
```
Pending → Processing → Consumed
                     → Failed
        → Ignored
```

---

## 8. DTO → View Mapping

| DTO | SQL View |
|-----|----------|
| `IngestionFileOverviewDto` | `reporting.vw_ingestion_file_overview` |
| `IngestionFileQualityDto` | `reporting.vw_ingestion_file_quality` |
| `IngestionDailySummaryDto` | `reporting.vw_ingestion_daily_summary` |
| `IngestionNetworkMatrixDto` | `reporting.vw_ingestion_network_matrix` |
| `IngestionExceptionHotspotDto` | `reporting.vw_ingestion_exception_hotspots` |
| `ReconDailyOverviewDto` | `reporting.vw_recon_daily_overview` |
| `ReconOpenItemDto` | `reporting.vw_recon_open_items` |
| `ReconOpenItemAgingDto` | `reporting.vw_recon_open_item_aging` |
| `ReconManualReviewQueueDto` | `reporting.vw_recon_manual_review_queue` |
| `ReconAlertSummaryDto` | `reporting.vw_recon_alert_summary` |
| `ReconCardContentDailyDto` | `reporting.vw_recon_live_card_content_daily` / `vw_recon_archive_card_content_daily` |
| `ReconClearingContentDailyDto` | `reporting.vw_recon_live_clearing_content_daily` / `vw_recon_archive_clearing_content_daily` |
| `ReconContentDailyDto` | `reporting.vw_recon_content_daily` |
| `ReconClearingControlStatAnalysisDto` | `reporting.vw_recon_clearing_controlstat_analysis` |
| `ReconFinancialSummaryDto` | `reporting.vw_recon_financial_summary` |
| `ReconResponseStatusAnalysisDto` | `reporting.vw_recon_response_status_analysis` |
| `ArchiveRunOverviewDto` | `reporting.vw_archive_run_overview` |
| `ArchiveEligibilityDto` | `reporting.vw_archive_eligibility` |
| `ArchiveBacklogTrendDto` | `reporting.vw_archive_backlog_trend` |
| `ArchiveRetentionSnapshotDto` | `reporting.vw_archive_retention_snapshot` |
| `FileReconSummaryDto` | `reporting.vw_file_recon_summary` |
| `ReconMatchRateTrendDto` | `reporting.vw_recon_match_rate_trend` |
| `ReconGapAnalysisDto` | `reporting.vw_recon_gap_analysis` |
| `UnmatchedTransactionAgingDto` | `reporting.vw_unmatched_transaction_aging` |
| `NetworkReconScorecardDto` | `reporting.vw_network_recon_scorecard` |
| `CardClearingCorrelationDto` | `reporting.vw_card_clearing_correlation` |

---

## 9. Business ve Teknik Kurallar Kataloğu

### Business Kuralları

1. **Sıralama zorunluluğu:** Evaluate → Execute sırası değiştirilemez. Execute öncesinde Evaluate olmadan çalıştırılacak operasyon yoktur.
2. **İkili dosya gerekliliği:** Eşleştirme için hem Card hem Clearing dosyası sisteme alınmış olmalı. Tek taraflı evaluate tüm satırları unmatched bırakır.
3. **Terminal olmayan kayıtlar arşivlenemez:** Reconciliation/operation/review/alert tamamlanmamış dosya `Archive/Run`'da skip edilir.
4. **Saklama süresi (Retention):** Varsayılan 90 gün (`RetentionDays`). Son güncelleme en az 72 saat önce (`MinLastUpdateAgeHours`).
5. **Manuel review SLA:** `ExpiresAt` yaklaşan review'lar `EXPIRING_SOON`, geçenleri `EXPIRED`. Expire olursa `ExpirationAction` (Cancel/Approve/Reject) otomatik uygulanır.
6. **Reject yorumu zorunlu:** `Reject` endpoint'inde `Comment` zorunlu (NotEmpty), `Approve`'da opsiyonel.
7. **Archive geri dönülemez:** `Archive/Run` commit sonrası canlı veriler silinir. Geri yükleme manuel yapılmalıdır.
8. **Auto Archive:** `AutoArchiveAfterExecute=true` ise Execute sonrası uygun dosyalar otomatik arşivlenir (fire-and-forget, Task.Run).

### Teknik Kurallar

1. **HTTP 200 yeterli değildir:** `ErrorCount` ve `Errors` her zaman kontrol edilmelidir.
2. **Evaluate idempotent değil ama güvenli:** Aynı dosya tekrar evaluate edilse sadece `Ready` satırlar hedeflenir. Stale claim → timeout sonrası başka çağrı devralır.
3. **Execute lease ile concurrent güvenli:** Birden fazla worker aynı anda Execute çağırabilir; lease çakışmasında biri bekler.
4. **Claim mekanizması (Evaluate):** `Serializable` isolation. Stale claim: `ClaimTimeoutSeconds` sonra başka çağrı devralabilir.
5. **Exponential backoff (Execute retry):** `NextAttemptAt = now + (30s × 2^RetryCount)`. `RetryCount >= MaxRetries` → Failed.
6. **Idempotency (Execute):** Aynı `IdempotencyKey` ile Completed başka operasyon → Skipped (`SKIPPED_ALREADY_APPLIED`).
7. **Batch fallback (Evaluate persistence):** Batch kayıt başarısızsa tek tek kaydetmeye geçilir.
8. **Audit zorunluluğu (Archive/Run):** `AuditStampService.EnsureAuditContext()` — audit bilgisi olmadan Run başlamaz.
9. **Count verification (Archive/Run):** ARCHIVE_COPY ve LIVE_DELETE sonrası sayı eşleşme kontrolü; mismatch → transaction rollback.
10. **ContinueOnError (Archive):** Varsayılan `false` — ilk hatada durur. `true` ile hatalıyı atlar ama hatalı dosyalar manuel kontrol edilmeli.
11. **Alert e-posta:** `AlertOptions.Enabled` ve `ToEmails` konfigürasyonuna bağlı. Alıcı listesi boşsa alert'ler gönderilmez ama `Pending` kalır.
12. **Raporları tek başına yorumlamak:** Özellikle `MatchRateTrend` tek başına okunursa yanlış aksiyon.

---

## 10. Request Parametre Sözlüğü

| Endpoint | Parametre | Tip | Zorunlu | Uyarı |
|----------|-----------|-----|---------|-------|
| Archive/Preview | `IngestionFileIds` | `Guid[]?` | Hayır | No Empty, Distinct; boş = tüm adaylar |
| Archive/Preview | `BeforeDate` | `DateTime?` | Hayır | ≤ bugün; `UseConfiguredBeforeDateOnly=true` ise yok sayılır |
| Archive/Preview | `Limit` | `int?` | Hayır | 0–10.000; Clamp(1,1000) |
| Archive/Run | `IngestionFileIds` | `Guid[]?` | Hayır | Preview sonucu ile aynı set önerilir |
| Archive/Run | `MaxFiles` | `int?` | Hayır | 0–10.000; büyük ortamda batch'e böl |
| Archive/Run | `ContinueOnError` | `bool?` | Hayır | İlk geçişte false, iyileştirme geçişinde true |
| FileIngestion | `FileSourceType` | `FileSourceType` | Evet | Remote/Local — dosya kaynağını belirler |
| FileIngestion | `FileType` | `FileType` | Evet | Card/Clearing — yanlış → detay tablosu hatalı |
| FileIngestion | `FileContentType` | `FileContentType` | Evet | Bkm/Msc/Visa — yanlış → parser bozulur |
| FileIngestion | `FilePath` | `string` | Koşullu | Local ise zorunlu, Remote ise boş/null | Remote ise config default | Local'de boş → validator reddeder |
| Evaluate | `IngestionFileIds` | `Guid[]` | Hayır | Boş = tüm Ready satırlar; geniş boş çağrı büyük batch |
| Evaluate | `Options.ChunkSize` | `int?` | Hayır | 100–10.000; trafike göre ayarla |
| Execute | `GroupIds` | `Guid[]` | Hayır | EvaluationRunId ile eşleşmeli |
| Execute | `EvaluationIds` | `Guid[]` | Hayır | Hedefli rerun için |
| Execute | `OperationIds` | `Guid[]` | Hayır | En dar kapsam; doğrulayarak kullan |
| Execute | `Options.MaxEvaluations` | `int?` | Hayır | 1–100.000 |
| Execute | `Options.LeaseSeconds` | `int?` | Hayır | 1–3.600; operasyon süresinin biraz üstünde |
| Approve | `OperationId` | `Guid` | Evet | Pending listeden kopyala |
| Approve | `Comment` | `string` | Hayır | Max 2000 karakter |
| Reject | `OperationId` | `Guid` | Evet | Karar öncesi branch etkisini kontrol et |
| Reject | `Comment` | `string` | **Evet** | NotEmpty, max 2000 |
| Pending Reviews | `Date` | `DateOnly?` | Hayır | ≤ bugün; vardiya bazlı ve sayfalı kullan |
| Alerts | `AlertStatus` | `AlertStatus?` | Hayır | Pending+Failed birlikte izle |

---

## 11. Response Alanları Sözlüğü

| Endpoint | Alan | Yanlış Yorumlama | Doğru Kullanım |
|----------|------|------------------|----------------|
| FileIngestion | `Status` | Processing'i başarı saymak | Success/Failed terminal durumunu bekle |
| FileIngestion | `TotalCount` | "Başarılı satır" sanmak | SuccessCount ve ErrorCount ile birlikte oku |
| Evaluate | `EvaluationRunId` | GroupId ilişkisi kaçırılırsa execute hedefleme zorlaşır | Execute çağrılarında GroupId olarak sakla |
| Evaluate | `CreatedOperationsCount` | Satır sayısı sanmak | Satır metriklerinden ayrı KPI |
| Evaluate | `SkippedCount` | "Önemsiz" saymak | Errors ile birlikte kök neden incele |
| Execute | `TotalSucceeded` | Skipped dahil olduğu unutulursa başarı abartılır | TotalFailed ve Results ile yorumla |
| Execute | `Results` | Boş = "hiç operasyon yok" | TotalAttempted ile birlikte oku |
| Approve/Reject | `Result` | "Invalid" görmezden gelmek | Invalid → review final state analizi yap |
| Pending Reviews | `ExpiresAt` | null'u gecikme sanmak | null = timeout yok; SLA'de ayrı ele al |
| Alerts | `OperationId` | Guid.Empty'yi veri hatası sanmak | Evaluation-level alert olabilir |
| Archive/Run | `ArchivedCount` | ProcessedCount ile karıştırmak | Processed/Skipped/Failed üçlüsüyle raporla |
| Archive/Run | `SkippedCount` | "Sorun değil" saymak | FailureReasons dağılımı ile kök sebep çıkar |

---

## 12. Rapor Karar Kartları

| Rapor | Neyi cevaplar? | Kim için? | En kritik 3 alan | Alarm | Yanlış karar |
|-------|----------------|-----------|-----------------|-------|--------------|
| Ingestion/FileOverview | "Hangi dosya alımı sağlıklı?" | Operasyon, teknik | `FileStatus`, `LineFailRatePct`, `CompletenessPct` | Failed + yüksek fail rate | Recon gecikmesini atlamak |
| Ingestion/FileQuality | "Dosya kalitesi bozuluyor mu?" | Operasyon, analist | `ErrorRatePct`, `DuplicateImpactPct`, `LinesWithRetryCount` | Duplicate/error sıçraması | Retry'ı normal saymak |
| Ingestion/DailySummary | "Günlük alım trendi?" | Yönetim, operasyon | `FileCount`, `FailedFileCount`, `ProcessedLineSuccessRatePct` | Failed trend artışı | Tek gün ile kalıcı bozuk demek |
| Ingestion/NetworkMatrix | "Hangi network/file type zayıf?" | Operasyon, yönetim | `ContentType`, `FailedLineCount`, `LastFileAt` | Belirli ağda sürekli başarısızlık | Toplam başarıyı görüp ağ bazlı sorunu kaçırmak |
| Ingestion/ExceptionHotspots | "En riskli dosyalar?" | Operasyon, teknik | `SeverityLevel`, `FailedLineCount`, `DistinctErrorMessageCount` | HIGH/CRITICAL yoğunluğu | Düşük hacimli kritik hatayı önemsiz saymak |
| Reconciliation/DailyOverview | "Mutabakat günlük performansı?" | Yönetim, operasyon | `OperationSuccessRatePct`, `FailedOperationCount`, `PendingAlertCount` | Başarı düşerken alert artışı | Sadece toplam operation sayısına bakmak |
| Reconciliation/OpenItems | "Hangi işler açık?" | Operasyon | `OperationStatus`, `Branch`, `IsManual` | Blocked+Planned birikmesi | Açık iş yaşını dikkate almamak |
| Reconciliation/OpenItemAging | "Açık işler ne kadar yaşlandı?" | Operasyon, yönetim | `AgeBucket`, `ItemCount` | Yaşlı bucket büyümesi | Sadece adete bakıp kritikliği kaçırmak |
| Reconciliation/ManualReviewQueue | "Manuel onay kuyruğu ne durumda?" | Operasyon, analist | `UrgencyLevel`, `ExpiresAt`, `OperationCode` | EXPIRED/EXPIRING_SOON artışı | Sırf adede bakıp kritikliği kaçırmak |
| Reconciliation/AlertSummary | "Alert'ler nerede yoğun?" | Operasyon, teknik | `AlertType`, `AlertStatus`, `Severity` | Pending/Failed yığılması | Info ve Error alert'i aynı ağırlıkta yorumlamak |
| Reconciliation/LiveCardContentDaily | "Canlı kart akışı durumu?" | Analist, yönetim | `TxnCount`, `Amount`, `Network` | Ani hacim/tutar kırılması | Sezon etkisini hesaba katmamak |
| Reconciliation/ArchiveCardContentDaily | "Arşiv kart geçmişi trendi?" | Analist | `TxnCount`, `Amount`, `ReportDate` | Tarihsel trendde keskin sapma | Live ve archive kıyasını aynı pencerede yapmamak |
| Reconciliation/LiveClearingContentDaily | "Canlı clearing dengeli mi?" | Operasyon, analist | `TxnCount`, `Amount`, `Network` | Card tarafıyla korelasyon bozulması | Tek taraf ile kesin karar vermek |
| Reconciliation/ContentDaily | "Card ve Clearing birleşik görünüm?" | Yönetim, analist | `DataScope`, `Side`, `Amount` | Side bazında kalıcı fark | Side filtresi olmadan karar vermek |
| Reconciliation/ClearingControlStatAnalysis | "Clearing kontrol statüleri sorunlu mu?" | Operasyon, teknik | `ControlStat`, `Count`, `Network` | Problemli statüde yoğunlaşma | Statü anlamını bilmeden aksiyon almak |
| Reconciliation/FinancialSummary | "Finansal özet ne durumda?" | Yönetim, finans | `FinancialType`, `TxnEffect`, `Amount` | Beklenmeyen effect yönü | Para birimi normalize etmeden kıyaslamak |
| Reconciliation/ResponseStatusAnalysis | "Response code sorunu var mı?" | Teknik, operasyon | `ResponseCode`, `ReconciliationStatus`, `Count` | Başarısız kod yoğunluğu | Geçici dış servis sorununu kalıcı rule hatası sanmak |
| Archive/RunOverview | "Arşiv koşuları nasıl?" | Operasyon, yönetim | `ArchiveStatus`, `ArchivedCount`, `FailedCount` | Failed run zinciri | Tek başarılı koşuyla sürdürülebilirlik varsaymak |
| Archive/Eligibility | "Hangi dosya neden uygun değil?" | Operasyon, analist | `ArchiveEligibilityStatus`, `FileType`, `ContentType` | FILE_NOT_COMPLETE/RECON_PENDING yığılması | Uygunsuz dosyayı zorla run'a almak |
| Archive/BacklogTrend | "Arşiv backlog'u artıyor mu?" | Yönetim, operasyon | `BacklogCount`, `Date` | Backlog sürekli yukarı | Kısa dönem düşüşü kalıcı iyileşme sanmak |
| Archive/RetentionSnapshot | "Canlı/arşiv dağılımı?" | Yönetim, teknik | `ActiveFileCount`, `ArchivedFileCount` | Active tarafta birikim | Yeni dosya etkisini hesaba katmadan alarm üretmek |
| Advanced/FileReconSummary | "Dosya bazında match performansı?" | Analist, operasyon | `MatchRatePct`, `MatchedCount`, `UnmatchedCount` | Match oranında kalıcı düşüş | Tek dosya ile bütün network değerlendirmesi |
| Advanced/MatchRateTrend | "Eşleşme oranı iyileşiyor mu?" | Yönetim, analist | `MatchRatePct`, `Date`, `Network` | Trend aşağı kırılım | Gecikmeli eşleşme etkisini hesaba katmamak |
| Advanced/GapAnalysis | "Card ve Clearing arasında fark mı büyüyor?" | Operasyon, analist | `LineCountDifference`, `AmountDifference`, `Network` | Farkların eşik üstü kalması | Kur/fx/zaman farkını kayıp sanmak |
| Advanced/UnmatchedTransactionAging | "Eşleşmeyenler ne kadar süredir bekliyor?" | Operasyon | `AgeBucket`, `UnmatchedCount`, `Side` | Yaşlı eşleşmeyen artışı | Yeni ile eskiyi aynı ele almak |
| Advanced/NetworkScorecard | "Ağ bazında kalite skoru?" | Yönetim, operasyon | `OverallMatchRatePct`, `ReconSuccessRatePct` | Skor sürekli düşüşte | Score bileşenlerini görmeden tek metrikle karar |
| Advanced/CardClearingCorrelation | "Bir kart hangi clearing kaydı/kayıtlarıyla çakıştı?" | Operasyon, teknik | `CardId`, `Clearing*LiveId`, `Clearing*ArchiveId` | Birden fazla network/scope'ta eşleşme = çakışma şüphesi | Tüm kolonların null olmasını "veri eksik" sanmak (gerçekten eşleşme yok demek) |

### Rapor Seçim Rehberi

```
"Dosyalar geldi mi?"
 └→ Ingestion/FileOverview, Ingestion/DailySummary

"Dosya kalitesi nasıl?"
 └→ Ingestion/FileQuality, Ingestion/ExceptionHotspots

"Hangi network'ten ne kadar veri geliyor?"
 └→ Ingestion/NetworkMatrix

"Mutabakat süreci sağlıklı mı?"
 └→ Reconciliation/DailyOverview

"Bekleyen/takılan işler var mı?"
 └→ Reconciliation/OpenItems, OpenItemAging

"Onay bekleyen işler neler?"
 └→ Reconciliation/ManualReviewQueue

"Uyarılar neler?"
 └→ Reconciliation/AlertSummary

"Kart işlem detayları?"
 └→ LiveCardContentDaily, ArchiveCardContentDaily

"Kliring detayları?"
 └→ LiveClearingContentDaily, ArchiveClearingContentDaily

"Kart ve kliring birlikte?"
 └→ ContentDaily

"Kliring kontrol durumları sorunlu mu?"
 └→ ClearingControlStatAnalysis

"Finansal özet?"
 └→ FinancialSummary

"Yanıt kodları eşleşmeyi etkiliyor mu?"
 └→ ResponseStatusAnalysis

"Arşivleme nasıl gidiyor?"
 └→ Archive/RunOverview, BacklogTrend, RetentionSnapshot

"Hangi dosyalar arşive uygun?"
 └→ Archive/Eligibility

"Dosya bazında eşleşme durumu?"
 └→ FileReconSummary

"Eşleşme oranı trend olarak?"
 └→ MatchRateTrend

"Kart vs kliring arasında fark?"
 └→ GapAnalysis

"Eşleşmeyenler ne kadar süredir bekliyor?"
 └→ UnmatchedTransactionAging

"Network bazında genel skor?"
 └→ NetworkScorecard

"Bir kart kaydı hangi clearing'lerle eşleşti / cross-network çakışma var mı?"
 └→ CardClearingCorrelation
```

---

## 13. En Sık Yapılan Hatalar

### Yanlış Endpoint Sırası

**❌ Execute → Evaluate:**  
`POST /Execute` → "0 operations executed"  
`POST /Evaluate` → Operasyonlar planlanır ama execute edilmez.  
**✅ Doğru:** Evaluate → Execute

**❌ Evaluate → Evaluate (tekrar):**  
`POST /Evaluate (ilk)` → 150 operasyon  
`POST /Evaluate (tekrar)` → 0 operasyon, 150 skipped  
Aynı satırlar yeniden değerlendirilmez.

**❌ Sadece Card dosyası ile Evaluate:**  
`POST /FileIngestion (Card)` + `POST /Evaluate` → Tüm satırlar "unmatched"  
Eşleşme için HEM card HEM clearing gerekir.

**❌ Archive → Execute:**  
Arşivlenen dosyaların operasyonları execute edilemez. Execute, archive şemasından çalışmaz.

### Yanlış Rapor Yorumlama

**❌ "MatchRatePct %95, her şey yolunda"**  
Kalan %5 yüksek tutarlıysa finansal risk büyük olabilir. `UnmatchedAmount`'a da bakılmalı.

**❌ "OperationSuccessRatePct %100, sorun yok"**  
Sadece DENENEN operasyonlar arasındaki başarı. 900 Blocked, 100 denendi ve %100 → ama 900 iş hâlâ bekliyor.

**❌ "FileStatus=Success, dosya sorunsuz"**  
Dosya seviyesi başarı. İçindeki satırlardan 200'ü Failed olabilir — `ErrorCount` bakılmalı.

**❌ "CompletenessPct=%0, hiç işlenmemiş"**  
`expected_line_count=0` ise hesaplama yapılamaz, 0 döner. Dosya işlenmiş ama expected set edilmemiş olabilir.

**❌ "ArchivedMarkedFileCount ≠ ArchiveTableFileCount → veri kaybolmuş"**  
Arşiv süreci yarıda kalmış olabilir. Veri kaybolmamıştır — ingestion'da hâlâ duruyor.

### Archive Hataları

**❌ Preview yapmadan Run:**  
Eligible olmayan dosyalar skip edilir; neden arşivlenmedi anlaşılmaz.

**❌ `ContinueOnError=false` ile büyük batch:**  
İlk hatalı dosyada tüm süreç durur. 100 dosyadan 1'i hatalı → 99 arşivlenmez.

**❌ Reconciliation tamamlanmadan arşivleme:**  
`ReconOpenLineCount > 0` → NotEligible.

**❌ `MaxFiles` olmadan büyük ortamda Run:**  
Binlerce dosya tek seferde → uzun süre, timeout riski.

### Genel Teknik Hatalar

1. Sadece HTTP 200'e bakmak — `ErrorCount` ve `Errors` kontrol edilmelidir.
2. `Blocked` durumu hata sanmak — çoğunlukla manuel karar veya sıra bekleme.
3. Pending review SLA takibi yapmamak — expire sonrası otomatik kararlar istenmeyen branch'i açabilir.
4. Raporları tek başına yorumlamak — özellikle `MatchRateTrend` tek başına okunursa yanlış aksiyon.

---

## 14. Uçtan Uca Senaryo

### Sabah 06:00 — Dosyalar Gelir

Visa sunucusundan bir Card ve bir Clearing dosyası:

```
POST /FileIngestion { FileSourceType:"Remote", FileType:"Card", FileContentType:"Visa" }
→ FileId=aaa-111, Status=Success, TotalCount=15000, SuccessCount=14980, ErrorCount=20

POST /FileIngestion { FileSourceType:"Remote", FileType:"Clearing", FileContentType:"Visa" }
→ FileId=bbb-222, Status=Success, TotalCount=14500, SuccessCount=14500, ErrorCount=0
```

Veritabanında 29.480 file_line, her biri `reconciliation_status=Ready`.

### Sabah 06:05 — Değerlendirme

```
POST /Reconciliation/Evaluate { "IngestionFileIds": ["aaa-111", "bbb-222"] }
→ EvaluationRunId=ccc-333, CreatedOperationsCount=520, SkippedCount=0
```

- 14.200 kart satırı eşleşti
- 780 kart satırı eşleşmedi
- 300 kliring satırı eşleşmedi
- 520 operasyon: 505 Planned, 15 Blocked (is_manual=true)

### Sabah 06:10 — Operatör Dashboard

```
GET /Reporting/Reconciliation/DailyOverview?DateFrom=2026-04-16
→ TotalOperationCount=520, ManualOperationCount=15, PendingReviewCount=15

GET /Reconciliation/Reviews/Pending
→ 15 review listelenir
```

12 → Approve, 3 → Reject:
```
POST /Reconciliation/Reviews/Approve { "OperationId": "op-1", "Comment": "Tutar farkı kur kaynaklı" }
POST /Reconciliation/Reviews/Reject  { "OperationId": "op-13", "Comment": "Mükerrer işlem şüphesi" }
```

12 operasyon → Planned, 3 operasyon → Cancelled.

### Sabah 06:30 — Execute

```
POST /Reconciliation/Operations/Execute
→ TotalAttempted=517, TotalSucceeded=515, TotalFailed=2
```

`AutoArchiveAfterExecute=true` → arka planda arşiv başlar.

```
(Otomatik) POST /Archive/Run
→ ArchivedCount=1 (bbb-222 arşivlendi), SkippedCount=1 (aaa-111 → 2 failed op var)
```

### Sabah 09:00 — Sorun Takibi

```
GET /Reporting/Reconciliation/OpenItems?OperationStatus=Failed
→ 2 operasyon, RetryCount=1, MaxRetryCount=3

POST /Reconciliation/Operations/Execute { "OperationIds": ["failed-op-1", "failed-op-2"] }
→ TotalSucceeded=2, TotalFailed=0
```

### Akşam 18:00 — Manuel Arşivleme

```
POST /Archive/Preview { "IngestionFileIds": ["aaa-111"] }
→ IsEligible=true

POST /Archive/Run { "IngestionFileIds": ["aaa-111"] }
→ ArchivedCount=1
```

**Gün sonu:** Tüm dosyalar arşivlenmiş. Aktif tablolar temiz.

---

## 15. Akış Diyagramları ve Hızlı Referans

### Temel Endpoint Akışı

```
POST /FileIngestion (Card)  POST /FileIngestion (Clearing)
         │                           │
         └───────────┬───────────────┘
                     ▼
              POST /Evaluate       → Operasyonlar planlanır
                     │
              ┌──────┴──────┐
         Otomatik          Manuel
         (Planned)         (Blocked)
              │                │
              │         GET /Reviews/Pending
              │                │
              │         Approve/Reject
              │                │
              └────────────────┘
                     │
              POST /Execute    → Operasyonlar çalışır
                     │
                     │  (AutoArchive=true ise)
              POST /Archive/Preview → /Archive/Run
                     │
              GET /Reporting/*
```

### Status Akış Diyagramları (Özet)

```
FileStatus:      Processing → Success | Failed
FileLine:        Processing → Success | Failed
ReconcilStatus:  Ready → Processing → Success | Failed
EvalStatus:      Pending → Evaluating → Planned → Completed | Failed
OperationStatus: Planned → Executing → Completed | Failed
                 Blocked → (Approve) → Planned | (Reject) → Cancelled
         └─ (Expire)  ──→ (ExpirationAction'a göre)
```

### DuplicateStatus Akış Diyagramı

```
DuplicateStatus:   null / Unique → normal akış
                     |
                     ├─ Primary  → RaiseAlert(C2) üret, akışa devam et
                     |
                     └─ Secondary → RaiseAlert(C2) üret, ERKEN ÇIK
                     |
                     └─ Conflict  → RaiseAlert(C2) üret, ERKEN ÇIK
```

---

## 16. Tamamlanan Analizler (Önceki Eksik Noktalar)

### 16.1 Raporlama View SQL Mantıkları

`ReportingService`, tüm raporlama DTO'larını `_dbContext.Set<T>().AsNoTracking()` ile sorgular — EF Core bu DTO'ları `keyless entity` olarak `reporting.*` schema view'larına eşler. Hesaplanan alanların gerçek formülleri `V1_0_4__ReportingViews.sql` migration dosyasında doğrulandı:

| Alan | View | Formül |
|------|------|--------|
| `LineSuccessRatePct` | `vw_ingestion_file_overview` | `ROUND((successful_line_count / processed_line_count) * 100, 2)` — pay 0 ise 0 |
| `LineFailRatePct` | `vw_ingestion_file_overview` | `ROUND((failed_line_count / processed_line_count) * 100, 2)` |
| `CompletenessPct` | `vw_ingestion_file_overview` | `ROUND((processed_line_count / expected_line_count) * 100, 2)` |
| `ErrorRatePct` | `vw_ingestion_file_quality` | `ROUND((failed_line_count / total_line_count) * 100, 2)` |
| `DuplicateImpactPct` | `vw_ingestion_file_quality` | `ROUND(((secondary + conflict) / total_line_count) * 100, 2)` |
| `MatchRatePct` | `vw_file_recon_summary` | `ROUND((matched_line_count / total_line_count) * 100, 2)` — matched: `matched_clearing_line_id IS NOT NULL` |
| `MatchRatePct` | `vw_recon_match_rate_trend` | Günlük GROUP BY; `ROUND((SUM(matched_count) / SUM(total_line_count)) * 100, 2)` |
| `OverallMatchRatePct` | `vw_network_recon_scorecard` | `ROUND((total_matched_count / (card_lines + clearing_lines)) * 100, 2)` | **pay: sadece card tarafının matched sayısı; payda: her iki tarafın toplamı** → teorik maksimum ~%50 |
| `CardMatchRatePct` | `vw_recon_gap_analysis` | `ROUND((card_matched_count / card_line_count) * 100, 2)` |
| `ClearingMatchRatePct` | `vw_recon_gap_analysis` | `ROUND((clearing_matched_count / clearing_line_count) * 100, 2)` |

**Tutar alanları:** `matched_amount` / `unmatched_amount` ise card tarafında `original_amount` / `settlement_amount`; clearing tarafında `source_amount` / `0` (settlement yok) olarak çözümlenir. Bu, her content type için ayrı LATERAL join ile elde edilir (Visa → `card_visa_detail`, Msc → `card_msc_detail`, Bkm → `card_bkm_detail` vb.).

---

### 16.2 FileIngestionOrchestrator Tam Akışı

2929 satırlık implementasyon incelendi. Ana akış şu adımlardan oluşur:

#### Adım 1 — Konfigürasyon & Profil Çözümleme
`BuildProfileKey(fileType, fileContentType)` → `Profiles[profileKey]` konfigürasyon sözlüğünden `ProfileOptions` ve `ParsingOptions` çekilir.

#### Adım 2 — Boundary Record Okuma
Dosyanın başından H (header) ve sonundan F (footer) kayıtları okunur (`ReadBoundaryRecordsAsync`). Footer'dan `ExpectedCount` elde edilir.

#### Adım 3 — Duplicate Kontrolü
```
FileKey = GenerateFileKey(header, footer)   // header+footer alanlarından deterministik hash
IngestionFiles WHERE FileKey = ? AND FileName = ? AND SourceType = ? AND FileType = ?
```
Sonuç:
- **Bulunamadı →** yeni dosya akışı (`ProcessNewFileAsync`)
- **Bulundu + `RequiresFullRecovery` →** `RecoverExistingFileAsync` (byte offset'ten devam + başarısız satır yeniden deneme)
- **Bulundu + `RequiresArchiveOnlyRecovery` →** `RetryArchiveOnlyAsync`
- **Bulundu + `RequiresReArchive` →** `RetryArchiveOnlyAsync`
- **Bulundu, normal duplicate →** mevcut entity döndürülür, `DuplicateFileReceived` mesajı yazılır

#### Adım 4 — Byte Offset Tracking & Resume (Devam Ettirme)
Her satır işlenirken `lineReadResult.NextByteOffset` ve `lineReadResult.LineNumber` `ProcessingProgress`'e kaydedilir. Her batch persist edildiğinde bu değerler `IngestionFile.LastProcessedByteOffset` ve `LastProcessedLineNumber` kolonlarına güncellenir.

Recovery sırasında:
```csharp
if (file.LastProcessedByteOffset > 0)
    await SkipToOffsetAsync(stream, file.LastProcessedByteOffset, cancellationToken);
```
Stream, son işlenen byte konumuna seek edilerek kaldığı yerden devam edilir. Bu mekanizma `ContinueMissingRowsAsync` içinde çalışır; koşul `file.ExpectedCount > file.TotalCount`.

#### Adım 5 — Satır İşleme & Batch Persist
Satırlar `ReadLinesWithByteOffsetsAsync` ile okunur → `FixedWidthRecordParser.Parse` → `ParsedRecordModelMapper.Create` → `BuildIngestionFileLine`. Batch dolduğunda (varsayılan **50.000** satır, `ProcessingOptions.BatchSize`) `PersistBatchAsync` çağrılır. PostgreSQL'de bulk insert için Npgsql `COPY` kullanılır; MSSQL'de `SqlBulkCopy`.

#### Adım 6 — Başarısız Satır Yeniden Deneme
`RetryFailedRowsAsync`: `Status = Failed AND RetryCount < MaxRetryCount (varsayılan 3)` olan satırları sıralar. Her satır için `ByteOffset` + `ByteLength` ile ham veri doğrudan kaynak dosyadan okunur (`ReadRangeAsync`), yeniden parse edilir ve entity güncellenir.

#### Adım 7 — Paralel İşleme
`EnableParallelProcessing = true` ve birden fazla dosya olduğunda `SemaphoreSlim(MaxDegreeOfParallelism)` ile scoped DI kapsamlarında paralel çalışır (varsayılan **8 worker**).

#### Adım 8 — Arşiv & Finalize
Satırlar işlenirken `TargetWriter` (arşiv dosyası) eş zamanlı yazılır. Tamamlayıcı adımlar: `FinalizeAsync` → `IsArchived` güncelleme → `FinalizeFileStateAsync` (dosya durumu `Completed`/`Failed` geçişi).

---

### 16.3 EvaluatorResolver ve Content-Type Bazlı Evaluator'lar

`EvaluatorResolver.Resolve(contentType)` DI'dan enjekte edilen `IEnumerable<IEvaluator>` içinden `evaluator.CanEvaluate(contentType) == true` olan ilkini seçer. Kayıtlı üç evaluator:

#### BkmEvaluator (`FileContentType.Bkm`)

Tam implement edilmiş; ~870 satır. Karar ağacı (kanonik karşılık: [`BkmEvaluator.cs`](LinkPara.Card.Infrastructure/Services/Reconciliation/Evaluate/Flows/BkmEvaluator.cs)):

```
0. Root detail çözümlenemiyorsa
   → ReconciliationCurrentCardRowMissingException (üst katman: row Failed + alarm)

1. C1 — File length / parse validation hatası (RootRow.Status == Failed)
   → RaiseAlert  [decision: C1]
   → STOP (NoAction haricinde aksiyon yok)

2. C2 — Duplicate / uniqueness değerlendirmesi
   Conflict   → RaiseAlert + STOP
   Secondary  → RaiseAlert + STOP   (primary işlenecek; bu skip edilir)
   Primary    → RaiseAlert + DEVAM  (bu kayıt asıl işlenen; alarm bilgi amaçlı)
   None       → DEVAM

3. CLEARING-PRESENCE GATE  (BuildAwaitingClearingResult)
   ClearingDetails.Count == 0
   → MarkAwaitingClearing("AWAIT_CLEARING")
   → RaiseAlert (bilgilendirme; manual review YOK)
   → EvaluateService satırı ReconciliationStatus.AwaitingClearing'e taşır
   → Clearing dosyası ileride başarıyla ingestion edildiğinde
     ClearingArrivalRequeueService satırı tekrar Ready'e çeker
     ve aynı kayıt eksiksiz veriyle yeniden değerlendirilir.

4. C3 — Cancel / Reversal (TxnStat = Reverse | Void)
   Orijinal txn (OceanMainTxnGuid) bulunamadı
     → RaiseAlert  (defansif; otomatik düzeltme YAPILMAZ)
   Orijinal zaten IsCancelled = true
     → NoAction (yalnızca not)
   Orijinal canlı
     → ReverseOriginalTransaction  [D7]
       (handler içinde: orijinali ters çevir + IsCancelled=1)

5. C5 — File transaction status branch'i
   ResolveFileTransactionStatus(detail) →
     TxnStat = Expired                                 → Expired
     IsSuccessfulTxn = Successful AND ResponseCode=00  → Successful
     diğer her şey                                     → Failed

6. EvaluateFailedBranch (file Failed)
   payify Failed     → NoAction
   payify Missing    → NoAction
   payify Successful → CorrectResponseCode + ConvertToFailed + ReverseByBalanceEffect  [D1]

7. EvaluateExpireBranchAsync (file Expired)
   payify Failed     → MoveToExpired                                          [C7]
   payify Missing    → CreateTransaction + MoveCreatedTransactionToExpired    [C8]
   payify Successful:
     ACC pending match (ControlStat = Problem ve alanlar uyuyor)
       → RaiseAlert (manuel müdahale Paycore tarafında)                       [C10]
     ACC pending match yok
       → MoveToExpired + ReverseByBalanceEffect                               [D2]

8. EvaluateSuccessfulBranch (file Successful)
   IsTxnSettle != Settled                          → NoAction
   payify Failed
     → CorrectResponseCode + ConvertToSuccessful + ReverseByBalanceEffect    [D3]
   payify Missing:
     refund değil → CreateTransaction + ApplyOriginalEffectOrRefund          [C13 + D4]
     refund      → CreateTransaction + EvaluateRefund                        [C13]
   payify Successful:
     refund      → EvaluateRefund
     latestEmoney null            → NoAction (defansif not)
     amount == billing            → NoAction
     amount  < billing            → InsertShadowBalanceEntry +
                                    RunShadowBalanceProcess  (fark kadar)    [D8]
     amount  > billing            → NoAction
                                    (Kural Kodu'nda EVET dalı boş)

9. EvaluateRefund (yardımcı; C13/payify Successful akışlarından çağrılır)
   IsMatchedRefund (MainTxnGuid > 0 ve mevcutten farklı)
     → ApplyLinkedRefund                                                     [D5]
   Eşleniksiz (unmatched)
     → CreateManualReview gate +
         Approve → ApplyUnlinkedRefundEffect
         Reject  → StartChargeback                                            [D6]
       (Tek manual review noktası; Approve/Reject branch'leri executor'da
        gerçekleştirilir; bkz. §4.7 / §4.8)
```

**ACC pending match (`HasAccPendingMatchAsync` + `IsAccFieldMatch`):** Clearing detayları içinde `ControlStat = Problem` olan ve aşağıdaki alanların tamamı uyuşan kayıt aranır: RRN (her iki tarafta da doluysa), CardNo, ProvisionCode, ARN, MCC, CardHolderBillingAmount (2 hane yuvarlama), CardHolderBillingCurrency.

**PayifyStatus çözümlemesi (`ResolvePayifyStatus`):**
- `transactions.Count == 0` → **Missing**
- Status = `"Failed"` → **Failed**
- Status = `"Completed"` veya `"Success"` → **Successful**
- Diğer / bilinmeyen → defansif olarak **Missing** (yanlış otomatik düzeltmeyi engellemek için)

**Sonuç tipleri matrisi:**

| Branch sonucu | Anlamı |
|---|---|
| **NoAction** | Yalnızca açıklayıcı `Note`; sistem hiçbir şey tetiklemez. Tutarlı durum veya ileride netleşecek case. |
| **Alert** | `RaiseAlert` üretilir; otomatik düzeltme yapılmaz. İnsan dikkatine sunulur. |
| **AutoOperation** | Bir veya daha fazla otomatik operasyon planlanır; sırası executor garantilidir. |
| **ManualOperation** | `CreateManualReview` gate + Approve / Reject branch'leri planlanır. Kuralda yalnızca **D6 (eşleniksiz iade)** branch'inde kullanılır. |
| **AwaitingClearing** | `Result.IsAwaitingClearing = true`; satır geri planda `Ready ↔ AwaitingClearing` döngüsünde tutulur. |

#### VisaEvaluator (`FileContentType.Visa`)
**Kural tanımlanmamış (stub).** `EvaluateAsync` yalnızca `result.SetNote("Reconciliation.Visa.RulesNotDefined")` döndürür; hiçbir operasyon üretmez.

#### MscEvaluator (`FileContentType.Msc`)
**Kural tanımlanmamış (stub).** `EvaluateAsync` yalnızca `result.SetNote("Reconciliation.Msc.RulesNotDefined")` döndürür; hiçbir operasyon üretmez.

> **Önemli:** Visa ve Msc content type'ları için evaluate çağrısı başarıyla tamamlanır ancak hiçbir operasyon planlanmaz. Bu dosya türleri için mutabakat operasyonları henüz üretim aşamasına gelmemiştir.

---

### 16.4 IContextBuilder (ContextBuilder) İmplementasyonu

`ContextBuilder.BuildManyAsync(rows, errors, ct)` şu tabloları kullanır:

| Adım | Tablo | Amaç |
|------|-------|------|
| 1 | `ingestion.file` | `IngestionFileId`'lerden `IngestionFile` nesneleri yüklenir (content type bilgisi için) |
| 2 | `ingestion.file_line` | `CorrelationKey + CorrelationValue` çiftine göre ilgili tüm satırlar çekilir; `RecordType = "D"` filtresi uygulanır; 5.000'lik batch'ler halinde |
| 3 | Emoney servisi (`IEmoneyService`) | `CorrelationKey = "OceanTxnGuid"` olan satırlar için `GetByCustomerTransactionIdAsync` çağrısı yapılır; max 8 eş zamanlı istek (`SemaphoreSlim`) |

**Context nesnesi oluşturma:**
```
ContentType.Bkm  → BkmEvaluationContext
  CardDetails    = correlated rows WHERE FileType=Card    + Deserialize<CardBkmDetail>
  ClearingDetails = correlated rows WHERE FileType=Clearing + Deserialize<ClearingBkmDetail>

ContentType.Visa → VisaEvaluationContext  (CardVisaDetail / ClearingVisaDetail)
ContentType.Msc  → MscEvaluationContext   (CardMscDetail / ClearingMscDetail)
```

`CorrelationValue` boş olan satırlar context'e dahil edilmez; `errors` listesine kaydedilmez (sessizce atlanır). Emoney lookup başarısız olursa satır context'e eklenmez; `EMONEY_LOOKUP_FAILED` hatası `errors` listesine yazılır.

---

### 16.5 ArchiveAggregateReader İmplementasyonu

#### `ResolveCandidateFileIdsAsync(fileIds, beforeDate, limit, ct)`
```sql
SELECT id FROM ingestion.file
WHERE (@fileIds IS EMPTY OR id IN @fileIds)
  AND (@beforeDate IS NULL OR create_date <= @beforeDate)
ORDER BY COALESCE(update_date, create_date), id
LIMIT @limit
```
- `fileIds` boş array gelirse tüm dosyalar kapsama alınır
- Sıralama: en eski güncelleme tarihi önce (FIFO arşiv sırası)
- `limit` → büyük toplu işlemlerde sayfa sayfa çekmeyi sağlar

#### `GetSnapshotAsync(ingestionFileId, ct)`
12 ayrı paralel olmayan COUNT sorgusu çalıştırır:

| Sorgu | Tablo | Filtre |
|-------|-------|--------|
| IngestionFile | `ingestion.file` | `id = @fileId` |
| IngestionFileLine sayısı | `ingestion.file_line` | `file_id = @fileId` |
| IngestionFileLine status dağılımı | `ingestion.file_line` | `file_id = @fileId` (DISTINCT status) |
| IngestionFileLine reconciliation status | `ingestion.file_line` | `reconciliation_status IS NOT NULL` |
| CardVisaDetail sayısı | `ingestion.card_visa_detail` | `file_line_id IN (fileLineIds)` |
| CardMscDetail sayısı | `ingestion.card_msc_detail` | aynı |
| CardBkmDetail sayısı | `ingestion.card_bkm_detail` | aynı |
| ClearingVisaDetail sayısı | `ingestion.clearing_visa_detail` | aynı |
| ClearingMscDetail sayısı | `ingestion.clearing_msc_detail` | aynı |
| ClearingBkmDetail sayısı | `ingestion.clearing_bkm_detail` | aynı |
| ReconciliationEvaluation | `reconciliation.evaluation` | `file_line_id IN (...)` |
| ReconciliationOperation | `reconciliation.operation` | `file_line_id IN (...)` |
| ReconciliationReview | `reconciliation.review` | `file_line_id IN (...)` |
| ReconciliationOperationExecution | `reconciliation.operation_execution` | `file_line_id IN (...)` |
| ReconciliationAlert | `reconciliation.alert` | `file_line_id IN (...)` |
| ExistsInArchive | `archive.ingestion_file` | `id = @fileId` |

Dosyaya ait satır yoksa (`fileLineIds.Count == 0`) sadece `ExistsInArchive` kontrolü yapılır, diğer detay sorgular atlanır.

---

### 16.6 IArchiveSqlDialect — MSSQL vs PostgreSQL Farkları

`ArchiveSqlDialect`, runtime'da `Database.ProviderName.Contains("SqlServer")` ile dialect'i belirler. Temel fark yalnızca **identifier quoting**'dir:

| Özellik | MSSQL | PostgreSQL |
|---------|-------|------------|
| Identifier quote | `[kolon_adi]` | `"kolon_adi"` |
| SQL yapısı | Aynı | Aynı |

Tüm copy ve delete SQL'leri `BuildCopySql<TSource, TArchive>` / `BuildReconCopySql` / `BuildIngestionDetailCopySql` generic metodları üzerinden üretilir. EF Core model metadata'sından (`IEntityType`, `GetTableName()`, `GetSchema()`, `GetColumnName()`) gerçek tablo ve kolon adları çekilir; hard-coded string kullanılmaz.

**Copy SQL şablonu (genel yapı):**
```sql
INSERT INTO {archive_table} ({col1}, {col2}, ..., update_date, last_modified_by)
SELECT s.{col1}, s.{col2}, ..., {0} /*archivedAt*/, {1} /*archivedBy*/
FROM {source_table} s
[JOIN ingestion.file_line l ON l.id = s.file_line_id]
WHERE [l.file_id = {2} | s.id = {2}]
```

`UpdateDate` ve `LastModifiedBy` kolonları kaynak değer yerine parametre `{0}` (archivedAt timestamp) ve `{1}` (archivedBy userId) ile override edilir — arşiv anındaki audit kaydını tutar.

**Delete sırası (FK bağımlılık sırası):**
1. `reconciliation.alert`
2. `reconciliation.operation_execution`
3. `reconciliation.review`
4. `reconciliation.operation`
5. `reconciliation.evaluation`
6. `ingestion.card_visa/msc/bkm_detail` + `ingestion.clearing_visa/msc/bkm_detail`
7. `ingestion.file_line`
8. `ingestion.file`

Detail tabloları için subquery: `WHERE file_line_id IN (SELECT id FROM ingestion.file_line WHERE file_id = {0})`.

---

## 21. DuplicateStatus — Unique / Primary / Secondary / Conflict

### 21.1 Nedir ve Nerede Kullanılır?

`DuplicateStatus`, **FileIngestion** adımında aynı dosya içinde (veya farklı dosyalarda) aynı işlem anahtarına sahip birden fazla satır tespit edildiğinde `IngestionFileLine.DuplicateStatus` alanına yazılan etiket değeridir.

Bu etiket, **Evaluate** adımında `BkmEvaluator` (ve diğer evaluator'lar) tarafından okunarak satırın mutabakat akışına sokulup sokulmayacağına karar verilir.

---

### 21.2 Değerler ve Anlamları

| Değer | Kod | Açıklama |
|-------|-----|----------|
| `Unique` | `1` | Satırın yinelenen karşılığı yoktur. Normal akışa devam eder. |
| `Primary` | `2` | Aynı `DuplicateDetectionKey`'e sahip birden fazla satır bulunmuş, **tüm kopyalar birbiriyle özdeş** (aynı imza) ve bu satır **birincil (işlenecek)** kopyadır. Normal akışa devam eder; Secondary'ler silinmiş sayılır. |
| `Secondary` | `3` | Özdeş kopya grubunda **birincil olmayan (atlanacak)** satır. `ReconciliationStatus=Failed` olarak işaretlenir, mutabakat akışına girmez. |
| `Conflict` | `4` | Aynı `DuplicateDetectionKey`'e sahip satırlar bulunmuş, **ancak içerikleri birbirinden farklı** (imzalar eşleşmiyor). Tüm satırlar `ReconciliationStatus=Failed` olarak işaretlenir ve `RaiseAlert` operasyonu tetiklenir. Manuel inceleme gerekir. |

---

### 21.3 Tespit Adımı — FileIngestion (Orchestrator)

Tespit işlemi dosya ingestion'ının **parse+insert aşaması tamamlandıktan sonra** çalışır:

```
FileIngestionOrchestrator
    │
    ├─ LoadDuplicateRowsAsync()
    │   → DuplicateDetectionKey'e göre GROUP BY → Count > 1 olan anahtarları bul
    │   → İlgili satırları LineNumber/Id sırasıyla yükle
    │
    ├─ ApplyCardDuplicateOutcomes()   (Card profilleri için)
    │   veya
    │   ApplyClearingDuplicateOutcomes() (Clearing profilleri için)
    │
    └─ DB'ye güncelle (DuplicateStatus, DuplicateGroupId, ReconciliationStatus)
```

#### DuplicateDetectionKey Nedir?

Her `IngestionFileLine` kaydı ingestion sırasında bir **`DuplicateDetectionKey`** (string) ile etiketlenir. Bu anahtar, satırın kimliğini tekil olarak tanımlayan birleşik bir değerdir (örn. OceanTxnGuid veya RRN+ARN kombinasyonu). Aynı dosya içinde bu anahtar birden fazla satırda görünüyorsa satırlar "duplicate grup" olarak değerlendirilir.

---

### 21.4 Özdeş Duplikat (Equivalent) — Primary / Secondary Atama

**Koşul:** Aynı `DuplicateDetectionKey`'e sahip tüm satırlar **imza bakımından özdeş**.

**Kart profilleri için imza karşılaştırması** (`CardDuplicateSignature`):

| Alan | Açıklama |
|------|----------|
| `CardNo` | Kart numarası |
| `Otc` | Orijinal işlem kodu |
| `Ots` | Orijinal işlem tipi |
| `CardHolderBillingAmount` | Kart sahibi fatura tutarı |

Bu dört alan tüm kopyalarda aynıysa → **özdeş duplikat**.

**Clearing profilleri için:** `ParsedData` (tüm JSON) birebir eşleşiyorsa → özdeş duplikat.

**Yapılan işlem:**

```
rows[0]       → DuplicateStatus = Primary,    ReconciliationStatus = Ready (işlenir)
rows[1..n]    → DuplicateStatus = Secondary,  ReconciliationStatus = Failed (işlenmez)

Tüm satırlar aynı DuplicateGroupId'yi paylaşır.
```

---

### 21.5 Çakışmalı Duplikat (Conflicting) — Conflict Atama

**Koşul:** Aynı `DuplicateDetectionKey`'e sahip satırlar var, ancak **imzaları birbirinden farklı** (kart tutarları veya işlem kodları uyuşmuyor / clearing JSON içerikleri farklı).

**Yapılan işlem:**

```
rows[0..n]    → DuplicateStatus = Conflict,   ReconciliationStatus = Failed (tümü işlenmez)

Tüm satırlar aynı DuplicateGroupId'yi paylaşır.
```

---

### 21.6 Evaluate Adımında DuplicateStatus'un Etkisi

`BkmEvaluator.TryHandleDuplicateRow()` her satırı evaluate etmeden önce bu alanı kontrol eder:

| DuplicateStatus | Evaluate Davranışı | Sonuç |
|----------------|-------------------|-------|
| `null` / `Unique` | Duplicate kontrolü geçildi → normal akış devam eder | — |
| `Primary` | Uyarı alert'i üretilir (`C2`) ama satır **evaluate akışına devam eder** (return false) | RaiseAlert + akış sürer |
| `Secondary` | Uyarı alert'i üretilir (`C2`) ve satır **durdurulur** (return true) | RaiseAlert + erken çıkış |
| `Conflict` | Uyarı alert'i üretilir (`C2`) ve satır **durdurulur** (return true) | RaiseAlert + erken çıkış |

> **Primary neden devam eder?** Özdeş kopyalar durumunda birincil satır gerçek ve geçerli işlemi temsil eder; Secondary'ler zaten `Failed` olarak işaretlenmiştir. Primary'nin mutabakat akışını tamamlaması beklenir — ancak operasyon ekibi bir uyarı alır.

---

### 21.7 Özet Yaşam Döngüsü

```
[FileIngestion — parse sonrası]
         │
         ├─ DuplicateDetectionKey grupları analiz edilir
         │
         ├─ Grup tüm kopya → özdeş imza
         │       ├─ rows[0]   → DuplicateStatus=Primary,    ReconciliationStatus=Ready
         │       └─ rows[1..] → DuplicateStatus=Secondary,  ReconciliationStatus=Failed
         │
         └─ Grup içinde farklı imza var
                 └─ rows[*]   → DuplicateStatus=Conflict,   ReconciliationStatus=Failed

[Evaluate — BkmEvaluator.TryHandleDuplicateRow()]
         │
         ├─ Unique / null   → geç (kontrol yok)
         ├─ Primary         → RaiseAlert(C2) üret, akışa devam et
         ├─ Secondary       → RaiseAlert(C2) üret, ERKEN ÇIK
         └─ Conflict        → RaiseAlert(C2) üret, ERKEN ÇIK

[Alert işleme]
         └─ AlertService → e-posta bildirimi (Pending → Consumed)
```



---

## 22. Raporlama Referans Kilavuzu

> Bu bolumun tam icerigi: [`docs/REPORTING_REFERENCE.md`](docs/REPORTING_REFERENCE.md)


> **Hedef kitle:** Raporları yorumlayacak ekip üyeleri (finans, operasyon, ürün)  
> **Güncelleme tarihi:** Nisan 2026  
> **Kaynak:** `reporting` şemasındaki view'lar — her rapor bir veritabanı view'ına dayalıdır ve salt okunurdur.

---

### İçindekiler

- [Genel Kavramlar](#genel-kavramlar)
- [A. File Ingestion Raporları](#a-file-ingestion-raporları)
  - [A1. Ingestion File Overview](#a1-ingestion-file-overview)
  - [A2. Ingestion File Quality](#a2-ingestion-file-quality)
  - [A3. Ingestion Daily Summary](#a3-ingestion-daily-summary)
  - [A4. Ingestion Network Matrix](#a4-ingestion-network-matrix)
  - [A5. Ingestion Exception Hotspots](#a5-ingestion-exception-hotspots)
- [B. Reconciliation Process Raporları](#b-reconciliation-process-raporları)
  - [B1. Recon Daily Overview](#b1-recon-daily-overview)
  - [B2. Recon Open Items](#b2-recon-open-items)
  - [B3. Recon Open Item Aging](#b3-recon-open-item-aging)
  - [B4. Recon Manual Review Queue](#b4-recon-manual-review-queue)
  - [B5. Recon Alert Summary](#b5-recon-alert-summary)
- [C. Reconciliation Content & Financial Raporları](#c-reconciliation-content--financial-raporları)
  - [C1. Recon Live Card Content Daily](#c1-recon-live-card-content-daily)
  - [C2. Recon Live Clearing Content Daily](#c2-recon-live-clearing-content-daily)
  - [C3. Recon Archive Card Content Daily](#c3-recon-archive-card-content-daily)
  - [C4. Recon Archive Clearing Content Daily](#c4-recon-archive-clearing-content-daily)
  - [C5. Recon Content Daily](#c5-recon-content-daily)
  - [C6. Recon Clearing ControlStat Analysis](#c6-recon-clearing-controlstat-analysis)
  - [C7. Recon Financial Summary](#c7-recon-financial-summary)
  - [C8. Recon Response Status Analysis](#c8-recon-response-status-analysis)
- [D. Archive Raporları](#d-archive-raporları)
  - [D1. Archive Run Overview](#d1-archive-run-overview)
  - [D2. Archive Eligibility](#d2-archive-eligibility)
  - [D3. Archive Backlog Trend](#d3-archive-backlog-trend)
  - [D4. Archive Retention Snapshot](#d4-archive-retention-snapshot)
- [E. Advanced Reconciliation Raporları](#e-advanced-reconciliation-raporları)
  - [E1. File Recon Summary](#e1-file-recon-summary)
  - [E2. Recon Match Rate Trend](#e2-recon-match-rate-trend)
  - [E3. Recon Gap Analysis](#e3-recon-gap-analysis)
  - [E4. Unmatched Transaction Aging](#e4-unmatched-transaction-aging)
  - [E5. Network Recon Scorecard](#e5-network-recon-scorecard)
- [Filtre Referansı](#filtre-referansı)

---

### Genel Kavramlar

Bu bölüm, raporların tamamında ortak olarak geçen terim ve değerleri açıklar.

#### DataScope
Verinin hangi ortama ait olduğunu gösterir.

| Değer | Anlamı |
|---|---|
| `Live` | Canlı (production) ortam verileri |
| `Archive` | Arşivlenmiş veriler |

#### FileContentType / Network
Ödeme ağını ifade eder.

| Değer | Anlamı |
|---|---|
| `Visa` | Visa kartı / clearing dosyaları |
| `Mastercard` | Mastercard (MSC) dosyaları |
| `Bkm` | BKM (Troy) dosyaları |

#### FileType
Dosyanın tipini belirtir.

| Değer | Anlamı |
|---|---|
| `Card` | Kart tarafı (issuer/banka) dosyası |
| `Clearing` | Ağ tarafı (network/clearing) dosyası |

#### FileStatus
Bir ingestion dosyasının işlem durumu.

| Değer | Anlamı |
|---|---|
| `Processing` (1) | Dosya henüz tamamlanmadı |
| `Failed` (2) | İşlem hatası oluştu |
| `Success` (3) | Tüm satırlar işlendi (satır bazında bireysel hatalar olabilir) |

> Tek doğruluk kaynağı: §6 ve [`FileStatus.cs`](LinkPara.Card.Domain/Enums/FileIngestion/FileStatus.cs).

#### ReconciliationStatus
Bir kart işlemi satırının mutabakat çevrim durumu.

| Değer | Anlamı |
|---|---|
| `Ready` (1) | Mutabakat için hazır; değerlendirici alabilir |
| `Failed` (2) | Değerlendirme veya persist hatası |
| `Success` (3) | Değerlendirme tamamlandı, planlanan operasyonlar oluşturuldu |
| `Processing` (4) | EvaluateService satırı claim etti |
| `AwaitingClearing` (5) | Karşılık gelen clearing kaydı henüz dosya olarak gelmemiş; karar ertelendi. Clearing dosyası başarıyla ingestion edildiğinde `ClearingArrivalRequeueService` satırı otomatik olarak `Ready`'e geri çeker (bkz. §16.3 — clearing-presence gate). |

> Tek doğruluk kaynağı: §6 ve [`ReconciliationStatus.cs`](LinkPara.Card.Domain/Enums/FileIngestion/ReconciliationStatus.cs).

#### ReconSide
Mutabakat tarafını belirtir.

| Değer | Anlamı |
|---|---|
| `Card` | Kart (issuer) tarafı satırları |
| `Clearing` | Clearing (network) tarafı satırları |

#### SeverityLevel (Hata Şiddeti)
Exception hotspot raporunda kullanılır.

| Değer | Anlamı |
|---|---|
| `LOW` (1) | Düşük hata oranı |
| `MEDIUM` (2) | Orta hata oranı |
| `HIGH` (3) | Yüksek hata oranı |
| `CRITICAL` (4) | Kritik — acil müdahale |

#### UrgencyLevel (Manuel İnceleme Aciliyeti)
Manuel review queue raporunda kullanılır.

| Değer | Anlamı |
|---|---|
| `NORMAL` (1) | Normal aciliyet — süresi içinde |
| `OVERDUE` (2) | Süresi geçmiş ancak hâlâ açık |
| `EXPIRING_SOON` (3) | Yakında expire olacak |
| `EXPIRED` (4) | Expire olmuş |

#### AlertStatus
Bir uyarının durumu.

| Değer | Anlamı |
|---|---|
| `Pending` (0) | Henüz işlenmedi |
| `Processing` (1) | Bildirim çıkışı sürüyor |
| `Consumed` (2) | Başarıyla iletildi |
| `Failed` (3) | Bildirim iletimi başarısız |
| `Ignored` (4) | Politika gereği atlandı |

#### DuplicateStatus
Bir satırın tekrar durumu.

| Değer | Anlamı |
|---|---|
| `Unique` | Tekil satır, duplikat yok |
| `Primary` | Özdeş kopya var, bu geçerli kayıt |
| `Secondary` | Özdeş kopya var, bu atılan kopyası |
| `Conflict` | Aynı anahtar için farklı içerikli kayıtlar var |

---

### A. File Ingestion Raporları

Bu raporlar, sisteme yüklenen dosyaların alım (ingestion) sürecine ilişkin operasyonel ve kalite metriklerini gösterir.

---

#### A1. Ingestion File Overview

**View:** `reporting.vw_ingestion_file_overview`

**Amaç:** Sisteme alınan her dosyanın detaylı durumunu, satır bazında başarı/hata oranlarını ve mutabakat hazırlık metriklerini tek tek gösterir. Bu rapor, bir dosyanın sağlığını anlık olarak değerlendirmek için kullanılır.

**Ne zaman bakılır?**
- Bir dosyanın neden tamamlanmadığını anlamak istediğinizde
- Günlük operasyonel kontrol sırasında
- Yüksek hata oranına sahip dosyaları tespit etmek için

**Filtreler:** `DataScope`, `ContentType` (ağ), `FileType`, `FileStatus`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `FileId` | Dosyanın benzersiz sistem kimliği (UUID) |
| `FileKey` | Depolama/blob key'i — dosyanın sistemdeki benzersiz yolu |
| `FileName` | Dosyanın orijinal adı |
| `SourceType` | Dosyanın nereden geldiği (SFTP, API, Manual vb.) |
| `FileType` | `Card` veya `Clearing` |
| `ContentType` | Ağ: Visa / Mastercard / Bkm |
| `FileStatus` | Dosyanın mevcut işlem durumu |
| `FileMessage` | Varsa hata veya bilgi mesajı |
| `ExpectedLineCount` | Dosya başlığında belirtilen beklenen satır sayısı |
| `ProcessedLineCount` | Şimdiye kadar parse edilerek işleme alınan satır sayısı |
| `SuccessfulLineCount` | Başarıyla tamamlanan satır sayısı |
| `FailedLineCount` | Hata ile sonuçlanan satır sayısı |
| `LastProcessedLineNumber` | En son işlenen satırın numarası |
| `LastProcessedByteOffset` | En son okunan byte konumu (yeniden başlatma için kullanılır) |
| `IsArchived` | Dosya arşivlendi mi? (`true`/`false`) |
| `FileCreatedAt` | Dosyanın sisteme alındığı zaman damgası |
| `FileUpdatedAt` | Son güncelleme zamanı |
| `LineSuccessRatePct` | `SuccessfulLineCount / ProcessedLineCount × 100` |
| `LineFailRatePct` | `FailedLineCount / ProcessedLineCount × 100` |
| `CompletenessPct` | `ProcessedLineCount / ExpectedLineCount × 100` — dosyanın ne kadarı işlendi |
| `ActualLineCount` | Veritabanında kayıtlı gerçek satır sayısı |
| `ActualSuccessLineCount` | DB'deki başarılı satır sayısı |
| `ActualFailedLineCount` | DB'deki hatalı satır sayısı |
| `ActualProcessingLineCount` | DB'de hâlâ işleniyor olan satır sayısı |
| `DuplicateLineCount` | Duplikat tespit edilen satır sayısı (Primary + Secondary + Conflict) |
| `ReconReadyCount` | Mutabakat için hazır satır sayısı |
| `ReconSuccessCount` | Mutabakatı başarıyla tamamlanan satır sayısı |
| `ReconFailedCount` | Mutabakatı başarısız satır sayısı |
| `ProcessingDurationSeconds` | Dosyanın işlenme süresi (saniye) |
| `DataScope` | `Live` veya `Archive` |

**Nasıl yorumlanır?**
- `CompletenessPct < 100` → Dosya henüz tam işlenmemiş veya yarıda kesmişse incelenmeli
- `LineFailRatePct > 5` → Kalite sorunu, kaynak dosyada veri problemi olabilir
- `DuplicateLineCount > 0` → Yükleme sırasında duplikat satır gelmiş, `A2` raporuna bakılmalı
- `ReconFailedCount > 0` → Bu dosyanın bir kısmı mutabakata girmemiş, `B2`/`B4` raporlarına bakılmalı

---

#### A2. Ingestion File Quality

**View:** `reporting.vw_ingestion_file_quality`

**Amaç:** Her dosyanın veri kalitesini ölçer; duplikat dağılımı, hata oranı ve yeniden deneme metriklerini ortaya koyar. `A1`'in kalite odaklı özet versiyonudur.

**Ne zaman bakılır?**
- Kaynak sistemde (ağ tarafında) veri kalitesini değerlendirirken
- Duplikat problemi olan dosyaları tespit ederken
- Aylık kalite raporlaması sırasında

**Filtreler:** `DataScope`, `ContentType`, `FileType`, `FileStatus`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `FileId` | Dosya kimliği |
| `FileName` | Dosya adı |
| `FileType` | Dosya tipi |
| `ContentType` | Ağ |
| `FileStatus` | Durum |
| `FileCreatedAt` | Alım zamanı |
| `TotalLineCount` | Toplam satır sayısı |
| `SuccessLineCount` | Başarılı satır sayısı |
| `FailedLineCount` | Hatalı satır sayısı |
| `ProcessingLineCount` | İşlenmekte olan satır sayısı |
| `DuplicateUniqueCount` | Duplikat olmayan (tekil) satır sayısı |
| `DuplicatePrimaryCount` | Duplikat var, ama bu geçerli kopya olan satır sayısı |
| `DuplicateSecondaryCount` | Duplikat var, atılan kopya sayısı — **bunlar mutabakata girmez** |
| `DuplicateConflictCount` | Aynı anahtar, farklı içerik — **hepsi başarısız, manuel inceleme gerekir** |
| `TotalRetryCount` | Tüm satırlardaki toplam yeniden deneme sayısı |
| `LinesWithRetryCount` | En az 1 kez yeniden denenen satır sayısı |
| `ErrorRatePct` | `FailedLineCount / TotalLineCount × 100` |
| `DuplicateImpactPct` | `(Secondary + Conflict) / TotalLineCount × 100` — kaliteye olumsuz etkiyen duplikat oranı |
| `DataScope` | Live / Archive |

**Nasıl yorumlanır?**
- `DuplicateConflictCount > 0` → Kritik! Kaynak sistemde aynı anahtara farklı tutar gelmiş
- `DuplicateImpactPct > 10` → %10'dan fazla duplikat, kaynak sistemle görüşülmeli
- `TotalRetryCount` yüksekse → Geçici hatalar veya altyapı sorunu var

---

#### A3. Ingestion Daily Summary

**View:** `reporting.vw_ingestion_daily_summary`

**Amaç:** Günlük bazda dosya yükleme hacmini ve başarı oranlarını özetler. Trend analizi için uygundur.

**Ne zaman bakılır?**
- Her sabah günlük kontrol rutinovunda
- Haftalık / aylık operasyonel raporlama sırasında
- İş yükü trendini analiz ederken

**Filtreler:** `DataScope`, `ContentType`, `FileType`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `ReportDate` | Raporun ait olduğu tarih |
| `ContentType` | Ağ (Visa/MSC/BKM) |
| `FileType` | Kart / Clearing |
| `FileCount` | O gün alınan toplam dosya sayısı |
| `SuccessFileCount` | Başarıyla tamamlanan dosya sayısı |
| `FailedFileCount` | Hata veren dosya sayısı |
| `ProcessingFileCount` | Hâlâ işlenmekte olan dosya sayısı |
| `ExpectedLineCount` | O günkü dosyaların toplam beklenen satır sayısı |
| `ProcessedLineCount` | İşlenen toplam satır sayısı |
| `SuccessfulLineCount` | Başarılı toplam satır sayısı |
| `FailedLineCount` | Hatalı toplam satır sayısı |
| `ProcessedLineSuccessRatePct` | `SuccessfulLineCount / ProcessedLineCount × 100` |
| `DataScope` | Live / Archive |

**Nasıl yorumlanır?**
- `FailedFileCount > 0` → Hatalı dosyalar `A1`'de detaylı incelenmeli
- `ProcessedLineSuccessRatePct < 95` → Kritik eşik, köken analizi yapılmalı
- `ProcessingFileCount` gün sonunda sıfırlanmıyorsa → Takılan bir process var

---

#### A4. Ingestion Network Matrix

**View:** `reporting.vw_ingestion_network_matrix`

**Amaç:** Her ağ ve dosya tipi kombinasyonu için kümülatif dosya ve satır istatistiklerini gösterir. "Hangi ağdan ne kadar veri gelmiş?" sorusunu yanıtlar.

**Ne zaman bakılır?**
- Ağ bazında iş yükü dağılımını görmek istediğinizde
- Yeni bir ağ bağlantısının ilk verilerini doğrularken
- Kurumsal raporlama için ağ bazlı özet gerektiğinde

**Filtreler:** `DataScope`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `ContentType` | Ağ (Visa/MSC/BKM) |
| `FileType` | Kart / Clearing |
| `FileCount` | Toplam dosya sayısı |
| `SuccessFileCount` | Başarılı dosya sayısı |
| `FailedFileCount` | Hatalı dosya sayısı |
| `ProcessedLineCount` | Toplam işlenen satır sayısı |
| `SuccessfulLineCount` | Toplam başarılı satır sayısı |
| `FailedLineCount` | Toplam hatalı satır sayısı |
| `FirstFileAt` | Bu ağ/tip kombinasyonunun ilk dosya alım zamanı |
| `LastFileAt` | En son dosya alım zamanı |
| `DataScope` | Live / Archive |

---

#### A5. Ingestion Exception Hotspots

**View:** `reporting.vw_ingestion_exception_hotspots`

**Amaç:** En fazla hata üreten dosyaları öne çıkarır. Hata yoğunluğuna göre sıralı liste sunar; kritik dosyaların hızla tespit edilmesini sağlar.

**Ne zaman bakılır?**
- Olağandışı hata artışı alarm geldiğinde
- Hangi dosyaların sistemi zorladığını anlamak için
- SLA ihlali riskini değerlendirirken

**Filtreler:** `DataScope`, `ContentType`, `FileType`, `SeverityLevel`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `FileId` | Dosya kimliği |
| `FileName` | Dosya adı |
| `SourceType` | Kaynak tip |
| `FileType` | Kart / Clearing |
| `ContentType` | Ağ |
| `FileStatus` | Dosya durumu |
| `FileMessage` | Hata mesajı |
| `FileCreatedAt` | Alım zamanı |
| `FailedLineCount` | Hatalı satır sayısı |
| `ProcessedLineCount` | İşlenen satır sayısı |
| `TotalRetryCount` | Tüm hatalı satırların toplam yeniden deneme sayısı |
| `MaxRetryCount` | En fazla denenen satırın deneme sayısı |
| `DistinctErrorMessageCount` | Kaç farklı hata mesajı türü var |
| `SeverityLevel` | Otomatik hesaplanan hata şiddeti (`Low`/`Medium`/`High`) |
| `DataScope` | Live / Archive |

**Nasıl yorumlanır?**
- `SeverityLevel = High` + `DistinctErrorMessageCount > 5` → Çok farklı hata tipi var, sistemik bir problem olabilir
- `MaxRetryCount` yüksekse → Belirli satırlar sürekli başarısız oluyor, veri problemi

---

### B. Reconciliation Process Raporları

Bu raporlar, mutabakat motorunun çalışma durumunu, açık kalemleri ve alert yönetimini gösterir.

---

#### B1. Recon Daily Overview

**View:** `reporting.vw_recon_daily_overview`

**Amaç:** Mutabakat motorunun günlük performansını tek bir özet satırla sunar. Evaluation, operation ve execution katmanlarının tamamını kapsar.

**Ne zaman bakılır?**
- Her sabah operasyonel durum kontrolü için
- Haftalık yönetim raporlaması için
- Performans trendini izlemek için

**Filtreler:** `DataScope`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `ReportDate` | Tarihi |
| `TotalEvaluationCount` | O gün başlatılan toplam evaluation sayısı |
| `CompletedEvaluationCount` | Tamamlanan evaluation sayısı |
| `FailedEvaluationCount` | Başarısız evaluation sayısı |
| `TotalOperationCount` | Toplam operation sayısı |
| `CompletedOperationCount` | Tamamlanan operation sayısı |
| `FailedOperationCount` | Başarısız operation sayısı |
| `BlockedOperationCount` | Bloklanmış (beklemede) operation sayısı |
| `PlannedOperationCount` | Planlanmış (henüz başlamamış) operation sayısı |
| `ManualOperationCount` | Manuel müdahale gerektiren operation sayısı |
| `TotalExecutionCount` | Toplam execution (çalıştırma girişimi) sayısı |
| `CompletedExecutionCount` | Başarılı execution sayısı |
| `FailedExecutionCount` | Başarısız execution sayısı |
| `AvgExecutionDurationSeconds` | Ortalama execution süresi (saniye) |
| `PendingReviewCount` | Karar bekleyen manuel review sayısı |
| `ApprovedReviewCount` | Onaylanan review sayısı |
| `RejectedReviewCount` | Reddedilen review sayısı |
| `PendingAlertCount` | Çözüme kavuşmamış alert sayısı |
| `FailedAlertCount` | İletim başarısız olan alert sayısı |
| `OperationSuccessRatePct` | `CompletedOperationCount / TotalOperationCount × 100` |
| `DataScope` | Live / Archive |

**Nasıl yorumlanır?**
- `OperationSuccessRatePct < 90` → Kritik, `B2` ve `B4` raporlarına derhal bakılmalı
- `BlockedOperationCount > 0` → Bloklanmış kalemler `B2`'de incelenmeli
- `PendingReviewCount` artıyorsa → Manuel review ekibi kapasitesi yetersiz

---

#### B2. Recon Open Items

**View:** `reporting.vw_recon_open_items`

**Amaç:** Mutabakat sürecinde tamamlanmamış (açık) operation kalemlerini listeler. Her kayıt, çözüm bekleyen bir operation'ı temsil eder.

**Ne zaman bakılır?**
- Günlük açık kalem takibinde
- SLA ihlali riskini değerlendirirken
- Hangi kalemlerin müdahale gerektirdiğini belirlemek için

**Filtreler:** `OperationStatus`, `Branch`, `IsManual`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `OperationId` | Operation'ın benzersiz kimliği |
| `FileLineId` | Bu operation'a konu olan dosya satırının kimliği |
| `EvaluationId` | Bu satırın bağlı olduğu evaluation kimliği |
| `GroupId` | Gruplama kimliği (eşleşen Card-Clearing çifti) |
| `SequenceNumber` | Operation'ın grup içindeki sıra numarası |
| `ParentSequenceNumber` | Varsa, üst operation'ın sıra numarası |
| `OperationCode` | Operation tipi kodu (ör. `MATCH_CARD_CLEARING`) |
| `Branch` | Hangi işlem koluna ait (ör. `VISA_CARD_LIVE`) |
| `IsManual` | `true` → Manuel olarak oluşturulmuş |
| `OperationStatus` | Mevcut durum (`Planned`, `Executing`, `Blocked`, `Completed`, `Failed`) |
| `RetryCount` | Şimdiye kadar deneme sayısı |
| `MaxRetryCount` | İzin verilen maksimum deneme sayısı |
| `NextAttemptAt` | Sonraki otomatik deneme zamanı |
| `LeaseOwner` | Şu an bu operation'ı işleyen worker kimliği |
| `LeaseExpiresAt` | Worker kilidinin sona erme zamanı |
| `LastError` | Son hata mesajı |
| `OperationCreatedAt` | Operation'ın oluşturulma zamanı |
| `OperationUpdatedAt` | Son güncelleme zamanı |
| `EvaluationStatus` | Bağlı evaluation'ın durumu |
| `EvaluationOperationCount` | Bu evaluation altında kaç operation var |
| `AgeHours` | Operation'ın kaç saattir açık olduğu |

**Nasıl yorumlanır?**
- `AgeHours > 24` → SLA ihlali riski, acil müdahale gerekebilir
- `OperationStatus = Blocked` → Manuel müdahale veya sistem hatası, `B4`'e bakılmalı
- `RetryCount = MaxRetryCount` → Otomatik yeniden deneme kalmadı, manuel review gerekli
- `LeaseOwner` dolu ama `LeaseExpiresAt` geçmiş → Worker takılmış, lease temizlenmeli

---

#### B3. Recon Open Item Aging

**View:** `reporting.vw_recon_open_item_aging`

**Amaç:** Açık kalemleri yaşlarına göre gruplandırır (aging buckets). Kaç kalemin ne kadar süredir çözümsüz kaldığını gösterir.

**Ne zaman bakılır?**
- Haftalık SLA raporlamasında
- Eski (stale) açık kalemlerin temizlenmesi gerekip gerekmediğini değerlendirirken

**Filtreler:** Yok (anlık snapshot)

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `BucketName` | Yaş aralığı etiketi — ör. `0-1h`, `1-6h`, `6-24h`, `1-3d`, `3-7d`, `7d+` |
| `ItemCount` | Bu yaş grubundaki toplam açık kalem sayısı |
| `PlannedCount` | Bu gruptaki `Planned` durumundaki kalemler |
| `BlockedCount` | Bu gruptaki `Blocked` durumundaki kalemler |
| `ExecutingCount` | Bu gruptaki `Executing` durumundaki kalemler |
| `ManualCount` | Bu gruptaki manuel kalemler |

**Nasıl yorumlanır?**
- `7d+` bucket'ında kalem varsa → Kritik, bu kalemler için kök neden analizi yapılmalı
- `BlockedCount` yoğunsa → Sistem seviyesinde bir blokaj var

---

#### B4. Recon Manual Review Queue

**View:** `reporting.vw_recon_manual_review_queue`

**Amaç:** Manuel inceleme bekleyen kalemleri tüm bağlamıyla (review, operation, evaluation, execution, dosya ve satır bilgisi) tek bir görünümde sunar. Bu rapor, manuel review yapacak analistler için ana çalışma ekranıdır.

**Ne zaman bakılır?**
- Günlük manuel review çalışmasında
- Acil kalem belirleme ve önceliklendirmede

**Filtreler:** `UrgencyLevel`, `OperationBranch`

**Kolonlar:**

*Review Bilgisi*

| Kolon | Açıklama |
|---|---|
| `ReviewId` | Review kaydının benzersiz kimliği |
| `Decision` | Analistin kararı: `Pending`, `Approved`, `Rejected` |
| `Comment` | Analistin yorumu |
| `DecisionAt` | Karar verilen zaman |
| `ExpiresAt` | Review'un son karar tarihi |
| `ExpirationAction` | Süre dolunca ne olacak: `AutoApprove`, `AutoReject`, `Hold` |
| `ExpirationFlowAction` | Süre dolunca akış yönlendirmesi |
| `ReviewCreatedAt` | Review'un oluşturulma zamanı |

*Operation Bilgisi*

| Kolon | Açıklama |
|---|---|
| `OperationCode` | Operation tipi |
| `OperationBranch` | İşlem kolu |
| `OperationStatus` | Operation'ın mevcut durumu |
| `OperationIsManual` | Manuel mi? |
| `OperationNote` | Operation notu |
| `OperationRetryCount` | Deneme sayısı |
| `OperationMaxRetries` | Max deneme limiti |
| `OperationNextAttemptAt` | Sonraki deneme zamanı |
| `OperationLeaseOwner` | Aktif worker |
| `OperationLeaseExpiresAt` | Kilit sona erme zamanı |
| `OperationLastError` | Son hata |
| `OperationPayload` | Operation'ın taşıdığı veri (JSON) |
| `OperationCreatedAt` | Oluşturulma zamanı |

*Evaluation Bilgisi*

| Kolon | Açıklama |
|---|---|
| `EvaluationStatus` | Evaluation sonucu |
| `EvaluationMessage` | Evaluation açıklama mesajı |
| `EvaluationOperationCount` | Bu evaluation altındaki operation sayısı |
| `EvaluationCreatedAt` | Evaluation oluşturulma zamanı |

*Son Execution Bilgisi*

| Kolon | Açıklama |
|---|---|
| `LastExecutionId` | Son execution kimliği |
| `LastAttemptNumber` | Son deneme numarası |
| `LastExecutionStatus` | Son execution durumu |
| `LastExecutionStartedAt` | Başlangıç zamanı |
| `LastExecutionFinishedAt` | Bitiş zamanı |
| `LastExecutionResultCode` | Sonuç kodu |
| `LastExecutionResultMessage` | Sonuç mesajı |
| `LastExecutionErrorCode` | Hata kodu |
| `LastExecutionErrorMessage` | Hata açıklaması |
| `TotalExecutionCount` | Toplam execution denemesi |

*Dosya Bilgisi*

| Kolon | Açıklama |
|---|---|
| `FileName` | Dosya adı |
| `FileKey` | Dosya yolu |
| `FileSourceType` | Kaynak tip |
| `FileType` | Kart / Clearing |
| `ContentType` | Ağ |
| `FileStatus` | Dosya durumu |

*Satır Bilgisi*

| Kolon | Açıklama |
|---|---|
| `LineNumber` | Dosya içindeki satır numarası |
| `LineRecordType` | Satır kayıt tipi (header/detail/trailer) |
| `LineStatus` | Satır işlem durumu |
| `LineReconciliationStatus` | Satırın mutabakat sonucu |
| `MatchedClearingLineId` | Eşleşilen clearing satırının ID'si |
| `CorrelationKey` | Eşleşme anahtarının tipi |
| `CorrelationValue` | Eşleşme anahtar değeri |
| `LineDuplicateStatus` | Duplikat durumu |
| `LineMessage` | Satır mesajı/hatası |

*Kart Satırı Verileri (Card side)*

| Kolon | Açıklama |
|---|---|
| `CardTransactionDate` | İşlem tarihi (YYYYMMDD integer) |
| `CardTransactionTime` | İşlem saati (HHMMSS integer) |
| `CardOriginalAmount` | İşlemin orijinal tutarı |
| `CardOriginalCurrency` | ISO 4217 para birimi kodu (integer, ör. 949 = TRY) |
| `CardSettlementAmount` | Takas tutarı |
| `CardBillingAmount` | Kart sahibine yansıtılan tutar |
| `CardFinancialType` | Finansal işlem tipi |
| `CardTxnEffect` | İşlemin etkisi (Debit/Credit) |
| `CardResponseCode` | Kart yanıt kodu |
| `CardIsSuccessfulTxn` | İşlem başarılı mı? |
| `CardRrn` | Retrieval Reference Number |
| `CardArn` | Acquirer Reference Number |

*Clearing Satırı Verileri (Clearing side)*

| Kolon | Açıklama |
|---|---|
| `ClearingTxnDate` | Clearing işlem tarihi |
| `ClearingTxnTime` | Clearing işlem saati |
| `ClearingIoDate` | Interchange/settlement tarihi |
| `ClearingSourceAmount` | Kaynak para birimi tutarı |
| `ClearingSourceCurrency` | Kaynak para birimi kodu |
| `ClearingDestinationAmount` | Hedef para birimi tutarı |
| `ClearingTxnType` | İşlem tipi |
| `ClearingIoFlag` | I/O yönü bayrağı |
| `ClearingControlStat` | Control status kodu |
| `ClearingRrn` | Clearing RRN |
| `ClearingArn` | Clearing ARN |

*Özet*

| Kolon | Açıklama |
|---|---|
| `WaitingHours` | Review'un kaç saattir beklendiği |
| `UrgencyLevel` | Otomatik hesaplanan aciliyet (`Low`/`Medium`/`High`/`Critical`) |
| `EffectiveError` | Görüntülenmesi önerilen etkin hata mesajı |

---

#### B5. Recon Alert Summary

**View:** `reporting.vw_recon_alert_summary`

**Amaç:** Mutabakat sistemi tarafından üretilen alert'lerin kategorik özetini sunar. Alert türü, şiddeti ve duruma göre gruplanmış sayımlar gösterilir.

**Ne zaman bakılır?**
- Sistem sağlığı günlük kontrolünde
- Tekrarlayan alert paternlerini tespit ederken
- Alert yönetimi KPI raporlamasında

**Filtreler:** `DataScope`, `Severity`, `AlertType`, `AlertStatus`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `Severity` | Alert şiddeti (`Low`, `Medium`, `High`, `Critical`) |
| `AlertType` | Alert kodu/türü (ör. `C2`, `AMOUNT_MISMATCH`) |
| `AlertStatus` | `Pending`, `Resolved`, `Failed` |
| `AlertCount` | Bu kombinasyondaki toplam alert sayısı |
| `DistinctGroupCount` | Kaç farklı grup etkilenmiş |
| `DistinctOperationCount` | Kaç farklı operation etkilenmiş |
| `FirstAlertAt` | Bu türde ilk alert zamanı |
| `LastAlertAt` | En son alert zamanı |
| `DataScope` | Live / Archive |

**Nasıl yorumlanır?**
- `AlertType = C2` → Duplikat kaynaklı uyarı (Primary satırlar için)
- `AlertStatus = Pending` yüksekse → Alert işleme pipeline'ı takılmış
- `LastAlertAt` yakın zamansa ve `AlertStatus = Pending` → Acil müdahale gerekebilir

---

### C. Reconciliation Content & Financial Raporları

Bu raporlar, Card ve Clearing taraflarının işlem içeriklerine ve finansal metriklerine odaklanır.

---

#### C1. Recon Live Card Content Daily

**View:** `reporting.vw_recon_live_card_content_daily`

**Amaç:** Canlı ortamdaki kart tarafı (issuer) satırlarını günlük bazda gruplandırarak işlem tipine, para birimine ve mutabakat durumuna göre özetler.

**Filtreler:** `Network`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `ReportDate` | Tarih |
| `DataScope` | Live / Archive |
| `Network` | Visa / MSC / BKM |
| `LineStatus` | Satır işlem durumu |
| `ReconciliationStatus` | Mutabakat durumu |
| `FinancialType` | Finansal işlem tipi (Purchase, Refund, vb.) |
| `TxnEffect` | Debit / Credit |
| `TxnSource` | İşlem kaynağı |
| `TxnRegion` | İşlem bölgesi (Domestic/International) |
| `TerminalType` | Terminal tipi (POS, ATM, E-commerce) |
| `ChannelCode` | Kanal kodu |
| `IsTxnSettle` | Takas tamamlandı mı? |
| `TxnStat` | İşlem durum kodu |
| `ResponseCode` | Yanıt kodu |
| `IsSuccessfulTxn` | Başarılı işlem mi? |
| `OriginalCurrency` | Para birimi kodu (ISO 4217 integer) |
| `TransactionCount` | Bu gruba ait işlem sayısı |
| `DistinctFileCount` | Kaç farklı dosyadan geldiği |
| `TotalCardOriginalAmount` | Toplam orijinal tutar |
| `TotalCardSettlementAmount` | Toplam takas tutarı |
| `TotalCardBillingAmount` | Toplam fatura tutarı |
| `AvgCardOriginalAmount` | Ortalama orijinal tutar |
| `MatchedCount` | Eşleşen satır sayısı |
| `UnmatchedCount` | Eşleşemeyen satır sayısı |

---

#### C2. Recon Live Clearing Content Daily

**View:** `reporting.vw_recon_live_clearing_content_daily`

**Amaç:** Canlı ortamdaki clearing (ağ) tarafı satırlarını günlük bazda işlem tipi ve para birimine göre özetler.

**Filtreler:** `Network`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `ReportDate` | Tarih |
| `DataScope` | Live / Archive |
| `Network` | Visa / MSC / BKM |
| `LineStatus` | Satır işlem durumu |
| `ReconciliationStatus` | Mutabakat durumu |
| `TxnType` | Clearing işlem tipi |
| `IoFlag` | I = Incoming (gelen), O = Outgoing (giden) |
| `ControlStat` | Ağın control status kodu |
| `SourceCurrency` | Para birimi kodu |
| `TransactionCount` | İşlem sayısı |
| `DistinctFileCount` | Kaç farklı dosyadan |
| `TotalClearingSourceAmount` | Toplam kaynak tutar |
| `TotalClearingDestinationAmount` | Toplam hedef tutar |
| `AvgClearingSourceAmount` | Ortalama kaynak tutar |
| `MatchedCount` | Eşleşen satır sayısı |
| `UnmatchedCount` | Eşleşemeyen satır sayısı |

---

#### C3. Recon Archive Card Content Daily

**View:** `reporting.vw_recon_live_card_content_daily` (archive scope)

**Amaç:** Arşivlenmiş dönemlere ait kart tarafı satırlarının aynı içerik analizini sunar. `C1` ile yapı aynıdır, DataScope `Archive` olarak döner.

> Kolon açıklamaları için bkz. [C1](#c1-recon-live-card-content-daily)

---

#### C4. Recon Archive Clearing Content Daily

**View:** `reporting.vw_recon_live_clearing_content_daily` (archive scope)

**Amaç:** Arşivlenmiş dönemlere ait clearing tarafı satırlarının içerik analizini sunar. `C2` ile yapı aynıdır.

> Kolon açıklamaları için bkz. [C2](#c2-recon-live-clearing-content-daily)

---

#### C5. Recon Content Daily

**View:** `reporting.vw_recon_content_daily`

**Amaç:** Kart ve Clearing taraflarını tek bir görünümde birleştirir. `Side` kolonu ile hangi tarafın verisi olduğu ayrıştırılır. İki tarafın karşılaştırmalı analizine olanak tanır.

**Filtreler:** `DataScope`, `Network`, `Side`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `ReportDate` | Tarih |
| `DataScope` | Live / Archive |
| `Network` | Ağ |
| `Side` | `Card` veya `Clearing` |
| `LineStatus` | Satır durumu |
| `ReconciliationStatus` | Mutabakat durumu |
| `TransactionCount` | İşlem sayısı |
| `DistinctFileCount` | Farklı dosya sayısı |
| `MatchedCount` | Eşleşen sayısı |
| `UnmatchedCount` | Eşleşemeyen sayısı |
| `TotalCardOriginalAmount` | Toplam kart orijinal tutar (Card side için) |
| `TotalCardSettlementAmount` | Toplam kart takas tutarı |
| `TotalCardBillingAmount` | Toplam kart fatura tutarı |
| `TotalClearingSourceAmount` | Toplam clearing kaynak tutarı |
| `TotalClearingDestinationAmount` | Toplam clearing hedef tutarı |

**Nasıl yorumlanır?**
- `Side = Card` ve `Side = Clearing` satırlarını aynı gün/ağ için karşılaştırın
- `MatchedCount` her iki tarafta da eşit olmalı (bire bir eşleşme)
- Tutar farkı varsa finansal uzlaştırma gerekebilir

---

#### C6. Recon Clearing ControlStat Analysis

**View:** `reporting.vw_recon_clearing_controlstat_analysis`

**Amaç:** Clearing dosyalarındaki `ControlStat` (control status) kodlarına göre işlemleri gruplayarak hangi ControlStat değerlerinin yüksek eşleşememe oranına yol açtığını ortaya koyar.

**Filtreler:** `DataScope`, `Network`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `DataScope` | Live / Archive |
| `Network` | Ağ |
| `LineStatus` | Satır durumu |
| `ControlStat` | Clearing ağının control status kodu |
| `IoFlag` | I (gelen) / O (giden) |
| `TransactionCount` | Bu kombinasyondaki işlem sayısı |
| `TotalClearingSourceAmount` | Toplam tutar |
| `MatchedCount` | Eşleşen sayısı |
| `UnmatchedCount` | Eşleşemeyen sayısı |
| `UnmatchedRatePct` | `UnmatchedCount / TransactionCount × 100` |

**Nasıl yorumlanır?**
- `UnmatchedRatePct` yüksek olan `ControlStat` kodları → O kod için eşleşme kuralı gözden geçirilmeli
- Ağa özgü belirli ControlStat değerleri her ağda farklı anlam taşır

---

#### C7. Recon Financial Summary

**View:** `reporting.vw_recon_financial_summary`

**Amaç:** Finansal işlem tipine (`FinancialType`) ve işlem etkisine (`TxnEffect`) göre toplam tutarları ve eşleşme durumlarını özetler. Muhasebe mutabakatı için temel rapordur.

**Filtreler:** `DataScope`, `Network`, `FinancialType`, `TxnEffect`, `OriginalCurrency`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `DataScope` | Live / Archive |
| `Network` | Ağ |
| `LineStatus` | Satır durumu |
| `FinancialType` | Purchase, Refund, Chargeback, vb. |
| `TxnEffect` | `Debit` veya `Credit` |
| `OriginalCurrency` | Para birimi kodu (integer) |
| `TransactionCount` | İşlem sayısı |
| `TotalCardOriginalAmount` | Toplam orijinal tutar |
| `TotalCardSettlementAmount` | Toplam takas tutarı |
| `TotalCardBillingAmount` | Toplam fatura tutarı |
| `SettledCount` | Takas tamamlanan işlem sayısı |
| `UnsettledCount` | Henüz takas tamamlanmamış işlem sayısı |
| `DebitAmount` | Toplam borç tutarı |
| `CreditAmount` | Toplam alacak tutarı |
| `MatchedCount` | Mutabık işlem sayısı |
| `UnmatchedCount` | Mutabık olmayan işlem sayısı |

**Nasıl yorumlanır?**
- `DebitAmount - CreditAmount` → Net pozisyon; sıfıra yakın olması beklenir
- `UnsettledCount > 0` → Takası bekleyen işlemler var, muhasebe etkisi olabilir
- `UnmatchedCount` yüksekse → Tutarsız tutarlar veya eksik clearing verisi

---

#### C8. Recon Response Status Analysis

**View:** `reporting.vw_recon_response_status_analysis`

**Amaç:** Kart tarafındaki yanıt kodlarına (`ResponseCode`) ve işlem durumlarına göre mutabakat başarısını analiz eder. Hangi yanıt kodu tiplerinin eşleşememe sorununa yol açtığını gösterir.

**Filtreler:** `DataScope`, `Network`, `ReconciliationStatus`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `DataScope` | Live / Archive |
| `Network` | Ağ |
| `LineStatus` | Satır durumu |
| `ResponseCode` | Kart yanıt kodu (ör. `00` = Approved, `05` = Do Not Honor) |
| `TxnStat` | İşlem durumu kodu |
| `IsSuccessfulTxn` | Başarılı işlem mi? (`Y`/`N`) |
| `IsTxnSettle` | Takas tamamlandı mı? |
| `ReconciliationStatus` | Mutabakat sonucu |
| `TransactionCount` | İşlem sayısı |
| `TotalCardOriginalAmount` | Toplam tutar |
| `MatchedCount` | Eşleşen sayısı |
| `UnmatchedCount` | Eşleşemeyen sayısı |

**Nasıl yorumlanır?**
- `IsSuccessfulTxn = Y` ama `ReconciliationStatus = Failed` → Onaylanan işlem eşleşememiş, öncelikli incelenmeli
- `ResponseCode = 00` ve yüksek `UnmatchedCount` → Onaylı işlemlerde clearing verisi eksik

---

### D. Archive Raporları

Bu raporlar, arşivleme işlemlerinin durumunu ve veri saklama metriklerini gösterir.

---

#### D1. Archive Run Overview

**View:** `reporting.vw_archive_run_overview`

**Amaç:** Her arşivleme çalıştırmasının (archive job) durumunu, süresini ve varsa hata bilgisini listeler.

**Filtreler:** `ArchiveStatus`, `ContentType`, `FileType`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `ArchiveLogId` | Arşivleme run'ının benzersiz kimliği |
| `IngestionFileId` | Arşivlenen dosyanın ID'si (nullable) |
| `FileName` | Dosya adı |
| `FileType` | Kart / Clearing |
| `ContentType` | Ağ |
| `ArchiveStatus` | `Success`, `Failed`, `Running`, `Skipped` |
| `ArchiveMessage` | Başarı veya hata mesajı |
| `FailureReasonsJson` | Varsa hata nedenlerinin JSON listesi |
| `FilterJson` | Arşivleme için uygulanan filtreler (JSON) |
| `ArchiveStartedAt` | Başlangıç zamanı |
| `ArchiveUpdatedAt` | Son güncelleme zamanı |
| `ArchiveDurationSeconds` | Tamamlanma süresi (saniye) |

**Nasıl yorumlanır?**
- `ArchiveStatus = Failed` → `FailureReasonsJson` incelenmeli
- `ArchiveDurationSeconds` beklenenden uzunsa → Veri hacmi artmış veya performans sorunu var

---

#### D2. Archive Eligibility

**View:** `reporting.vw_archive_eligibility`

**Amaç:** Her dosyanın arşivlenmeye uygun olup olmadığını değerlendirir. Henüz arşivlenmemiş ancak arşivlenebilir durumdaki dosyaları belirler.

**Filtreler:** `ContentType`, `FileType`, `ArchiveEligibilityStatus`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `FileId` | Dosya kimliği |
| `FileName` | Dosya adı |
| `FileType` | Kart / Clearing |
| `ContentType` | Ağ |
| `FileStatus` | Dosya işlem durumu |
| `IsArchived` | Arşivlenmiş mi? |
| `FileCreatedAt` | Dosyanın sisteme alım zamanı |
| `AgeDays` | Dosyanın sistemdeki yaşı (gün) |
| `TotalReconLineCount` | Mutabakat satırı toplam sayısı |
| `ReconSuccessLineCount` | Başarıyla mutabık satır sayısı |
| `ReconOpenLineCount` | Hâlâ açık (mutabakatsız) satır sayısı |
| `ArchiveEligibilityStatus` | `Eligible` (uygun), `NotEligible` (uygun değil), `AlreadyArchived` |

**Nasıl yorumlanır?**
- `ArchiveEligibilityStatus = Eligible` → Bu dosya arşivlenebilir, arşivleme işlemi başlatılabilir
- `ReconOpenLineCount > 0` → Bu dosyanın açık satırları var, arşivlemeden önce kapatılmalı
- `AgeDays` büyük ve `ArchiveEligibilityStatus = NotEligible` → Neden uygun olmadığı araştırılmalı

---

#### D3. Archive Backlog Trend

**View:** `reporting.vw_archive_backlog_trend`

**Amaç:** Günlük arşivleme iş yükü trendini gösterir. Kaç arşivleme run'ı yapıldığını ve başarı oranını gün gün izler.

**Filtreler:** `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `ReportDate` | Tarih |
| `ArchiveRunCount` | O gün yapılan toplam arşivleme run'ı |
| `SuccessRunCount` | Başarılı run sayısı |
| `FailedRunCount` | Başarısız run sayısı |
| `OtherRunCount` | Diğer durumlardaki run sayısı (Skipped vb.) |

---

#### D4. Archive Retention Snapshot

**View:** `reporting.vw_archive_retention_snapshot`

**Amaç:** Sistemdeki toplam veri hacminin anlık görüntüsünü sunar. Aktif ve arşiv tablolarındaki kayıt sayılarını gösterir. Kapasite planlaması için kullanılır.

**Filtreler:** Yok (anlık snapshot)

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `ActiveFileCount` | Canlı tablolarda aktif dosya sayısı |
| `ArchivedMarkedFileCount` | `IsArchived = true` işaretlenmiş dosya sayısı |
| `ArchiveTableFileCount` | Arşiv tablosundaki dosya sayısı |
| `ArchiveTableFileLineCount` | Arşiv tablosundaki dosya satırı sayısı |
| `ArchiveTableEvaluationCount` | Arşiv tablosundaki evaluation sayısı |
| `ArchiveTableOperationCount` | Arşiv tablosundaki operation sayısı |
| `ArchiveTableReviewCount` | Arşiv tablosundaki review sayısı |
| `ArchiveTableAlertCount` | Arşiv tablosundaki alert sayısı |
| `ArchiveTableExecutionCount` | Arşiv tablosundaki execution sayısı |
| `OldestUnarchivedFileDate` | Arşivlenmemiş en eski dosyanın tarihi |

**Nasıl yorumlanır?**
- `OldestUnarchivedFileDate` çok eskiyse → Arşivleme backlog'u birikmiş
- `ActiveFileCount` sürekli artıyorsa → Arşivleme hızı yetersiz, kapasite artırımı gerekebilir

---

### E. Advanced Reconciliation Raporları

Bu raporlar, mutabakat kalitesini ve trendini derinlemesine analiz etmek için kullanılır.

---

#### E1. File Recon Summary

**View:** `reporting.vw_file_recon_summary`

**Amaç:** Her dosya için mutabakat başarı oranını ve finansal özetini tek satırda sunar. Dosya bazlı mutabakat performansını karşılaştırmak için idealdir.

**Filtreler:** `DataScope`, `ContentType`, `FileType`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `FileId` | Dosya kimliği |
| `FileName` | Dosya adı |
| `FileType` | Kart / Clearing |
| `ContentType` | Ağ |
| `FileStatus` | Dosya durumu |
| `FileCreatedAt` | Alım zamanı |
| `DataScope` | Live / Archive |
| `TotalLineCount` | Toplam satır sayısı |
| `MatchedLineCount` | Eşleşen satır sayısı |
| `UnmatchedLineCount` | Eşleşemeyen satır sayısı |
| `MatchRatePct` | `MatchedLineCount / TotalLineCount × 100` |
| `TotalOriginalAmount` | Toplam orijinal tutar |
| `MatchedAmount` | Eşleşen işlemlerin toplam tutarı |
| `UnmatchedAmount` | Eşleşemeyen işlemlerin toplam tutarı |
| `TotalSettlementAmount` | Toplam takas tutarı |
| `ReconReadyCount` | Mutabakat için hazır satır sayısı |
| `ReconSuccessCount` | Başarıyla mutabık satır sayısı |
| `ReconFailedCount` | Başarısız mutabakat satır sayısı |
| `ReconNotApplicableCount` | Mutabakat uygulanmayan satır sayısı |

**Nasıl yorumlanır?**
- `MatchRatePct < 95` → Bu dosya için detaylı inceleme yapılmalı
- `UnmatchedAmount` büyükse → Finansal risk, öncelikli çözülmeli
- `ReconFailedCount > 0` → `B2` veya `B4` raporlarında bu dosyanın kalemleri aranmalı

---

#### E2. Recon Match Rate Trend

**View:** `reporting.vw_recon_match_rate_trend`

**Amaç:** Eşleşme oranının zaman içindeki değişimini ağ ve taraf bazında gösterir. Trendleri ve anomalileri tespit etmek için kullanılır.

**Filtreler:** `DataScope`, `Network`, `Side`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `ReportDate` | Tarih |
| `DataScope` | Live / Archive |
| `Network` | Ağ |
| `Side` | `Card` veya `Clearing` |
| `TotalLineCount` | Toplam satır sayısı |
| `MatchedCount` | Eşleşen satır sayısı |
| `UnmatchedCount` | Eşleşemeyen satır sayısı |
| `MatchRatePct` | Eşleşme oranı (%) |
| `TotalAmount` | Toplam tutar |
| `MatchedAmount` | Eşleşen tutarı |
| `UnmatchedAmount` | Eşleşemeyen tutar |

**Nasıl yorumlanır?**
- Trend düşüyorsa → Kaynak sistem veya mutabakat kurallarında değişiklik var
- Belirli bir günde ani düşüş → O günkü dosyalarda sorun var, `A1`'e bakılmalı
- Card ve Clearing side arasındaki fark → Veri üretim asimetrisi (gecikme, eksik dosya)

---

#### E3. Recon Gap Analysis

**View:** `reporting.vw_recon_gap_analysis`

**Amaç:** Kart ve Clearing tarafları arasındaki satır sayısı ve tutar farklarını (gap) günlük bazda analiz eder. "Neden tam eşleşme yok?" sorusunu yanıtlar.

**Filtreler:** `DataScope`, `Network`, `DateFrom`, `DateTo`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `ReportDate` | Tarih |
| `DataScope` | Live / Archive |
| `Network` | Ağ |
| `CardLineCount` | Kart tarafı satır sayısı |
| `ClearingLineCount` | Clearing tarafı satır sayısı |
| `LineCountDifference` | `CardLineCount - ClearingLineCount` — pozitif = fazla kart, negatif = fazla clearing |
| `CardMatchedCount` | Kart tarafında eşleşen satır sayısı |
| `ClearingMatchedCount` | Clearing tarafında eşleşen satır sayısı |
| `CardTotalAmount` | Kart tarafı toplam tutar |
| `ClearingTotalAmount` | Clearing tarafı toplam tutar |
| `AmountDifference` | `CardTotalAmount - ClearingTotalAmount` — finansal gap |
| `CardMatchRatePct` | Kart tarafı eşleşme oranı (%) |
| `ClearingMatchRatePct` | Clearing tarafı eşleşme oranı (%) |

**Nasıl yorumlanır?**
- `LineCountDifference ≠ 0` → Bir tarafta eksik/fazla dosya var
- `AmountDifference ≠ 0` → Finansal tutar farkı, muhasebe incelemesi gerektirir
- `CardMatchRatePct` vs `ClearingMatchRatePct` arasında büyük fark → Eşleşme asimetrisi, kural problemi olabilir

---

#### E4. Unmatched Transaction Aging

**View:** `reporting.vw_unmatched_transaction_aging`

**Amaç:** Eşleşemeyen işlemleri yaşlarına göre gruplandırır. Ne kadar süredirkorunduklarını ve finansal ağırlıklarını gösterir.

**Filtreler:** `DataScope`, `Network`, `Side`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `AgeBucket` | Yaş aralığı — ör. `<1d`, `1-3d`, `3-7d`, `7-30d`, `>30d` |
| `DataScope` | Live / Archive |
| `Network` | Ağ |
| `Side` | Kart / Clearing |
| `UnmatchedCount` | Bu yaş grubundaki eşleşmemiş işlem sayısı |
| `UnmatchedAmount` | Bu yaş grubundaki toplam eşleşmemiş tutar |
| `PctOfTotalUnmatched` | Tüm eşleşmemişler içindeki yüzde payı |

**Nasıl yorumlanır?**
- `>30d` bucket'ında yüksek `UnmatchedAmount` → Uzun vadeli uzlaştırma sorunu, finans departmanı uyarılmalı
- `PctOfTotalUnmatched` yüksek bir yaş bucket'ı → Sistemik bir problem o dönemde yaşanmış
- Düşük yaş bucket'larında yoğunluk → Geçici gecikmeler, genellikle otomatik çözülür

---

#### E5. Network Recon Scorecard

**View:** `reporting.vw_network_recon_scorecard`

**Amaç:** Her ağ için kümülatif mutabakat performansını tek bir özet satırda sunar. Yönetim raporlaması ve ağlar arası karşılaştırma için tasarlanmıştır.

**Filtreler:** `DataScope`, `Network`

**Kolonlar:**

| Kolon | Açıklama |
|---|---|
| `DataScope` | Live / Archive |
| `Network` | Visa / MSC / BKM |
| `TotalFileCount` | Toplam dosya sayısı |
| `TotalCardLineCount` | Toplam kart satırı sayısı |
| `TotalClearingLineCount` | Toplam clearing satırı sayısı |
| `TotalMatchedCount` | Toplam eşleşen satır sayısı |
| `TotalUnmatchedCount` | Toplam eşleşemeyen satır sayısı |
| `OverallMatchRatePct` | Genel eşleşme oranı (%) |
| `TotalCardAmount` | Toplam kart tutarı |
| `TotalClearingAmount` | Toplam clearing tutarı |
| `NetAmountDifference` | `TotalCardAmount - TotalClearingAmount` — net finansal fark |
| `AvgCardOriginalAmount` | Ortalama kart işlem tutarı |
| `AvgClearingSourceAmount` | Ortalama clearing işlem tutarı |
| `ReconSuccessLineCount` | Mutabakat başarılı satır sayısı |
| `ReconFailedLineCount` | Mutabakat başarısız satır sayısı |
| `ReconPendingLineCount` | Mutabakat bekleyen satır sayısı |
| `ReconSuccessRatePct` | `ReconSuccessLineCount / TotalCardLineCount × 100` |
| `FirstFileDate` | Bu ağın ilk dosya tarihi |
| `LastFileDate` | Bu ağın son dosya tarihi |

**Nasıl yorumlanır?**
- `OverallMatchRatePct` ağlar arasında karşılaştırılabilir → Hangi ağın daha iyi performans sergilediği görünür
- `NetAmountDifference` büyükse → O ağ için finansal inceleme gerekli
- `ReconPendingLineCount` yüksekse → Motor henüz kalemleri işlememiş veya tıkanmış

---

### Filtre Referansı

Tüm raporlarda kullanılan filtrelerin özet tablosu:

| Filtre | Tip | Açıklama |
|---|---|---|
| `DataScope` | Enum | `Live` veya `Archive` |
| `ContentType` / `Network` | Enum | `Visa`, `Mastercard`, `Bkm` |
| `FileType` | Enum | `Card`, `Clearing` |
| `FileStatus` | Enum | `Pending`, `Processing`, `Completed`, `Failed`, `PartiallyCompleted` |
| `DateFrom` | DateTime | Başlangıç tarihi (dahil) |
| `DateTo` | DateTime | Bitiş tarihi (dahil) |
| `SeverityLevel` | Enum | `Low`, `Medium`, `High` |
| `OperationStatus` | Enum | `Planned`, `Executing`, `Blocked`, `Completed`, `Failed` |
| `UrgencyLevel` | Enum | `Low`, `Medium`, `High`, `Critical` |
| `AlertStatus` | Enum | `Pending`, `Resolved`, `Failed` |
| `ArchiveEligibilityStatus` | Enum | `Eligible`, `NotEligible`, `AlreadyArchived` |
| `ReconciliationStatus` | Enum | `Ready`, `Matched`, `Failed`, `NotApplicable` |
| `Side` | Enum | `Card`, `Clearing` |
| `Branch` | String | Operation kolu (ör. `VISA_CARD_LIVE`) |
| `IsManual` | Bool | `true` = manuel operation |
| `FinancialType` | String | `Purchase`, `Refund`, `Chargeback`, vb. |
| `TxnEffect` | String | `Debit`, `Credit` |
| `OriginalCurrency` | Int | ISO 4217 para birimi kodu (ör. 949 = TRY, 840 = USD) |
| `ArchiveStatus` | String | `Success`, `Failed`, `Running`, `Skipped` |

---

*Bu doküman koddan otomatik türetilmiştir. Yeni rapor eklendiğinde güncellenmesi gerekir.*

