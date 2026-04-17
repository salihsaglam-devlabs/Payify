# LinkPara Card — Konsolide Teknik Dokümantasyon

**Oluşturulma Tarihi:** 16 Nisan 2026  
**Kaynak:** `backend.card` repository kaynak kodu + SQL view analizi  
**Dil:** Türkçe (teknik terimler İngilizce ile)  
**Kapsam:** Archive, FileIngestion, Reconciliation, Reporting — tüm endpoint, enum, view, iş kuralı ve operasyonel rehber

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
- 4.11 [Reporting Endpoint'leri (D1–D27)](#411-reporting-endpointleri)
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
| Arşiv servisi | `ArchiveService` | Infrastructure |
| Arşiv yürütücü | `ArchiveExecutor` | Infrastructure |
| Uygunluk değerlendiricisi | `ArchiveEligibilityEvaluator` | Infrastructure |
| Operasyon yürütücü | `OperationExecutor` | Infrastructure |
| Raporlama servisi | `ReportingService` | Infrastructure |

**DB Context:** `CardServiceDbContext` → `Infrastructure/Persistence/CardServiceDbContext.cs`

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
| 11–32 | ReportingController | GET | `v1/Reporting/...` (22 endpoint) | `Reconciliation:ReadAll` |

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
| `Limit` | `int?` | Hayır | Maks aday sayısı. `Math.Clamp(1,1000)` | `DefaultPreviewLimit` (5000) | Negatif → muhtemelen 0 aday |

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
4. `ResolveEffectiveLimit`: `Math.Clamp(1,1000)`
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
| `MaxFiles` | `int?` | `DefaultMaxRunCount` (50000), `Math.Clamp(1,1000)` | İşlenecek max dosya |
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

| Kod | Açıklama | Dış Servis |
|-----|----------|-----------|
| `RaiseAlert` | Alert kaydı oluşturur | Hayır |
| `CreateManualReview` | Manuel review gate | Hayır |
| `MarkOriginalTransactionCancelled` | İşlemi iptal işaretler | `EmoneyService.UpdateTransactionStatusAsync` |
| `ReverseOriginalTransaction` | Bakiye etkisini tersine çevirir | `EmoneyService.ReverseBalanceEffectAsync` |
| `CorrectResponseCode` | Response code düzeltme | `EmoneyService.CorrectResponseCodeAsync` |
| `ConvertTransactionToFailed` | İşlem → Failed | `EmoneyService.UpdateTransactionStatusAsync` |
| `ConvertTransactionToSuccessful` | İşlem → Completed | `EmoneyService.UpdateTransactionStatusAsync` |
| `ReverseByBalanceEffect` | Bakiye ters kayıt | `EmoneyService.ReverseBalanceEffectAsync` |
| `MoveTransactionToExpired` | İşlem → Expired | `EmoneyService.ExpireTransactionAsync` |
| `CreateTransaction` | Eksik işlem oluşturur (wallet binding gerekli) | `EmoneyService.CreateTransactionAsync` |
| `MoveCreatedTransactionToExpired` | Yeni oluşturulan → Expired | `EmoneyService.ExpireTransactionAsync` |
| `ApplyOriginalEffectOrRefund` | Effect/refund uygula | `EmoneyService.RefundTransactionAsync` |
| `ApplyLinkedRefund` | Eşlenikli iade | `EmoneyService.RefundTransactionAsync` |
| `ApplyUnlinkedRefundEffect` | Eşleniksiz iade | `EmoneyService.RefundTransactionAsync` |
| `StartChargeback` | Chargeback başlat | `EmoneyService.InitChargebackAsync` + `ApproveChargebackAsync` |
| `InsertShadowBalanceEntry` | Gölge bakiye kaydı | `EmoneyService.CreateShadowBalanceDebtCreditAsync` |
| `RunShadowBalanceProcess` | Gölge bakiye işlem | `EmoneyService.RunShadowBalanceProcessAsync` |
| `RecoverMissingCardRow` | Satır → Ready, yeniden kuyruğa | DB update |
| `DropMissingCardRow` | Manuel branch kapatma | Hayır |
| `ApproveAmbiguousPayifyRecord` | Satır → Ready | DB update |
| `RejectAmbiguousPayifyRecord` | Manuel branch kapatma | Hayır |
| `ApproveUnmatchedFlow` | Manuel branch kapatma | Hayır |
| `RejectUnmatchedFlow` | Manuel branch kapatma | Hayır |
| `BindOriginalTransactionAndContinue` | Satır → Ready | DB update |
| `RejectReversalRecord` | Manuel branch kapatma | Hayır |
| `ApprovePendingAccReview` | Manuel branch kapatma | Hayır |
| `RejectPendingAccReview` | Manuel branch kapatma | Hayır |
| `ApproveMissingPayifyTransaction` | Manuel branch kapatma | Hayır |
| `RejectMissingPayifyTransaction` | Manuel branch kapatma | Hayır |

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
5. Review güncellendiyse: `ReconciliationOperation.NextAttemptAt=now`, `LeaseExpiresAt=null`
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
| `CompletenessPct` | `processed/expected` | expected=0 → null/0 döner; "%0 tamamlandı" demek değil |
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
| `ErrorRatePct` | file_line COUNT (A2 view) | D1'deki `LineFailRatePct` file tablosu sayacından → farklı sonuç verebilir |
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
| `ReconciliationStatus` | Ready(1), Failed(2), Success(3), Processing(4) |

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
| Executing(2) | Şu an çalışıyor (lease aktif) |
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
(yeni satır) → Ready → Processing → Success
                                   → Failed
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

---

## 9. Business ve Teknik Kurallar Kataloğu

### Business Kuralları

1. **Sıralama zorunluluğu:** Evaluate → Execute sırası değiştirilemez. Execute öncesinde Evaluate olmadan çalıştırılacak operasyon yoktur.
2. **İkili dosya gerekliliği:** Eşleştirme için hem Card hem Clearing dosyası sisteme alınmış olmalı. Tek taraflı evaluate tüm satırları unmatched bırakır.
3. **Terminal olmayan kayıtlar arşivlenemez:** Reconciliation/operation/review/alert tamamlanmamış dosya `Archive/Run`'da skip edilir.
4. **Saklama süresi (Retention):** Varsayılan 90 gün (`RetentionDays`). Son güncelleme en az 72 saat önce (`MinLastUpdateAgeHours`).
5. **Manuel review SLA:** `ExpiresAt` yaklaşan review'lar `EXPIRING_SOON`, geçenleri `EXPIRED`. Expire olursa `ExpirationAction` (Cancel/Approve/Reject) otomatik uygulanır.
6. **Reject yorumu zorunlu:** `Reject` endpoint'inde `Comment` zorunlu (NotEmpty), `Approve`'da opsiyonel.
7. **Archive geri döndürülemez:** `Archive/Run` commit sonrası canlı veriler silinir. Geri yükleme manuel yapılmalıdır.
8. **Auto Archive:** `AutoArchiveAfterExecute=true` ise Execute sonrası uygun dosyalar otomatik arşivlenir (fire-and-forget, Task.Run).

### Teknik Kurallar

1. **HTTP 200 yeterli değildir:** `ErrorCount` ve `Errors` her zaman kontrol edilmeli.
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
12. **Raporlama view tabanlı:** Performans sorunlarında view optimizasyonu veritabanı katmanında yapılmalı.

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
| FileIngestion | `FilePath` | `string` | Koşullu | Local ise zorunlu, Remote ise boş |
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
| Reconciliation/OpenItemAging | "Açık işler ne kadar yaşlandı?" | Operasyon, yönetim | `AgeBucket`, `ItemCount` | Yaşlı bucket büyümesi | Sadece adete bakıp yaş etkisini yok saymak |
| Reconciliation/ManualReviewQueue | "Manuel onay kuyruğu ne durumda?" | Operasyon, analist | `UrgencyLevel`, `ExpiresAt`, `OperationCode` | EXPIRED/EXPIRING_SOON artışı | Sırf adede bakıp kritikliği kaçırmak |
| Reconciliation/AlertSummary | "Alert yükü nerede yoğun?" | Operasyon, teknik | `AlertType`, `AlertStatus`, `Severity` | Pending/Failed yığılması | Info ve Error alert'i aynı ağırlıkta yorumlamak |
| Reconciliation/LiveCardContentDaily | "Canlı kart akışı durumu?" | Analist, yönetim | `TxnCount`, `Amount`, `Network` | Ani hacim/tutar kırılması | Sezon etkisini hesaba katmamak |
| Reconciliation/ArchiveCardContentDaily | "Arşiv kart geçmişi trendi?" | Analist | `TxnCount`, `Amount`, `ReportDate` | Tarihsel trendde keskin sapma | Live ve archive kıyasını aynı pencerede yapmamak |
| Reconciliation/LiveClearingContentDaily | "Canlı clearing dengeli mi?" | Operasyon, analist | `TxnCount`, `Amount`, `Network` | Card tarafıyla korelasyon bozulması | Tek taraf ile kesin karar vermek |
| Reconciliation/ContentDaily | "Card/Clearing birleşik görünüm?" | Yönetim, analist | `DataScope`, `Side`, `Amount` | Side bazında kalıcı fark | Side filtresi olmadan karar vermek |
| Reconciliation/ClearingControlStatAnalysis | "Clearing kontrol statüleri?" | Operasyon, teknik | `ControlStat`, `Count`, `Network` | Problemli statüde yoğunlaşma | Statü anlamını bilmeden aksiyon almak |
| Reconciliation/FinancialSummary | "Finansal özet ne durumda?" | Yönetim, finans | `FinancialType`, `TxnEffect`, `Amount` | Beklenmeyen effect yönü | Para birimi normalize etmeden kıyaslamak |
| Reconciliation/ResponseStatusAnalysis | "Response code sorunu var mı?" | Teknik, operasyon | `ResponseCode`, `ReconciliationStatus`, `Count` | Başarısız kod yoğunluğu | Geçici dış servis sorununu kalıcı rule hatası sanmak |
| Archive/RunOverview | "Arşiv koşuları nasıl?" | Operasyon, yönetim | `ArchiveStatus`, `ArchivedCount`, `FailedCount` | Failed run zinciri | Tek başarılı koşuyla sürdürülebilirlik varsaymak |
| Archive/Eligibility | "Hangi dosya neden uygun değil?" | Operasyon, analist | `ArchiveEligibilityStatus`, `FileType`, `ContentType` | FILE_NOT_COMPLETE/RECON_PENDING yığılması | Uygunsuz dosyayı zorla run'a almak |
| Archive/BacklogTrend | "Arşiv backlog'u artıyor mu?" | Yönetim, operasyon | `BacklogCount`, `Date` | Backlog sürekli yukarı | Kısa dönem düşüşü kalıcı iyileşme sanmak |
| Archive/RetentionSnapshot | "Canlı/arşiv dağılımı?" | Yönetim, teknik | `ActiveFileCount`, `ArchivedFileCount` | Active tarafta birikim | Yeni dosya etkisini hesaba katmadan alarm üretmek |
| Advanced/FileReconSummary | "Dosya bazında match performansı?" | Analist, operasyon | `MatchRatePct`, `MatchedCount`, `UnmatchedCount` | Match oranında kalıcı düşüş | Tek dosya ile bütün network değerlendirmesi |
| Advanced/MatchRateTrend | "Eşleşme oranı iyileşiyor mu?" | Yönetim, analist | `MatchRatePct`, `Date`, `Network` | Trend aşağı kırılım | Gecikmeli eşleşme etkisini hesaba katmamak |
| Advanced/GapAnalysis | "Card-Clearing farkı nerede büyüyor?" | Operasyon, analist | `LineCountDifference`, `AmountDifference`, `Network` | Farkların eşik üstü kalması | Kur/fx/zaman farkını kayıp sanmak |
| Advanced/UnmatchedTransactionAging | "Eşleşmeyenler ne kadar yaşlı?" | Operasyon | `AgeBucket`, `UnmatchedCount`, `Side` | Yaşlı eşleşmeyen artışı | Yeni ile eskiyi aynı ele almak |
| Advanced/NetworkScorecard | "Ağ bazında kalite skoru?" | Yönetim, operasyon | `OverallMatchRatePct`, `ReconSuccessRatePct` | Skor sürekli düşüşte | Score bileşenlerini görmeden tek metrikle karar |

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

1. Sadece HTTP 200'e bakmak — `ErrorCount` ve `Errors` kontrol edilmeli.
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
ReviewDecision:  Pending → Approved | Rejected | Cancelled
AlertStatus:     Pending → Processing → Consumed | Failed | Ignored
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
| `OverallMatchRatePct` | `vw_network_recon_scorecard` | `ROUND((total_matched_count / (card_lines + clearing_lines)) * 100, 2)` — **pay: sadece card tarafının matched sayısı; payda: her iki tarafın toplamı** → teorik maksimum ~%50 |
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
Tam implement edilmiş; 769 satır. Karar ağacı:

```
1. FileLengthValidation hatası (row.Status = Failed)
   → RaiseAlert [C1]

2. Duplicate satır kontrolü (DuplicateStatus alanı)
   Conflict/Secondary → RaiseAlert [C2], dur
   Primary → RaiseAlert [C2], devam et

3. Payify ambiguity (emoneyTransactions.Count > 1 veya bilinmeyen status)
   → RaiseAlert + CreateManualReview [C19]
   Approve: ApproveAmbiguousPayifyRecord
   Reject:  RejectAmbiguousPayifyRecord

4. Cancel/Reversal (TxnStat = Reverse | Void)
   OceanMainTxnGuid → orijinal txn aranır
   Missing/Ambiguous → RaiseAlert + CreateManualReview [C3]
   Found + zaten Cancel → no-op
   Found + değil → MarkOriginalTransactionCancelled + ReverseOriginalTransaction [D7]

5. Failed (IsSuccessfulTxn != Successful AND ResponseCode != "00")
   PayifyStatus = Failed   → no-op (zaten başarısız)
   PayifyStatus = Missing  → no-op
   PayifyStatus = Success  → CorrectResponseCode + ConvertToFailed + ReverseByBalanceEffect [D1]

6. Expired (TxnStat = Expired)
   PayifyStatus = Failed  → MoveToExpired [C7]
   PayifyStatus = Missing → CreateTransaction + MoveCreatedToExpired [C8]
   AccPending match var   → RaiseAlert + CreateManualReview [C10]
   ClearingDetails boş   → RaiseAlert + CreateManualReview [D2_ACC_PENDING]
   Diğer                  → MoveToExpired + ReverseByBalanceEffect [D2]

7. DataAnomaly (IsSuccessful=true & ResponseCode!="00" veya tersi)
   → RaiseAlert + CreateManualReview [DATA_ANOMALY]

8. Successful (IsSuccessfulTxn = Successful AND ResponseCode = "00")
   NOT Settled (IsTxnSettle != Settled) → no-op
   ClearingDetails boş (clearing henüz gelmedi)
   → RaiseAlert + CreateManualReview [ACC_MISSING]
   PayifyStatus = Failed → CorrectResponseCode + ConvertToSuccessful + ReverseByBalance [D3]
   PayifyStatus = Missing + Refund → CreateTransaction + EvaluateRefund [C13]
   PayifyStatus = Missing + diğer → CreateTransaction + ApplyOriginalEffectOrRefund [D4]
   Refund (linked) → ApplyLinkedRefund [C17]
   Refund (unlinked) → CreateManualReview [C16]
     Approve: ApplyUnlinkedRefundEffect
     Reject:  StartChargeback
   latestEmoney null → RaiseAlert + CreateManualReview [C19]
   Amounts equal → no-op
   payify < billing → InsertShadowBalanceEntry + RunShadowBalanceProcess [D8]
   payify > billing → no-op (not)

9. Unmatched (diğer tüm durumlar)
   → RaiseAlert + CreateManualReview [UNMATCHED]
```

**AccPending eşleştirme kriteri (`IsAccFieldMatch`):** RRN, CardNo, ProvisionCode, ARN, MCC, CardHolderBillingAmount (2 hane yuvarla), CardHolderBillingCurrency alanlarının tamamı eşleşmeli. Son 20 gün içindeki clearing satırlarına bakılır.

**PayifyStatus çözümlemesi:**
- `transactions.Count == 0` → Missing
- `transactions.Count > 1` → Ambiguous
- Status = "Failed" → Failed
- Status = "Completed" | "Success" → Successful
- Diğer → Ambiguous

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

### 16.7 Notification E-posta Şablonu

`NotificationEmailService`, MassTransit `IBus` üzerinden `exchange:Notification.SendEmail` exchange'ine `SendEmail` mesajı gönderir:

```csharp
new SendEmail {
    TemplateName = templateName,          // örn. "ReconciliationAlertTemplate"
    DynamicTemplateData = { key: value }, // şablona aktarılan değişkenler
    ToEmail = toEmail
}
```

**Şablon sistemi harici bir notification servisinde** (muhtemelen SendGrid veya benzeri) yönetilir. `TemplateName` sabiti olarak `"ReconciliationAlertTemplate"` verilir; içerik `backend.card` reposunda tanımlı değildir. `DynamicTemplateData` sözlüğündeki key-value çiftleri şablon motoru tarafından yer tutucularla eşleştirilir.

Gönderim akışı:
```
ReconciliationService
  → INotificationEmailService.SendEmailAsync(templateName, data, toEmail)
    → IBus.GetSendEndpoint("exchange:Notification.SendEmail")
      → SendEmail command (MassTransit message bus)
        → [Harici Notification Microservice]
```

`AlertOptions.ToEmails` listesi boşsa döngü çalışmaz; herhangi bir mesaj bus'a gönderilmez.

---

### 16.8 SearchQueryParams Tam Yapısı

`SearchQueryParams`, `LinkPara.SharedModels.Pagination` NuGet paketinde tanımlıdır. `ReportingService.PaginateAsync` içindeki kullanımdan çıkarılan gerçek alan isimleri:

```csharp
paging.Page      // int — 1'den başlar; <1 gelirse Math.Max ile 1'e çekilir
paging.Size      // int — sayfa boyutu; Math.Clamp(1, 1000) aralığına sıkıştırılır
paging.OrderBy   // string? — sıralama kolonu adı (PaginatedList'e aktarılır)
paging.SortBy    // string? — sıralama yönü ("asc"/"desc") (PaginatedList'e aktarılır)
```

`PaginatedList<T>` constructor parametreleri: `(items, total, page, pageSize, orderBy, sortBy)`.

`skip = (page - 1) * pageSize` hesabıyla EF Core `Skip/Take` uygulanır. `OrderBy`/`SortBy` ReportingService tarafından dinamik sıralama için **kullanılmaz** — her sorgu kendi sabit `OrderBy` ifadesine sahiptir (örn. `OrderByDescending(x => x.FileCreatedAt)`). Bu alanlar yalnızca istemciye metadata olarak iletilir.

> **Not:** `SearchTerm` alanı `docs/temp/1.md` notu ile listelenmiştir ancak `ReportingService` içinde bu alana başvuru yoktur; filtreleme yalnızca tip-güvenli parametrelerle (ContentType, FileType vb.) yapılır.

---

### 16.9 Options Merge Mantığı

**Konfigürasyon katmanı** (`FileIngestionOptions`) ve **request parametreleri** (`FileIngestionRequest`) iki ayrı katmandadır; runtime merge yoktur.

`ProcessingOptions.ValidateAndApplyDefaults()` startup'ta bir kez çağrılır:

```csharp
BatchSize              ??= 50_000   // null ise default, config değerini geçmez
RetryBatchSize         ??= 10_000
FailedRowMaxRetryCount ??= 3
UseBulkInsert          ??= true
EnableParallelProcessing??= true
MaxDegreeOfParallelism ??= 8
```

`??=` operatörü sayesinde **null-coalescing atama** uygulanır: Vault/appsettings'ten gelen değer korunur; sadece yoksa default atanır. Sonradan override yoktur.

`FileIngestionRequest` (tek seferlik istek), profil seçimi için `FileType` + `FileContentType` + `FileSourceType` + `FilePath` taşır; `ProcessingOptions` değerlerini override edemez. Dolayısıyla **merge** değil, **startup-time default fill** mekanizması mevcuttur.

`ProfileOptions` konfigürasyondan (Vault) doğrudan okunur; request içinde profile override yoktur. Hangi profilin kullanılacağı `BuildProfileKey(fileType, fileContentType)` → `"Card_Bkm"`, `"Clearing_Visa"` vb. composite key ile belirlenir.

---

## 17. Operasyonel Öneriler

1. **Her zaman `ErrorCount` kontrol edin.** HTTP 200 dönmesi işlemin hatasız olduğunu garanti etmez.

2. **Evaluate çağrıları idempotent değildir** ama claim mekanizması concurrent güvenlik sağlar. Aynı dosya tekrar evaluate edildiğinde sadece `Ready` satırlar hedeflenir.

3. **Execute çağrıları lease mekanizması ile concurrent güvenlidir.** Birden fazla worker aynı anda Execute çağırabilir.

4. **Manuel review SLA takibi yapın.** `ExpiresAt` yaklaşan review'lar `ManualReviewQueue` raporunda `EXPIRING_SOON` görünür. Expire sonrası otomatik kararlar istenmeyen branch'i açabilir.

5. **Arşivlemeden önce Preview yapın.** `Archive/Run` geri dönüşü zor bir işlemdir.

6. **`ContinueOnError=false` (varsayılan) ilk hatada durur.** Toplu arşiv işlemlerinde `true` kullanılabilir; hatalı dosyalar manuel kontrol edilmeli.

7. **Raporlama verileri view tabanlıdır.** Performans sorunlarında view optimizasyonu veritabanı katmanında yapılmalıdır.

8. **Alert e-postaları `AlertOptions.Enabled` ve `ToEmails` konfigürasyonuna bağlıdır.** Alıcı listesi boşsa alert'ler gönderilmez ama `Pending` kalır.

9. **Raporları tek başına yorumlama.** Özellikle `MatchRateTrend`, `GapAnalysis`, `NetworkScorecard` birlikte okunmalıdır.

10. **`OverallMatchRatePct` (NetworkScorecard) matematiksel sınırlama var.** Paydada card+clearing toplamı kullanılıyor; eşleşme tek taraflı olduğundan oran pratikte %50 bandında kalabilir. Bu bir hesaplama tasarım sorunudur.

---

## 18. Sözlük

| Terim | Açıklama |
|-------|----------|
| **Ingestion** | Dosya sisteme alma işlemi |
| **File Line** | Dosyadaki tek bir satır/kayıt |
| **Card File** | Kart işlem dosyası (harcama, çekim, iade vb.) |
| **Clearing File** | Takas/hesaplaşma dosyası |
| **Content Type / Network** | Bkm, Msc (Mastercard), Visa — dosya formatı |
| **Evaluate** | Satırları değerlendirme, eşleştirme ve operasyon planlama |
| **Operation** | Yapılması planlanan tek bir aksiyon |
| **Execute** | Operasyonları gerçekleştirme |
| **Review** | Manuel onay/red süreci |
| **Alert** | Sistemin ürettiği uyarı |
| **Archive** | Tamamlanan verileri arşiv şemasına taşıma |
| **Matched** | Kart satırının clearing karşılığının bulunması (`matched_clearing_line_id != null`) |
| **Unmatched** | Clearing karşılığı bulunamayan kart satırı |
| **DataScope** | Live (aktif tablo) veya Archive (arşiv tablosu) |
| **Side** | Card tarafı veya Clearing tarafı (raporlarda) |
| **ControlStat** | Clearing kayıtlarının kontrol durumu (network'e özgü) |
| **Correlation Key** | Eşleştirme anahtarı (RRN, ARN vb.) |
| **Branch Operation** | Approve/Reject sonrası tetiklenen yan operasyon |
| **Lease** | Bir operasyonu işleyen worker'ın geçici kilidi |
| **Claim** | Evaluate sırasında satırın başka worker'larca alınmaması için sahiplik işareti |
| **Retry** | Geçici hatalarda işlemin tekrar planlanması |
| **Chunk** | Evaluate'de bir seferde işlenen satır grubu |
| **Idempotency Key** | Aynı işlemin iki kez uygulanmasını önleyen benzersiz anahtar |
| **Terminal Status** | İşlemin tamamlandığını gösteren nihai durum (başka geçiş yok) |
| **LIVE Data** | `ingestion.*` ve `reconciliation.*` şemalarındaki aktif veri |
| **ARCHIVE Data** | `archive.*` şemasındaki arşivlenmiş veri |
| **EvaluationRunId / GroupId** | Bir Evaluate çalışmasının tüm satırları için ortak kimlik |
| **ExpirationAction** | Review süresi dolunca otomatik alınacak karar (Cancel/Approve/Reject) |
| **AutoArchiveAfterExecute** | Execute başarısı sonrası otomatik arşivleme tetikleyici konfigürasyonu |

---

*Bu doküman `backend.card` repository kaynak kodu ve `V1_0_4__ReportingViews.sql` SQL view dosyasının analizine dayanmaktadır. Kapsam dışı: PaycoreCard, PaycoreCustomer, PaycoreDebitAuthorization, PaycoreParameters, CustomerWalletCard controller'ları.*

---

## 19. Parametre & Konfigürasyon Rehberi

> **Kapsam:** FileIngestion · Reconciliation · Archive · Reporting  
> **Konfigürasyon kaynağı:** HashiCorp Vault → `appsettings.{env}.json` → ortam değişkenleri (öncelik sırasıyla)

### 19.1 Genel Yapı ve Yükleme Mekanizması

```
Vault Secret
    └─ FileIngestion      → FileIngestionOptions  (SectionName = "FileIngestion")
    └─ Reconciliation     → ReconciliationOptions (SectionName = "Reconciliation")
    └─ Archive            → ArchiveOptions        (SectionName = "Archive")
    └─ AppConfig          → AppConfigOptions      (SectionName = "AppConfig")
```

- Vault değerleri uygulama başlangıcında okunur ve ilgili `*Options` sınıfına bağlanır.
- Her options sınıfı `ValidateAndApplyDefaults()` çağırarak eksik değerleri **built-in default** ile doldurur; geçersiz değerlerde başlatma aşamasında istisna fırlatır ve uygulama **ayağa kalkmaz**.
- `null` bırakılan alanlar default değerle çalışır; **zorla `null` göndermek yerine alanı hiç koymamak** tercih edilmelidir.

---

### 19.2 FileIngestion (`vault_fileingestion.json`)

**C# sınıfı:** `FileIngestionOptions` · **Section:** `"FileIngestion"`

#### 19.2.1 Connections (Bağlantılar)

Dosya kaynağı (`Source`) ve arşiv hedefi (`Target`) bağlantı ayarlarını içerir. Her iki endpoint de **zorunlu**dur; eksikse uygulama başlamaz.

```jsonc
{
  "Connections": {
    "Source": { /* EndpointOptions */ },
    "Target": { /* EndpointOptions */ }
  }
}
```

**EndpointOptions**

| Alan | Tip | Zorunlu | Açıklama |
|------|-----|---------|----------|
| `Protocol` | `string` | **Evet** | Aktif protokol: `"Ftp"` \| `"Sftp"` \| `"Local"` |
| `Ftp` | obje | Hayır | FTP bağlantı detayları |
| `Sftp` | obje | Hayır | SFTP bağlantı detayları |
| `Local` | obje | Hayır | Yerel dizin konfigürasyonu |

> Yalnızca `Protocol` ile belirtilen bölüm doğrulanır; diğerleri yok sayılır.

**SFTP Konfigürasyonu**

| Alan | Tip | Default | Kural | Açıklama |
|------|-----|---------|-------|----------|
| `Host` | `string` | — | Zorunlu | SFTP sunucu IP veya hostname |
| `Port` | `int` | `22` | > 0 | Port numarası |
| `Username` | `string` | — | Zorunlu | Kullanıcı adı |
| `Password` | `string` | — | — | Şifre |
| `PrivateKeyPath` | `string` | `""` | — | SSH private key dosya yolu |
| `PrivateKeyPassphrase` | `string` | `""` | — | Private key şifresi |
| `KnownHostFingerprint` | `string` | `""` | — | Sunucu parmak izi doğrulaması |
| `TimeoutSeconds` | `int` | `300` | > 0 | Bağlantı timeout (saniye) |
| `OperationTimeoutSeconds` | `int` | `600` | > 0 | Transfer operasyon timeout (saniye) |
| `RetryCount` | `int` | `3` | ≥ 0 | Yeniden deneme sayısı |
| `RetryDelaySeconds` | `int` | `10` | ≥ 0 | Denemeler arası bekleme (saniye) |
| `Paths` | `dict<string,string>` | — | — | Profil adı → uzak dizin eşlemesi |

**Paths anahtarları:** `CardBkm`, `CardMsc`, `CardVisa`, `ClearingBkm`, `ClearingMsc`, `ClearingVisa`

**FTP Konfigürasyonu**

| Alan | Tip | Default | Kural | Açıklama |
|------|-----|---------|-------|----------|
| `Host` | `string` | — | Zorunlu | FTP sunucu adresi |
| `Port` | `int` | `21` | > 0 | Port numarası |
| `Username` / `Password` | `string` | — | — | Kimlik bilgileri |
| `UsePassive` | `bool` | `true` | — | Pasif mod |
| `TimeoutSeconds` | `int` | `300` | > 0 | Timeout (saniye) |
| `RetryCount` | `int` | `3` | ≥ 0 | Yeniden deneme sayısı |
| `RetryDelaySeconds` | `int` | `10` | ≥ 0 | Bekleme (saniye) |
| `Paths` | `dict<string,string>` | — | — | Profil adı → dizin eşlemesi |

**Local Konfigürasyonu** (geliştirme/test ortamı için)

```json
"Local": {
  "Paths": {
    "CardBkm": {
      "Current": "/override/path",
      "Defaults": {
        "Linux":   "/turkonay/PROD/PTS2TURKONAY/PAYCORE_REPORTS",
        "MacOS":   "/Users/shared/turkonay/PROD/PTS2TURKONAY/PAYCORE_REPORTS",
        "Windows": "\\\\turkonay\\\\PROD\\\\PTS2TURKONAY\\\\PAYCORE_REPORTS"
      }
    }
  }
}
```

`Current` boş bırakılırsa çalışan OS'a göre `Defaults` içindeki yol kullanılır.

#### 19.2.2 Processing (İşleme)

| Alan | Tip | Default | Kural | Açıklama |
|------|-----|---------|-------|----------|
| `BatchSize` | `int` | `50000` | > 0 | DB'ye tek seferde yazılacak satır sayısı |
| `RetryBatchSize` | `int` | `10000` | > 0 | Başarısız batch'lerin retry boyutu |
| `FailedRowMaxRetryCount` | `int` | `3` | ≥ 0 | Bir satır için maksimum retry sayısı |
| `UseBulkInsert` | `bool` | `true` | — | `true` → EFCore BulkInsert; `false` → standart INSERT |
| `EnableParallelProcessing` | `bool` | `true` | — | Paralel satır işleme |
| `MaxDegreeOfParallelism` | `int` | `8` | > 0 | Maksimum paralel iş parçacığı; CPU çekirdek sayısını aşmaması önerilir |

> **Performans ipucu:** Bellek baskısı varsa `BatchSize` düşürülmeli, `RetryBatchSize` ise `BatchSize`'ın ~yarısı olarak bırakılmalıdır.

#### 19.2.3 Profiles (Profiller)

Her profil bir dosya türünü tanımlar. Anahtar: `CardBkm`, `CardMsc`, `CardVisa`, `ClearingBkm`, `ClearingMsc`, `ClearingVisa`.

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Pattern` | `string (regex)` | Dosya adı doğrulama deseni. Eşleşmeyen dosyalar atlanır. |
| `DefaultEncoding` | `string` | Dosya okuma karakter kodlaması (`"UTF-8"`, `"ISO-8859-9"` vb.) |
| `FileExtensions` | `string[]` | İşlenecek uzantılar (`[".txt", ".dat"]`) |
| `SourceDateSubfolderFormat` | `string` | Tarih bazlı alt klasör formatı — yalnızca Clearing profilleri (`"yyMMdd"`) |

**Pattern Örnekleri**

| Profil | Pattern | Örnek dosya adı |
|--------|---------|----------------|
| CardBkm / CardMsc / CardVisa | `^CARD_TRANSACTIONS_\d{8}_\d+$` | `CARD_TRANSACTIONS_20240101_001` |
| ClearingBkm | `^BKMACC\d{8}\d+$` | `BKMACC2024010101` |
| ClearingMsc | `^MSCACC\d{8}\d+$` | `MSCACC2024010101` |
| ClearingVisa | `^VISAACC\d{8}\d+$` | `VISAACC2024010101` |

**ParsingOptions**

| Alan | Açıklama |
|------|----------|
| `DetailLength` | Detay (D) satırının beklenen karakter uzunluğu |
| `HeaderPrefix` | Header satırını belirleyen ilk karakter (`"H"`) |
| `FooterPrefix` | Footer satırını belirleyen ilk karakter (`"F"`) |
| `TreatFirstLineAsHeader` | `true` → ilk satır prefix kontrolü olmaksızın header kabul edilir |
| `Records` | Kayıt tipi (`H`/`D`/`F`) → alan tanımları (her alan `Start` + `Length` ile sabit-uzunluklu satırdan okunur) |

**Kart Profilleri (D kaydı) — Temel Alanlar**

| Alan | Start | Length | Açıklama |
|------|-------|--------|----------|
| `TransactionDate` | 1 | 8 | İşlem tarihi (YYYYMMDD) |
| `TransactionTime` | 9 | 9 | İşlem saati |
| `CardNo` | 34 | 19 | Kart numarası (maskelenmiş) |
| `OceanTxnGuid` | 53 | 16 | OCEAN işlem GUID |
| `RRN` | 101 | 12 | Retrieval Reference Number |
| `ARN` | 113 | 11 | Acquirer Reference Number |
| `OriginalAmount` | 427 | 19 | Orijinal tutar |
| `SettlementAmount` | 449 | 19 | Takas tutarı |
| `BillingAmount` | 493 | 19 | Fatura tutarı |
| `MerchantName` | 280 | 25 | Üye işyeri adı |
| `MerchantId` | 339 | 15 | Üye işyeri kimliği |
| `TerminalId` | 331 | 8 | Terminal kimliği |
| `ResponseCode` | 369 | 2 | Yanıt kodu |
| `IsSuccessfulTxn` | 371 | 1 | Başarılı mı? |

*(Tüm 60+ alan için `docs/temp/Parametre_Konfigurasyon_Rehberi.md` bölüm 2.3'e bakınız.)*

**Clearing Profilleri Özet**

| Profil | DetailLength | Ek Alanlar |
|--------|-------------|------------|
| ClearingBkm | 331 | `ClrNo`, `Arn`, `Rrn`, `IoDate`, `IoFlag`, `FunctionCode`, `ProcessCode`, `ControlStat`, `DisputeCode`, `SourceAmount`, `DestinationAmount`, `ReimbursementAmount`, `CardAcceptorId`, `FileId` |
| ClearingMsc | 347 | ClearingBkm + `ReversalIndicator`, `AncillaryTransactionCode`, `AncillaryTransactionAmount` |
| ClearingVisa | 321 | ClearingBkm – `FunctionCode`/`ProcessCode` + `TC`, `UsageCode` |

---

### 19.3 Reconciliation (`vault_reconciliation.json`)

**C# sınıfı:** `ReconciliationOptions` · **Section:** `"Reconciliation"`

#### Consumer

| Alan | Tip | Default | Açıklama |
|------|-----|---------|----------|
| `RespondToContext` | `bool` | `false` | `true` → MassTransit consumer mesajın tüketildiğini context'e bildirir; dead-letter queue yönetimini etkiler |

#### Alert (E-posta Bildirimi)

| Alan | Tip | Default | Kural | Açıklama |
|------|-----|---------|-------|----------|
| `Enabled` | `bool` | `true` | — | `false` → bildirim tamamen devre dışı |
| `TemplateName` | `string` | `"ReconciliationAlertTemplate"` | — | DB'deki şablon adı |
| `ToEmails` | `string[]` | `[]` | — | Alıcı e-posta adresleri |
| `BatchSize` | `int` | `10000000` | > 0 | E-postaya dahil edilecek maksimum satır sayısı |
| `IncludeFailed` | `bool` | `true` | — | Başarısız kayıtları bildirime dahil et |

#### Evaluate (Değerlendirme)

| Alan | Tip | Default | Kural | Açıklama |
|------|-----|---------|-------|----------|
| `ChunkSize` | `int` | `50000` | > 0 | Tek seferde yüklenen kayıt sayısı |
| `ClaimTimeoutSeconds` | `int` | `1800` | > 0 | Claim kilidinin maksimum süresi (saniye). Bu süre dolmadan aynı kayıt başka worker tarafından işlenmez. |
| `ClaimRetryCount` | `int` | `5` | > 0 | Claim alınamazsa yeniden deneme sayısı |
| `OperationMaxRetries` | `int` | `5` | ≥ 0 | Başarısız operasyon için maksimum retry |

> **Kritik:** Dağıtık ortamlarda `ClaimTimeoutSeconds`, beklenen en uzun işlem süresinden büyük olmalıdır; aksi halde aynı kayıt iki kez işlenebilir.

#### Execute (Yürütme)

| Alan | Tip | Default | Kural | Açıklama |
|------|-----|---------|-------|----------|
| `MaxEvaluations` | `int` | `500000` | > 0 | Tek execute tetiklemesinde işlenecek maksimum kayıt sayısı |
| `LeaseSeconds` | `int` | `900` | > 0 | Execute kilitinin süresi (saniye); başka instance'ın aynı işi almasını önler |

---

### 19.4 Archive (`vault_archive.json`)

**C# sınıfı:** `ArchiveOptions` · **Section:** `"Archive"`

#### Genel

| Alan | Tip | Default | Açıklama |
|------|-----|---------|----------|
| `Enabled` | `bool` | `true` | `false` → arşivleme tamamen devre dışı; endpoint'ler 200 döner ama işlem yapmaz |
| `AutoArchiveAfterExecute` | `bool` | `false` | `true` → reconciliation execute bittikten sonra otomatik arşivleme tetiklenir |

#### Defaults (API Override Edilmeyenler)

| Alan | Tip | Default | Kural | Açıklama |
|------|-----|---------|-------|----------|
| `PreviewLimit` | `int` | `5000` | > 0 | `/archive/preview` endpoint maksimum kayıt sayısı |
| `MaxRunCount` | `int` | `50000` | > 0 | Tek archive çalışmasında işlenecek maksimum kayıt |
| `ContinueOnError` | `bool` | `false` | — | `true` → hata olan dosya atlanır, sonrakine devam edilir |
| `UseConfiguredBeforeDateOnly` | `bool` | `false` | — | `true` → API'den gelen `beforeDate` yok sayılır, yalnızca `RetentionDays` kullanılır |
| `DefaultBeforeDateStrategy` | `string` | `"RetentionDays"` | — | `"RetentionDays"` veya `"MinLastUpdateAge"` |
| `MaxRetryPerFile` | `int` | `1` | ≥ 0 | Dosya başına hata retry sayısı |
| `RetryDelaySeconds` | `int` | `2` | ≥ 0 | Retry bekleme süresi (saniye) |

#### Rules (Arşivleme Kuralları)

| Alan | Tip | Default | Kural | Açıklama |
|------|-----|---------|-------|----------|
| `RetentionDays` | `int` | `90` | ≥ 0 | Son güncellemeden bu yana geçmesi gereken minimum gün. `0` = tüm terminal statülü kayıtlar aday |
| `MinLastUpdateAgeHours` | `int` | `72` | ≥ 0 | Son güncellemeden bu yana geçmesi gereken minimum saat. `RetentionDays`'e ek güvenlik katmanı. |
| `RetentionOnlyMode` | `bool` | `false` | — | `true` → soft delete (işaretleme), `false` → fiziksel silme |

> **⚠️ Uyarı:** `RetentionOnlyMode: false` + `RetentionDays: 0` kombinasyonu tüm terminal statülü kayıtları fiziksel olarak siler.

#### TerminalStatuses

Arşivlemeye uygun sayılacak statüler. Yalnızca bu listedeki statüdeki kayıtlar arşivlenir.

```json
"TerminalStatuses": {
  "IngestionFile":                   ["Success", "Failed"],
  "IngestionFileLine":               ["Success", "Failed"],
  "IngestionFileLineReconciliation": ["Success", "Failed"],
  "ReconciliationEvaluation":        ["Completed", "Failed"],
  "ReconciliationOperation":         ["Completed", "Failed", "Cancelled"],
  "ReconciliationOperationExecution":["Completed", "Failed", "Skipped"],
  "ReconciliationReview":            ["Approved", "Rejected", "Cancelled"],
  "ReconciliationAlert":             ["Consumed", "Failed", "Ignored"]
}
```

> **İpucu:** `InProgress` / `Processing` statüsü eklenmesi henüz tamamlanmamış işlemleri siler. Bu listeye yalnızca **gerçekten terminal (nihai)** statüler eklenmelidir.

---

### 19.5 Uygulama Genel Konfigürasyonu (`vault_app_config.json`)

**C# sınıfı:** `AppConfigOptions` · **Section:** `"AppConfig"`

| Alan | Tip | Default | Açıklama |
|------|-----|---------|----------|
| `AuthBypass.Enabled` | `bool` | `false` | `true` → `Controllers` listesindeki controller'lar için JWT doğrulaması atlanır. **Üretimde `false` olmalıdır.** |
| `AuthBypass.Controllers` | `string[]` | `[]` | Bypass uygulanacak controller isimleri |
| `Database.EnableAutoMigrate` | `bool` | `true` | `true` → uygulama başlangıcında bekleyen EF migration'lar otomatik uygulanır |

---

### 19.6 Reporting

Reporting modülü için ayrı bir Vault konfigürasyon dosyası **bulunmamaktadır**. Davranış API sorgu parametreleri ile kontrol edilir.

**Sayfalama Parametreleri** (tüm raporlama endpoint'lerinde geçerli):

| Parametre | Tip | Default | Açıklama |
|-----------|-----|---------|----------|
| `Page` | `int` | `1` | Sayfa numarası (1'den başlar) |
| `Size` | `int` | `20` | Sayfa başı kayıt sayısı |
| `SortBy` | `string` | — | Sıralama alanı adı |
| `SortDesc` | `bool` | `false` | `true` → azalan sıralama |

**Raporlama Kategorileri:**

| Endpoint Grubu | Açıklama |
|---------------|----------|
| `/reporting/ingestion` | Dosya yükleme özet ve detay raporları |
| `/reporting/recon-process` | Mutabakat süreç istatistikleri |
| `/reporting/recon-content` | Mutabakat içerik detayları (eşleşen/eşleşmeyen) |
| `/reporting/recon-advanced` | İleri seviye mutabakat analizleri |
| `/reporting/archive` | Arşivleme geçmişi ve özet |

Raporlama sorguları DB view'larına `DbContext.Set<T>()` ile erişir; view tanımları migration dosyalarında bulunur, ayrı konfigürasyon gerektirmez.

---

### 19.7 Doğrulama Kuralları Özeti

| Bölüm | Alan | Kural | Hata Sınıfı |
|-------|------|-------|-------------|
| FileIngestion | `Connections` | Zorunlu | `FileIngestionConfigConnectionsMissingException` |
| FileIngestion | `Profiles` | Zorunlu | `FileIngestionConfigProfilesMissingException` |
| FileIngestion | `Connections.Source` | Zorunlu | `FileIngestionConfigSourceMissingException` |
| FileIngestion | `Connections.Target` | Zorunlu | `FileIngestionConfigTargetMissingException` |
| FileIngestion | `Connections.*.Protocol` | Zorunlu, boş olamaz | `FileIngestionConfigProtocolMissingException` |
| Processing | `BatchSize` | > 0 | `FileIngestionProcessingBatchSizeInvalidException` |
| Processing | `RetryBatchSize` | > 0 | `FileIngestionProcessingRetryBatchSizeInvalidException` |
| Processing | `MaxDegreeOfParallelism` | > 0 | `FileIngestionProcessingMaxParallelismInvalidException` |
| SFTP | `TimeoutSeconds` | > 0 | `FileIngestionSftpTimeoutInvalidException` |
| SFTP | `OperationTimeoutSeconds` | > 0 | `FileIngestionSftpOperationTimeoutInvalidException` |
| SFTP | `RetryCount` | ≥ 0 | `FileIngestionSftpRetryCountInvalidException` |
| FTP | `TimeoutSeconds` | > 0 | `FileIngestionFtpTimeoutInvalidException` |
| Reconciliation.Evaluate | `ChunkSize` | > 0 | `ReconciliationEvaluateChunkSizeInvalidException` |
| Reconciliation.Evaluate | `ClaimTimeoutSeconds` | > 0 | `ReconciliationEvaluateClaimTimeoutInvalidException` |
| Reconciliation.Evaluate | `ClaimRetryCount` | > 0 | `ReconciliationEvaluateClaimRetryInvalidException` |
| Reconciliation.Evaluate | `OperationMaxRetries` | ≥ 0 | `ReconciliationEvaluateOperationMaxRetriesInvalidException` |
| Reconciliation.Execute | `MaxEvaluations` | > 0 | `ReconciliationExecuteMaxEvaluationsInvalidException` |
| Reconciliation.Execute | `LeaseSeconds` | > 0 | `ReconciliationExecuteLeaseSecondsInvalidException` |
| Reconciliation.Alert | `BatchSize` | > 0 | `ReconciliationAlertBatchSizeInvalidException` |
| Archive.Defaults | `PreviewLimit` | > 0 | `ArchivePreviewLimitInvalidException` |
| Archive.Defaults | `MaxRunCount` | > 0 | `ArchiveMaxRunCountInvalidException` |
| Archive.Defaults | `MaxRetryPerFile` | ≥ 0 | `ArchiveMaxRetryPerFileInvalidException` |
| Archive.Defaults | `RetryDelaySeconds` | ≥ 0 | `ArchiveRetryDelaySecondsInvalidException` |
| Archive.Rules | `RetentionDays` | ≥ 0 | `ArchiveRetentionDaysInvalidException` |
| Archive.Rules | `MinLastUpdateAgeHours` | ≥ 0 | `ArchiveMinUpdateAgeInvalidException` |

> Detaylı açıklamalar ve tam alan haritaları için: `docs/temp/Parametre_Konfigurasyon_Rehberi.md`

---

## 20. Consumer Yapısı, Tetikleyiciler ve Alert/Notification Mekanizması

### 20.1 Genel Mimari — Üç Tetikleme Katmanı

Sistem **üç** farklı tetikleme yoluna sahiptir:

```
┌──────────────────────────────────────────────────────────────────────┐
│                      Tetikleme Katmanları                            │
│                                                                      │
│  1) Scheduler (Otomatik — Hangfire Cron)                             │
│     backend.scheduler → JobSchedulerService                          │
│     → CronJob kaydından IJobTrigger.TriggerAsync()                   │
│     → MassTransit IBus → RabbitMQ SerialQueue                        │
│                                                                      │
│  2) MassTransit Consumer (Asenkron)                                  │
│     RabbitMQ: Card.FileIngestionAndReconciliation.SerialQueue        │
│     → FileIngestionAndReconciliationConsumer                         │
│     → MediatR Command Pipeline                                       │
│                                                                      │
│  3) HTTP API (Senkron, manuel tetikleme)                             │
│     POST /FileIngestion, /Reconciliation/Evaluate, /Execute vb.      │
│     → MediatR Command Pipeline (aynı pipeline)                       │
└──────────────────────────────────────────────────────────────────────┘
```

Tüm yollar sonuçta aynı **MediatR command/query pipeline**'ını çalıştırır.

---

### 20.2 Scheduler Katmanı (backend.scheduler — Hangfire)

#### 20.2.1 Genel Yapı

`backend.scheduler` bağımsız bir .NET servis projesidir. **Hangfire** kullanarak cron job'ları zamanlar; her tetiklemede ilgili `IJobTrigger.TriggerAsync()` çağrılır.

```
Hangfire (Cron tetikleyici)
    │
    ▼
JobSchedulerService.ScheduleAsync()
    │
    ├─ DB'den tüm CronJob kayıtları yüklenir
    ├─ RecordStatus=Active olmayanlar → RecurringJob.RemoveIfExists()
    └─ Active olanlar:
         ├─ CronJobType=QueueMessage → JobFactory() → IJobTrigger → RecurringJob.AddOrUpdate()
         └─ CronJobType=Http → IJobHttpInvoker → RecurringJob.AddOrUpdate()
```

**JobFactory mekanizması:** `JobFactory(cronJob.Name, cronJob.Module)` çağrısı, `Type.GetType("LinkPara.Scheduler.API.Jobs.{Module}.{Name}")` ile ilgili job sınıfını reflection ile bulur ve DI container'dan inject ederek örnek oluşturur.

Tüm Card job'ları için → `LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.<JobAdı>`

**Saat dilimi:** Tüm cron ifadeleri **Turkey Standard Time (UTC+3)** olarak değerlendirilir.

#### 20.2.2 CronJob Veri Modeli (`core.cron_job` tablosu)

| Alan | Tip | Açıklama |
|------|-----|----------|
| `Id` | `uuid` | Birincil anahtar |
| `Name` | `string` | Job sınıf adıyla birebir eşleşmeli (örn. `RemoteCardBkmJob`) |
| `CronExpression` | `string` | Standart cron formatı (`dakika saat gün ay haftaGünü`) |
| `Description` | `string` | İnsan okunabilir açıklama |
| `Module` | `string` | Namespace modül adı (örn. `Card`) |
| `CronJobType` | `enum` | `QueueMessage` → MassTransit kuyruğu; `Http` → HTTP çağrısı |
| `HttpType` | `enum` | `None`, `Get`, `Post` vb. (yalnızca Http tipinde geçerli) |
| `Uri` | `string?` | Http tipinde hedef URL |
| `RecordStatus` | `enum` | `Active` → zamanlanır; diğerleri → kaldırılır |

#### 20.2.3 Card Job Sınıf Hiyerarşisi

```
IJobTrigger
└── FileIngestionAndReconciliationJobBase (abstract)
        ├── FileIngestionAndReconciliationPipelineJobBase (abstract)
        │    │  TriggerAsync() → 3 mesaj sırayla kuyruğa atar:
        │    │    1. IngestFile (ilgili template)
        │    │    2. EvaluateDefault
        │    │    3. ExecuteDefault
        │    │
        │    ├── RemoteCardBkmJob
        │    ├── RemoteCardMscJob
        │    ├── RemoteCardVisaJob
        │    ├── RemoteClearingBkmJob
        │    ├── RemoteClearingMscJob
        │    ├── RemoteClearingVisaJob
        │    ├── LocalCardBkmJob
        │    ├── LocalCardMscJob
        │    ├── LocalCardVisaJob
        │    ├── LocalClearingBkmJob
        │    ├── LocalClearingMscJob
        │    └── LocalClearingVisaJob
        │
        └── FileIngestionAndReconciliationSingleStepJobBase (abstract)
             │  TriggerAsync() → tek mesaj kuyruğa atar
             │
             ├── EvaluateDefaultJob
             └── ExecuteDefaultJob
```

> **Pipeline job'ları** (Remote/Local …) tek bir Hangfire tetiklemesinde **üç mesajı** sırayla kuyruğa ekler: `IngestFile → Evaluate → Execute`. Kuyruğun serial yapısı (`PrefetchCount=1`, `ConcurrentMessageLimit=1`) bu üç adımın sıralı işlenmesini garantiler.

#### 20.2.4 Aktif Zamanlamalar (Üretim)

| Job Adı | Cron | Saat (TR) | Açıklama |
|---------|------|-----------|----------|
| `RemoteCardBkmJob` | `15 1 * * *` | 01:15 | BKM kart dosyası → Ingest + Evaluate + Execute |
| `RemoteCardVisaJob` | `45 15 * * *` | 15:45 | Visa kart dosyası → Ingest + Evaluate + Execute |
| `RemoteCardMscJob` | `45 19 * * *` | 19:45 | MSC kart dosyası → Ingest + Evaluate + Execute |
| `RemoteClearingBkmJob` | `15 16 * * *` | 16:15 | BKM takas dosyası → Ingest + Evaluate + Execute |
| `RemoteClearingVisaJob` | `45 15 * * *` | 15:45 | Visa takas dosyası → Ingest + Evaluate + Execute |
| `RemoteClearingMscJob` | `45 19 * * *` | 19:45 | MSC takas dosyası → Ingest + Evaluate + Execute |
| `EvaluateDefaultJob` | `15 23 * * *` | 23:15 | Gün sonu kontrol Evaluate |
| `ExecuteDefaultJob` | `15 0 * * *` | 00:15 | Gün sonu kontrol Execute |

> **Devre dışı Local Job'lar:** `LocalCard*` ve `LocalClearing*` job kayıtları migration dosyasında `IF 1=2 THEN` bloğu içinde tanımlıdır — yani **hiçbir zaman çalıştırılmaz**. Yalnızca local geliştirme/test için elle aktive edilmelidir.

#### 20.2.5 Mesaj Payload Fabrikası (`FileIngestionAndReconciliationPayloadFactory`)

Her job template için hazır payload üretir:

| Template | Type | FileSourceType | FileType | FileContentType |
|----------|------|---------------|----------|----------------|
| `IngestRemoteCardBkm` | IngestFile | Remote | Card | Bkm |
| `IngestRemoteCardMsc` | IngestFile | Remote | Card | Msc |
| `IngestRemoteCardVisa` | IngestFile | Remote | Card | Visa |
| `IngestRemoteClearingBkm` | IngestFile | Remote | Clearing | Bkm |
| `IngestRemoteClearingMsc` | IngestFile | Remote | Clearing | Msc |
| `IngestRemoteClearingVisa` | IngestFile | Remote | Clearing | Visa |
| `IngestLocalCard*` | IngestFile | Local | Card | Bkm/Msc/Visa |
| `IngestLocalClearing*` | IngestFile | Local | Clearing | Bkm/Msc/Visa |
| `EvaluateDefault` | Evaluate | — | — | — |
| `ExecuteDefault` | Execute | — | — | — |

`EvaluateDefault` için üretilen payload:
```json
{
  "Type": 2,
  "InitiatedByUserId": "<scheduler-app-user-id>",
  "EvaluateRequest": {
    "IngestionFileIds": [],
    "Options": {
      "ChunkSize": 50000,
      "ClaimTimeoutSeconds": 1800,
      "ClaimRetryCount": 5,
      "OperationMaxRetries": 5
    }
  }
}
```

`ExecuteDefault` için:
```json
{
  "Type": 3,
  "InitiatedByUserId": "<scheduler-app-user-id>",
  "ExecuteRequest": {
    "GroupIds": [],
    "EvaluationIds": [],
    "OperationIds": [],
    "Options": {
      "MaxEvaluations": 500000,
      "LeaseSeconds": 900
    }
  }
}
```

**`UseStringEnums`:** `false` (default) → enum değerleri **integer** olarak gönderilir. `true` → string. `FlexibleEnumJsonConverter` her ikisini de kabul eder.

**`InitiatedByUserId`:** Scheduler'ın Application User kimliği (`IApplicationUserService.ApplicationUserId`) otomatik eklenir. Bu sayede tüm audit kayıtları scheduler kullanıcısına atanır.

**Zaman aşımı:** Her job tetiklemesinde `CancellationTokenSource(30 saniye)` kullanılır. 30 saniye içinde kuyruk mesajı gönderilmezse işlem iptal edilir (kuyruğa yazma hatası).

---

### 20.3 MassTransit Consumer — `FileIngestionAndReconciliationConsumer`

**Sınıf:** `LinkPara.Card.Infrastructure.Consumers.FileIngestionAndReconciliation.FileIngestionAndReconciliationConsumer`  
**Arayüz:** `IConsumer<FileIngestionAndReconciliationJobRequest>` (MassTransit)

#### Kuyruk Konfigürasyonu

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Kuyruk adı** | `Card.FileIngestionAndReconciliation.SerialQueue` | RabbitMQ queue adı |
| **PrefetchCount** | `1` | Aynı anda en fazla 1 mesaj alınır |
| **ConcurrentMessageLimit** | `1` | Aynı anda en fazla 1 mesaj işlenir |
| **x-single-active-consumer** | `true` | Birden fazla instance varsa yalnızca biri aktif tüketir |
| **Deserializer** | `UseRawJsonDeserializer` | Ham JSON gövde; standart MassTransit zarf formatı kullanılmaz |

> **Kritik tasarım:** `PrefetchCount=1` + `ConcurrentMessageLimit=1` + `x-single-active-consumer=true` kombinasyonu, Scheduler'ın kuyruğa koyduğu IngestFile → Evaluate → Execute mesajlarının **kesinlikle sıralı ve tekil** işlenmesini garantiler. Birden fazla pod çalışsa bile yalnızca bir pod bu kuyruktan mesaj alır.

#### İş Tipi Yönlendirmesi

```
Type = IngestFile  →  FileIngestionCommand    → FileIngestionOrchestrator
Type = Evaluate    →  EvaluateCommand         → EvaluateService
Type = Execute     →  ExecuteCommand          → ExecuteService
Type = (bilinmeyen) → Hata yanıtı (IsSuccess=false)
```

#### Mesaj Yapısı

```json
{
  "Type": 1,
  "InitiatedByUserId": "user-id",
  "IngestionRequest": {
    "FileSourceType": 1,
    "FileType": 1,
    "FileContentType": 1,
    "FilePath": null
  },
  "EvaluateRequest": null,
  "ExecuteRequest": null
}
```

| Alan | Açıklama |
|------|----------|
| `Type` | İş tipi: `1=IngestFile`, `2=Evaluate`, `3=Execute`. String veya integer kabul edilir. |
| `InitiatedByUserId` | Audit için kullanıcı kimliği. Boşsa header'lardan okunur (`InitiatedByUserId`, `UserId`, `user-id`, `x-user-id`). |
| `IngestionRequest` | `Type=IngestFile` için zorunlu. `FilePath` boşsa Vault konfigürasyonundaki profil yolu kullanılır. |
| `EvaluateRequest` | `Type=Evaluate` için opsiyonel; `null` gelirse defaults uygulanır. `IngestionFileIds=[]` → tüm bekleyen satırlar. |
| `ExecuteRequest` | `Type=Execute` için opsiyonel; `null` gelirse defaults uygulanır. `GroupIds/EvaluationIds/OperationIds=[]` → tüm bekleyen operasyonlar. |

#### Yanıt Yapısı

```json
{
  "Type": 1,
  "IsSuccess": true,
  "ErrorMessage": "",
  "IngestionResponses": [{ "FileId": "guid", ... }],
  "EvaluateResponse": null,
  "ExecuteResponse": null
}
```

**`IsSuccess` başarı koşulları:**

| Tip | Koşul |
|-----|-------|
| `IngestFile` | `responses.Count > 0 && responses.All(x => x.FileId != Guid.Empty)` |
| `Evaluate` | `response.EvaluationRunId != Guid.Empty` |
| `Execute` | `response.TotalAttempted >= 0` |

#### `RespondToContext` Davranışı

`Reconciliation.Consumer.RespondToContext`:
- `false` (default) → fire-and-forget, yanıt gönderilmez.
- `true` → `context.RespondAsync(response)` çağrılır; publisher yanıtı bekleyebilir.

#### Audit Kullanıcısı Yönetimi

Consumer her mesajda mevcut `AuditUserId`'yi saklar → mesajdaki/header'daki userId set eder → `finally` bloğunda eski değeri geri yükler. Tüm DB kayıtları doğru kullanıcıya atanır.

#### Hata Yönetimi

Unhandled exception → MassTransit'e yayılır → `x-single-active-consumer` nedeniyle DLQ'ya (dead-letter queue) taşınır, yeniden kuyruğa alınmaz.

---

### 20.4 HTTP API Tetikleyicileri (Senkron, Manuel)

| Endpoint | Method | Policy | Tetiklenen Komut |
|----------|--------|--------|-----------------|
| `POST /FileIngestion/{fileType}/{sourceType}` | HTTP | `ReadAll` | `FileIngestionCommand` |
| `POST /FileIngestion` (body) | HTTP | `ReadAll` | `FileIngestionCommand` |
| `POST /Reconciliation/Evaluate` | HTTP | `Create` | `EvaluateCommand` |
| `POST /Reconciliation/Operations/Execute` | HTTP | `Create` | `ExecuteCommand` |
| `POST /Reconciliation/Reviews/Approve` | HTTP | `Update` | `ApproveCommand` |
| `POST /Reconciliation/Reviews/Reject` | HTTP | `Update` | `RejectCommand` |
| `GET /Reconciliation/Reviews/Pending` | HTTP | `ReadAll` | `GetPendingReviewsQuery` |
| `GET /Reconciliation/Alerts` | HTTP | `ReadAll` | `GetAlertsQuery` |
| `POST /Archive/Preview` | HTTP | `ReadAll` | `PreviewArchiveQuery` |
| `POST /Archive/Run` | HTTP | `Delete` | `RunArchiveCommand` |

---

### 20.5 Tam Tetikleme Akışı (Scheduler'dan Sonuca)

```
[Hangfire Cron — Turkey Standard Time]
   Örn: 01:15 → RemoteCardBkmJob tetiklenir
           │
           ▼
   FileIngestionAndReconciliationPipelineJobBase.TriggerAsync()
           │
           ├─ [1] SendAsync(IngestRemoteCardBkm)
           │       → MassTransit IBus → queue:Card...SerialQueue
           │
           ├─ [2] SendAsync(EvaluateDefault)
           │       → MassTransit IBus → queue:Card...SerialQueue
           │
           └─ [3] SendAsync(ExecuteDefault)
                   → MassTransit IBus → queue:Card...SerialQueue

[RabbitMQ: Card.FileIngestionAndReconciliation.SerialQueue]
   PrefetchCount=1, x-single-active-consumer=true
           │
           ▼ (mesaj 1)
   FileIngestionAndReconciliationConsumer → IngestFile
           → FileIngestionOrchestrator
               → SFTP'den dosya listele/indir
               → Parse (fixed-width, profil konfigürasyonuna göre)
               → DB'ye bulk insert (IngestionFile, IngestionFileLine)
               → FileId döndür
           │
           ▼ (mesaj 2)
   FileIngestionAndReconciliationConsumer → Evaluate
           → EvaluatorResolver.Resolve(contentType) → BkmEvaluator / VisaEvaluator / MscEvaluator
           → Her satır için ContextBuilder → eşleşme sorgusu
           → Operasyon üret → ReconciliationOperation kayıtları
           → Uyuşmazlık/hata → ReconciliationAlert (Pending) oluştur
           │
           ▼ (mesaj 3)
   FileIngestionAndReconciliationConsumer → Execute
           → OperationExecutor: Her operasyona Lease kilidi al
           → Operasyonu yürüt → ReconciliationOperationExecution kaydı
           → MaxRetries aşıldıysa → ReconciliationAlert (Pending) oluştur
           → AutoArchiveAfterExecute=true ise → ArchiveService tetikle
```

---

### 20.6 Alert Oluşturma Mekanizması

Alert kayıtları (`reconciliation.reconciliation_alert` tablosu) otomatik üretilir:

- **Evaluate** → Uyuşmazlık, hata veya dikkat gerektiren durumda `ReconciliationAlert` → `Pending` statüsüyle oluşturulur.
- **Execute** → Bir operasyon `MaxRetries` sayısını aştığında yeni bir `Alert` → `Pending` oluşturulur.

#### Alert Statü Geçişleri

```
Pending ────────────────────────────────────────────┐
   │                                                 │
   │  AlertService.ExecuteAsync() alır               │
   ▼                                                 │
Processing ── e-posta başarılı ──▶ Consumed          │
   │                                                 │
   └── e-posta hatası ──────────▶ Failed ────────────┘
                                  (sonraki çalışmada tekrar alınır)

Ignored  ← Manuel "yoksay" işlemi
```

| Statü | Açıklama |
|-------|----------|
| `Pending` | Gönderilmeyi bekliyor |
| `Processing` | `AlertService` tarafından kilitlendi, gönderim devam ediyor |
| `Consumed` | E-posta başarıyla gönderildi |
| `Failed` | Gönderim başarısız; `Message` alanına `" | <hata>"` eklenerek biriktirildi |
| `Ignored` | Manuel yoksayıldı |

---

### 20.7 AlertService — Çalışma Mekanizması

**Tetiklenme:** `GET /Reconciliation/Alerts` endpoint'i veya iç servis çağrısıyla.

#### Adım Adım Akış

```
AlertService.ExecuteAsync()
│
├─ 1. Alert.Enabled=false → dur
│
├─ 2. ToEmails boşsa uyarı logla ve dur
│
├─ 3. DB: Pending (+ IncludeFailed=true ise Failed) alertler
│      → OrderBy CreateDate, Take(BatchSize)
│
├─ 4. Toplu yükle: Evaluation, Operation, OperationExecution
│      (N+1 sorgu yok — tek sorgu + gruplama)
│
└─ 5. Her alert için:
       ├─ TryMarkAsProcessingAsync()  ← Optimistic Lock
       │   WHERE Id=? AND Status IN (Pending/Failed)
       │   rows=0 → başka worker aldı → atla
       │
       ├─ BuildTemplateData() → şablon değişkenleri
       │
       ├─ NotificationEmailService.SendEmailAsync()
       │   → IBus → exchange:Notification.SendEmail
       │
       ├─ MarkAsConsumedAsync()  (başarı)
       └─ MarkAsFailedAsync()   (hata — mesaj biriktirilir)
```

**Optimistic Lock:** `TryMarkAsProcessingAsync()` birden fazla AlertService instance'ının aynı alert'i iki kez göndermesini önler:
```sql
UPDATE reconciliation_alert
SET alert_status = 'Processing'
WHERE id = ? AND alert_status IN ('Pending', 'Failed')
-- rows=0 ise başka instance aldı, atla
```

---

### 20.8 NotificationEmailService — E-posta Gönderimi

```
AlertService
    └─ NotificationEmailService.SendEmailAsync(templateName, templateData, toEmail)
           └─ MassTransit IBus → exchange: Notification.SendEmail
                  └─ Notification Mikro Servisi
                         └─ E-posta Provider (SMTP / SendGrid vb.)
```

**`toEmail`:** Birden fazla alıcı `"mail1@x.com;mail2@x.com"` formatında birleştirilir.

#### E-posta Şablonu Değişkenleri

| Değişken | Kaynak | Açıklama |
|----------|--------|----------|
| `emailsubject` | Lokalizasyon | Konu başlığı |
| `alerttype` | `ReconciliationAlert.AlertType` | Uyarı tipi |
| `alertseverity` | `ReconciliationAlert.Severity` | Önem derecesi |
| `raisedat` | Anlık zaman | Oluşturulma zamanı |
| `summary` | Hesaplanan | `AlertType | Severity | EvaluationStatus | OperationCode | Execution` özeti |
| `evaluationstatus` | `ReconciliationEvaluation.Status` | Değerlendirme durumu |
| `evaluationmessage` | `ReconciliationEvaluation.Message` | Mesaj |
| `operationcode` | `ReconciliationOperation.Code` | Operasyon kodu |
| `operationstatus` | `ReconciliationOperation.Status` | Operasyon durumu |
| `lastexecutionstatus` | Son `OperationExecution.Status` | Son yürütme durumu |
| `lastresultcode` | Son execution `ResultCode` | Sonuç kodu |
| `lasterrormessage` | Son execution `ErrorMessage` | Hata mesajı |
| `detailmessage` | Hesaplanan | Alert + Evaluation + Operation + Execution geçmişinin tam metni |

**`detailmessage`** sırasıyla şunları içerir:
1. Alert: Id, AlertType, Severity, GroupId, FileLineId, Message
2. Evaluation: Id, Status, Message, CreatedOperationCount
3. Operation: Code, Status, Note, Branch, LeaseOwner, RetryCount, MaxRetries, NextAttemptAt, LastError
4. Son Execution: AttemptNumber, Status, StartedAt, FinishedAt, ResultCode, ErrorCode, RequestPayload (max 1000 karakter), ResponsePayload (max 1000 karakter)
5. Tüm Execution geçmişi (attempt numarasına göre sıralı)

---

### 20.9 Alert Veri Modeli

```
ReconciliationAlert
├── Id (Guid)
├── FileLineId (Guid)       — hangi ingestion satırı için üretildi
├── GroupId (Guid)          — Evaluate çalışma grubu kimliği
├── EvaluationId (Guid)     — ilgili değerlendirme
├── OperationId (Guid)      — ilgili operasyon (Empty ise operasyon yok)
├── AlertType (string)      — uyarı tipi
├── Severity (string)       — önem derecesi
├── Message (string)        — açıklama / birikmiş hata geçmişi
├── AlertStatus (enum)      — Pending | Processing | Consumed | Failed | Ignored
├── CreateDate
└── UpdateDate / LastModifiedBy (audit)
```

---

### 20.10 EventBus (RabbitMQ) Konfigürasyonu

Her iki proje de (`backend.card` ve `backend.scheduler`) aynı Vault secret'ından RabbitMQ bağlantı bilgisini okur:

```json
// Vault: SharedSecrets / EventBusSettings
{
  "Host": "rabbitmq.internal",
  "Username": "card-service",
  "Password": "secret"
}
```

Yerel geliştirme için `appsettings.Development.json`:
```json
{
  "LocalConfiguration": {
    "IsEnabled": true,
    "EventBusSettings": {
      "Host": "localhost",
      "Username": "guest",
      "Password": "guest"
    }
  }
}
```

#### Hangfire Depolama

Hangfire job durumları aynı Scheduler DB'sinde `core` şemasında tutulur.

| DB | Schema | Açıklama |
|----|--------|----------|
| PostgreSQL | `core` | `PostgreSqlStorageOptions.SchemaName = "core"` |
| MSSQL | `Core` | `SqlServerStorageOptions.SchemaName = "Core"` |

