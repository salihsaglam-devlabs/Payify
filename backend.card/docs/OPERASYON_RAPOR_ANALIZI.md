# Payify Kart Mutabakat Sistemi — Operasyon Rapor ve Süreç Analizi

> **Versiyon:** 1.0  
> **Tarih:** 16 Nisan 2026  
> **Kapsam:** Sistem envanteri, endpoint kataloğu, veri soy ağacı, rapor tasarımları, gap analizi, yol haritası  
> **Kaynak:** backend.card repo, TEKNIK_IS_AKISI_DOKUMANI.md, REPORTING_VIEWS_TECHNICAL_ANALYSIS.md, vault konfigürasyonları, entity modelleri, SQL dosyaları

---

## 1. Executive Summary

Payify Kart Mutabakat Sistemi, BKM/Visa/Mastercard ağlarından gelen kart işlem ve takas (clearing) dosyalarını alarak uçtan uca mutabakat sürecini yöneten bir .NET tabanlı backend servistir. Sistem üç ana süreçten oluşur:

1. **File Ingestion:** Kart ağlarından SFTP/lokal dosya alma, sabit genişlikli parse, duplikat tespiti, clearing eşleştirmesi
2. **Reconciliation:** Dosya satırlarını değerlendirme (evaluate), operasyon planlama, yürütme (execute), manuel onay/red akışı, alert yönetimi
3. **Archive:** Tamamlanmış kayıtların retention policy'ye göre silinmesi

Ek olarak **Paycore entegrasyonu** ile kart oluşturma, müşteri yönetimi, debit otorisasyon ve parametre servisleri sunulmaktadır.

### Mevcut Durum
- 23 adet PostgreSQL reporting view mevcut (raporlama katmanı)
- 10 controller, ~30 endpoint aktif
- 17 veritabanı tablosu (ingestion, reconciliation, archive, core)
- Konfigürasyon Vault üzerinden yönetiliyor

### Kritik Bulgular
- Reporting katmanı yeniden tasarıma ihtiyaç duyuyor (kullanıcı talebi)
- Core banking entegrasyonu hakkında veri modeli repo'da **bulunmuyor** (DebitAuthorization tablosu mevcut ancak GL/muhasebe bağlantısı yok)
- Finansal etki raporu için gerekli tutar reconciliation alanları mevcut ama GL hesap eşleştirmesi eksik
- Operasyon ekibinin kullanabileceği dashboardlar için veri altyapısı büyük ölçüde hazır

---

## 2. Sistem ve Süreç Haritası

### 2.1 Ana Süreç Akışı

```
┌─────────────────────────────────────────────────────────────────┐
│                    DOSYA KAYNAKLARI                              │
│  BKM SFTP ──────┐                                               │
│  Visa SFTP ─────┼──► SFTP/FTP Sunucu (10.222.21.101)           │
│  MSC SFTP ──────┘    Port 22 (SFTP) / Port 21 (FTP)            │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  ADIM 1: FILE INGESTION                                         │
│  POST /v1/FileIngestion                                         │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌───────────────────┐  │
│  │Dosya     │→│Parse     │→│Duplikat  │→│Clearing           │  │
│  │Keşif     │ │(Fixed-   │ │Tespiti   │ │Eşleştirme         │  │
│  │& Download│ │Width)    │ │          │ │(OceanTxnGuid)     │  │
│  └──────────┘ └──────────┘ └──────────┘ └───────────────────┘  │
│  Yazılan tablolar: ingestion.file, file_line,                    │
│  card_bkm/visa/msc_detail, clearing_bkm/visa/msc_detail        │
└────────────────────────────┬────────────────────────────────────┘
                             │ ReconciliationStatus = Ready
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  ADIM 2: EVALUATE                                               │
│  POST /v1/Reconciliation/Evaluate                               │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌───────────────────┐  │
│  │Claim     │→│Context   │→│Evaluator │→│Operasyon          │  │
│  │(Serialize│ │Build     │ │Seçimi    │ │Planlama           │  │
│  │ Txn)     │ │          │ │(BKM/VISA/│ │(Review varsa)     │  │
│  └──────────┘ └──────────┘ │MSC)      │ └───────────────────┘  │
│                             └──────────┘                         │
│  Yazılan tablolar: reconciliation.evaluation, operation, review  │
└────────────────────────────┬────────────────────────────────────┘
                             │ Operasyonlar Planned/Blocked
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  ADIM 3: EXECUTE                                                │
│  POST /v1/Reconciliation/Operations/Execute                     │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌───────────────────┐  │
│  │Operasyon │→│Lease     │→│Yürütme   │→│Retry /            │  │
│  │Seçimi    │ │Claim     │ │          │ │Alert              │  │
│  └──────────┘ └──────────┘ └──────────┘ └───────────────────┘  │
│  Yazılan tablolar: reconciliation.operation_execution, alert     │
│  Manuel gate: Reviews/Approve veya Reviews/Reject                │
└────────────────────────────┬────────────────────────────────────┘
                             │ AutoArchiveAfterExecute = true
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  ADIM 4: ARCHIVE                                                │
│  POST /v1/Archive/Run                                           │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐                        │
│  │Uygunluk  │→│Silme     │→│Audit Log │                        │
│  │Kontrolü  │ │(Cascade) │ │          │                        │
│  └──────────┘ └──────────┘ └──────────┘                        │
│  Yazılan tablolar: archive.archive_log, archive.ingestion_file   │
│  Retention: 90 gün, MinAge: 72 saat                              │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 Sistem Bileşenleri Envanteri

| Bileşen Adı | Tip | Kaynak Dosya | İşlev | Girdi | Çıktı | Bağlı Süreç | Operasyonel Önemi | İzlenmesi Gereken KPI |
|---|---|---|---|---|---|---|---|---|
| **FileIngestionController** | API | Controllers/FileIngestionController.cs | Kart/clearing dosyalarını sisteme alır | FileSourceType, FileType, FileContentType, FilePath | FileIngestionResponse (FileId, Status, Counts) | Ingestion | Tüm sürecin başlangıç noktası; dosya alınmazsa mutabakat başlamaz | Günlük dosya sayısı, success_rate, error_count, beklenen vs gelen dosya |
| **ReconciliationController** | API | Controllers/ReconciliationController.cs | Mutabakat değerlendirme, yürütme, onay/red, alert | EvaluateRequest, ExecuteRequest, ApproveRequest | EvaluateResponse, ExecuteResponse | Reconciliation | Finansal mutabakatın çekirdeği | CreatedOperationsCount, TotalFailed, pending review sayısı |
| **ArchiveController** | API | Controllers/ArchiveController.cs | Eski kayıtları temizler | IngestionFileIds, BeforeDate | ArchiveRunResponse (ArchivedCount) | Archive | Veritabanı performansı ve saklama | ArchivedCount, FailedCount, days_to_archive |
| **ReportingController** | API | Controllers/ReportingController.cs | Mutabakat raporlarını sunar | Filtreler (tarih, network, mismatch) | İşlem listeleri, özet KPI'lar | Reporting | Operasyon görünürlüğü | clean_count/total_count oranı, problem_count trendi |
| **PaycoreCardController** | API | Controllers/PaycoreCardController.cs | Kart CRUD, durum güncelleme, yetkilendirme | CardNo, CustomerNo, ProductCode | Kart bilgileri, işlem listesi | Core Banking | Kart yaşam döngüsü yönetimi | Aktif kart sayısı, durum değişiklik sayısı |
| **PaycoreCustomerController** | API | Controllers/PaycoreCustomerController.cs | Müşteri CRUD, limit yönetimi, iletişim güncelleme | BankingCustomerNo | Müşteri bilgileri, kart listesi | Core Banking / Onboarding | Müşteri onboarding ve güncelleme | Yeni müşteri sayısı, limit değişiklik sayısı |
| **PaycoreDebitAuthorizationController** | API | Controllers/PaycoreDebitAuthorizationController.cs | Kart işlem otorisasyonu (debit) | İşlem detayları (tutar, kart, üye iş yeri) | Otorisasyon yanıtı | Core Banking | Gerçek zamanlı işlem onayı | İşlem adedi, onay/red oranı, ortalama yanıt süresi |
| **PaycoreCardPinController** | API | Controllers/PaycoreCardPinController.cs | Kart PIN belirleme | Şifreli PIN verisi | İşlem sonucu | Core Banking | Kart aktivasyonu | PIN set başarı oranı |
| **PaycoreParametersController** | API | Controllers/PaycoreParametersController.cs | Ürün parametreleri sorgulama | - | Ürün listesi | Core Banking | Parametre yönetimi | - |
| **CustomerWalletCardController** | API | Controllers/CustomerWalletCardController.cs | Müşteri cüzdan kartları yönetimi | BankingCustomerNo, WalletNumber | Kart listesi | Wallet / Onboarding | Cüzdan-kart ilişkisi | Aktif cüzdan-kart sayısı |
| **ingestion.file** | DB Table | Domain/Entities/FileIngestion/Persistence/IngestionFile.cs | Yüklenen dosya metadata | Dosya bilgileri | - | Ingestion | Dosya takibi | Dosya başarı oranı |
| **ingestion.file_line** | DB Table | Domain/Entities/FileIngestion/Persistence/IngestionFileLine.cs | Dosya satır kayıtları | Parse edilmiş satır | - | Ingestion/Reconciliation | Satır bazlı izleme | ReconciliationStatus dağılımı |
| **ingestion.card_bkm_detail** | DB Table | Domain/Entities/FileIngestion/Persistence/IngestionCardBkmDetail.cs | BKM kart işlem detayı | Parse edilmiş BKM alanları | - | Ingestion | BKM kart verisi | - |
| **ingestion.card_visa_detail** | DB Table | Domain/Entities/FileIngestion/Persistence/IngestionCardVisaDetail.cs | Visa kart işlem detayı | Parse edilmiş Visa alanları | - | Ingestion | Visa kart verisi | - |
| **ingestion.card_msc_detail** | DB Table | Domain/Entities/FileIngestion/Persistence/IngestionCardMscDetail.cs | MSC kart işlem detayı | Parse edilmiş MSC alanları | - | Ingestion | MSC kart verisi | - |
| **ingestion.clearing_bkm_detail** | DB Table | Domain/Entities/FileIngestion/Persistence/IngestionClearingBkmDetail.cs | BKM clearing detayı | Parse edilmiş BKM clearing | - | Ingestion/Clearing | BKM takas verisi | - |
| **ingestion.clearing_visa_detail** | DB Table | Domain/Entities/FileIngestion/Persistence/IngestionClearingVisaDetail.cs | Visa clearing detayı | Parse edilmiş Visa clearing | - | Ingestion/Clearing | Visa takas verisi | - |
| **ingestion.clearing_msc_detail** | DB Table | Domain/Entities/FileIngestion/Persistence/IngestionClearingMscDetail.cs | MSC clearing detayı | Parse edilmiş MSC clearing | - | Ingestion/Clearing | MSC takas verisi | - |
| **reconciliation.evaluation** | DB Table | Domain/Entities/Reconciliation/Persistence/ReconciliationEvaluation.cs | Mutabakat değerlendirme kaydı | FileLineId, GroupId | Status, OperationCount | Reconciliation | Değerlendirme takibi | Evaluation başarı oranı |
| **reconciliation.operation** | DB Table | Domain/Entities/Reconciliation/Persistence/ReconciliationOperation.cs | Planlanan mutabakat operasyonu | EvaluationId, Code, Payload | Status, RetryCount | Reconciliation | Operasyon yaşam döngüsü | Failed operasyon sayısı, ortalama retry |
| **reconciliation.operation_execution** | DB Table | Domain/Entities/Reconciliation/Persistence/ReconciliationOperationExecution.cs | Operasyon yürütme denemesi | OperationId, AttemptNumber | Status, ErrorCode | Reconciliation | Execution audit trail | Başarısız execution sayısı |
| **reconciliation.review** | DB Table | Domain/Entities/Reconciliation/Persistence/ReconciliationReview.cs | Manuel onay/red kaydı | OperationId, ReviewerId | Decision, Comment | Reconciliation | Manuel karar izleme | Pending review sayısı, ortalama karar süresi |
| **reconciliation.alert** | DB Table | Domain/Entities/Reconciliation/Persistence/ReconciliationAlert.cs | Mutabakat uyarısı | Severity, AlertType, Message | AlertStatus | Reconciliation | Alarm yönetimi | Açık alert sayısı, severity dağılımı |
| **archive.archive_log** | DB Table | Domain/Entities/Archive/ArchiveLog.cs | Arşiv işlem logu | IngestionFileId | İşlem sonucu | Archive | Arşiv denetim izi | Arşiv başarı oranı |
| **archive.ingestion_file** | DB Table | Domain/Entities/Archive/ArchiveIngestionFile.cs | Arşivlenmiş dosya snapshot | Dosya bilgileri kopyası | - | Archive | Regülasyon saklama | Saklanan kayıt sayısı |
| **DebitAuthorization** | DB Table | Domain/Entities/DebitAuthorization.cs | Otorisasyon işlem kaydı | İşlem detayları | - | Core Banking | Otorisasyon audit | İşlem adedi, tutar toplamları |
| **DebitAuthorizationFee** | DB Table | Domain/Entities/DebitAuthorizationFee.cs | Otorisasyon komisyon kaydı | OceanTxnGUID, Type, Amount | - | Core Banking | Komisyon takibi | Komisyon toplamları |
| **CustomerWalletCard** | DB Table | Domain/Entities/CustomerWalletCard.cs | Müşteri-cüzdan-kart ilişkisi | BankingCustomerNo, CardNumber | - | Wallet | Kart envanteri | Aktif kart sayısı |
| **SFTP Dosya Çekme** | Batch/File | vault_fileingestion.json → Connections.Source.Sftp | SFTP üzerinden dosya indirme | SFTP bağlantı bilgileri | Dosya | Ingestion | Dosya kaynağı erişimi | Bağlantı başarı oranı, dosya boyutları |
| **Dosya Arşivleme (Target)** | File Output | vault_fileingestion.json → Connections.Target | İşlenen dosyanın hedef depoya kopyalanması | Dosya | Arşiv kopyası | Ingestion | Dosya bütünlüğü | IsArchived oranı |
| **E-posta Alert** | Notification | reconciliation_notification_template_postgre.sql | Mutabakat alert e-posta bildirimi | Alert bilgileri | E-posta | Reconciliation | Operasyon bilgilendirme | Gönderilen/başarısız bildirim sayısı |
| **23 Reporting View** | DB View | docs/REPORTING_VIEWS_TECHNICAL_ANALYSIS.md | Raporlama katmanı | Operasyonel tablolar | Normalize edilmiş rapor verileri | Reporting | Operasyon görünürlüğü | View yanıt süreleri |

---

## 3. Endpoint Kataloğu

### 3.1 File Ingestion Endpoints

#### POST /v1/FileIngestion
| Özellik | Detay |
|---------|-------|
| **Method** | POST |
| **Path** | /v1/FileIngestion |
| **Request Alanları** | `FileSourceType` (Remote/Local), `FileType` (Card/Clearing), `FileContentType` (Bkm/Msc/Visa), `FilePath` (opsiyonel) |
| **Response Alanları** | `FileId`, `FileKey`, `FileName`, `Status`, `StatusName`, `Message`, `TotalCount`, `SuccessCount`, `ErrorCount`, `Errors[]` |
| **İş Kuralı Özeti** | Dosyayı SFTP/lokal'den alır, sabit genişlikli parse eder, duplikat tespit eder, clearing eşleştirmesi yapar, arşivler. FileKey (header+footer hash) ile dosya benzersizliği sağlanır. |
| **Çağırdığı Servisler** | FileIngestionOrchestrator, IFileTransferClient (SFTP/Local), BulkInsert, DuplicateDetection, ClearingMatcher |
| **Yazdığı Tablolar** | ingestion.file, ingestion.file_line, ingestion.card_*_detail, ingestion.clearing_*_detail |
| **Okuduğu Tablolar** | ingestion.file (duplikat kontrolü), ingestion.file_line (clearing eşleştirme) |
| **Operasyon Etkisi** | Tüm mutabakat sürecinin başlangıcı. Dosya alınmazsa hiçbir şey çalışmaz. |
| **Kullanılabileceği Raporlar** | Dosya alım özeti, parse başarı raporu, beklenen vs gelen dosya raporu, SLA raporu |

#### POST /v1/FileIngestion/{fileSourceType}/{fileType}/{fileContentType}
| Özellik | Detay |
|---------|-------|
| **Method** | POST |
| **Path** | /v1/FileIngestion/{fileSourceType}/{fileType}/{fileContentType} |
| **Request Alanları** | Route parametreleri + `FilePath` (body) |
| **Response/Davranış** | Yukarıdakiyle aynı, route parametreleri body yerine URL'den alınır |
| **Operasyon Etkisi** | Cron job / scheduler tarafından spesifik dosya tipi çekmek için |

---

### 3.2 Reconciliation Endpoints

#### POST /v1/Reconciliation/Evaluate
| Özellik | Detay |
|---------|-------|
| **Method** | POST |
| **Path** | /v1/Reconciliation/Evaluate |
| **Request Alanları** | `IngestionFileIds[]` (opsiyonel), `Options.ChunkSize` (50.000), `Options.ClaimTimeoutSeconds` (1.800), `Options.ClaimRetryCount` (5), `Options.OperationMaxRetries` (5) |
| **Response Alanları** | `EvaluationRunId`, `CreatedOperationsCount`, `SkippedCount`, `Message`, `ErrorCount`, `Errors[]` |
| **İş Kuralı Özeti** | Ready satırları claim eder, kart-clearing karşılaştırması yapar, operasyonlar planlar. Boş request tüm bekleyen satırları işler. SERIALIZABLE isolation. |
| **Yazdığı Tablolar** | reconciliation.evaluation, reconciliation.operation, reconciliation.review, reconciliation.alert |
| **Okuduğu Tablolar** | ingestion.file_line, ingestion.card_*_detail, ingestion.clearing_*_detail |
| **Operasyon Etkisi** | Mutabakat planının oluşturulması. Yüksek CreatedOperationsCount = çok uyumsuzluk. |
| **Kullanılabileceği Raporlar** | Evaluation performans raporu, uyumsuzluk kırılım raporu |

#### POST /v1/Reconciliation/Operations/Execute
| Özellik | Detay |
|---------|-------|
| **Method** | POST |
| **Path** | /v1/Reconciliation/Operations/Execute |
| **Request Alanları** | `GroupIds[]`, `EvaluationIds[]`, `OperationIds[]` (tümü opsiyonel — boş = hepsini çalıştır), `Options.MaxEvaluations` (500.000), `Options.LeaseSeconds` (900) |
| **Response Alanları** | `TotalAttempted`, `TotalSucceeded`, `TotalFailed`, `Results[]` |
| **İş Kuralı Özeti** | Planlanan operasyonları sırayla yürütür. Manuel gate bekleyenler bloke olur. Retry: exponential backoff (30*2^n sn). AutoArchive tetiklenir. |
| **Yazdığı Tablolar** | reconciliation.operation (status güncelleme), reconciliation.operation_execution, reconciliation.alert |
| **Operasyon Etkisi** | Mutabakat aksiyonlarının fiili uygulanması |
| **Kullanılabileceği Raporlar** | Execution performans raporu, retry analizi, başarısız operasyon raporu |

#### POST /v1/Reconciliation/Reviews/Approve
| Özellik | Detay |
|---------|-------|
| **Method** | POST |
| **Path** | /v1/Reconciliation/Reviews/Approve |
| **Request Alanları** | `OperationId` (zorunlu), `ReviewerId` (opsiyonel), `Comment` (opsiyonel) |
| **Response Alanları** | `OperationId`, `Result` (Approved/NotFound/Invalid/Failed), `Message` |
| **İş Kuralı Özeti** | Pending review'ı onaylar. Approve dalı aktifleşir, reject dalı iptal. NextAttemptAt = now. |
| **Yazdığı Tablolar** | reconciliation.review (Decision), reconciliation.operation (NextAttemptAt) |
| **Operasyon Etkisi** | İnsan kararıyla sürecin devam etmesi |
| **Kullanılabileceği Raporlar** | Manuel karar raporu, karar süresi raporu |

#### POST /v1/Reconciliation/Reviews/Reject
| Özellik | Detay |
|---------|-------|
| **Method/Path/Request** | Approve ile aynı yapı |
| **İş Kuralı Farkı** | Decision = Rejected. Reject dalı aktifleşir, approve dalı iptal. |

#### GET /v1/Reconciliation/Reviews/Pending
| Özellik | Detay |
|---------|-------|
| **Method** | GET |
| **Path** | /v1/Reconciliation/Reviews/Pending |
| **Request Alanları** | `Date` (opsiyonel), `Page`, `Size`, `SortBy`, `OrderBy` |
| **Response Alanları** | PaginatedList: `OperationId`, `FileLineId`, `OperationCode`, `OperationPayload`, `CreatedAt`, `ApproveBranchOperations`, `RejectBranchOperations`, `ExpiresAt`, `ExpirationAction`, `ExpirationFlowAction` |
| **İş Kuralı Özeti** | Operatörün iş kuyruğu. ExpiresAt yakın olanlar öncelikli. |
| **Okuduğu Tablolar** | reconciliation.operation, reconciliation.review |
| **Operasyon Etkisi** | Bekleyen kararların görünürlüğü |
| **Kullanılabileceği Raporlar** | Bekleyen iş raporu, SLA takip raporu |

#### GET /v1/Reconciliation/Alerts
| Özellik | Detay |
|---------|-------|
| **Method** | GET |
| **Path** | /v1/Reconciliation/Alerts |
| **Request Alanları** | `Date` (opsiyonel), `AlertStatus` (Pending/Processing/Consumed/Failed/Ignored) |
| **Response Alanları** | Alert listesi: `Severity`, `AlertType`, `Message`, `OperationId`, `EvaluationId`, `AlertStatus` |
| **İş Kuralı Özeti** | EvaluationFailed (OperationId boş) veya OperationExecutionFailed (OperationId dolu) uyarıları |
| **Okuduğu Tablolar** | reconciliation.alert, reconciliation.operation |
| **Operasyon Etkisi** | Sorun tespiti ve eskalasyon |
| **Kullanılabileceği Raporlar** | Alert dashboard, severity analizi, trend raporu |

---

### 3.3 Archive Endpoints

#### POST /v1/Archive/Preview
| Özellik | Detay |
|---------|-------|
| **Method** | POST |
| **Path** | /v1/Archive/Preview |
| **Request Alanları** | `IngestionFileIds[]`, `BeforeDate`, `Limit` (max 1000) |
| **Response Alanları** | Candidates[]: `IngestionFileId`, `IsEligible`, `FailureReasons[]`, `Counts` (satır, detay, evaluation, operation, review, execution, alert sayıları) |
| **İş Kuralı Özeti** | Veri değiştirmez. Retention: 90 gün, MinAge: 72 saat. Tüm statüler terminal olmalı. |
| **Kullanılabileceği Raporlar** | Arşiv aday raporu, retention analizi |

#### POST /v1/Archive/Run
| Özellik | Detay |
|---------|-------|
| **Method** | POST |
| **Path** | /v1/Archive/Run |
| **Request Alanları** | `IngestionFileIds[]`, `BeforeDate`, `MaxFiles` (50.000), `ContinueOnError` |
| **Response Alanları** | `ProcessedCount`, `ArchivedCount`, `SkippedCount`, `FailedCount`, `Items[]` |
| **İş Kuralı Özeti** | Uygun dosyaları ve tüm ilişkili kayıtları kalıcı siler. Geri dönüşü yok. |
| **Yazdığı Tablolar** | archive.archive_log, archive.ingestion_file (snapshot). Siler: ingestion.*, reconciliation.* |
| **Kullanılabileceği Raporlar** | Arşiv raporu, saklama süresi raporu |

---

### 3.4 Reporting Endpoints (Mevcut — Yeniden Tasarlanacak)

| Endpoint | Method | Path | Amacı |
|----------|--------|------|-------|
| Transactions | GET | /v1/Reporting/Reconciliation/Transactions | Kart-clearing eşleştirilmiş işlemler (filtreli, sayfalı) |
| Problems | GET | /v1/Reporting/Reconciliation/Problems | Sorunlu kayıtlar (herhangi bir mismatch) |
| Unmatched | GET | /v1/Reporting/Reconciliation/Unmatched | Eşleşmemiş kart kayıtları |
| Summary/Daily | GET | /v1/Reporting/Reconciliation/Summary/Daily | Günlük KPI özeti |
| Summary/Network | GET | /v1/Reporting/Reconciliation/Summary/Network | Ağ bazlı KPI |
| Summary/File | GET | /v1/Reporting/Reconciliation/Summary/File | Dosya bazlı KPI |
| Summary | GET | /v1/Reporting/Reconciliation/Summary | Genel tek satır özet |

---

### 3.5 Paycore / Wallet Endpoints

| Endpoint | Method | Path | Amacı |
|----------|--------|------|-------|
| Create Card | POST | /v1/PaycoreCard | Yeni kart oluşturma |
| Update Card Status | PUT | /v1/PaycoreCard/status | Kart durumu güncelleme |
| Get Card Info | GET | /v1/PaycoreCard/card-info | Kart bilgisi sorgulama |
| Get Card Authorizations | GET | /v1/PaycoreCard/card-authorizations | Kart yetkilendirme bilgileri |
| Update Card Authorization | PUT | /v1/PaycoreCard/card-authorization | Yetkilendirme güncelleme |
| Get Card Transactions | GET | /v1/PaycoreCard/transactions | Kart işlemleri |
| Get Card Sensitive Data | GET | /v1/PaycoreCard/card-sensitive-data | Hassas kart verisi |
| Card Renewal | POST | /v1/PaycoreCard/card-renewal | Kart yenileme |
| Get Card Status | GET | /v1/PaycoreCard/card-status | Kart durumu |
| Get Last Courier Activity | GET | /v1/PaycoreCard/card-last-courier-activity | Son kurye aktivitesi |
| Add Limit Restriction | PUT | /v1/PaycoreCard/additional-limit-restriction | Ek limit kısıtlaması |
| Create Customer | POST | /v1/PaycoreCustomer | Müşteri oluşturma |
| Get Customer Info | GET | /v1/PaycoreCustomer | Müşteri bilgisi |
| Get Customer Cards | GET | /v1/PaycoreCustomer/cards | Müşteri kartları |
| Get Customer Limit Info | GET | /v1/PaycoreCustomer/limit-info | Müşteri limit bilgisi |
| Update Customer | PUT | /v1/PaycoreCustomer/customer | Müşteri güncelleme |
| Update Customer Communication | PUT | /v1/PaycoreCustomer/communication | İletişim güncelleme |
| Update Customer Address | PUT | /v1/PaycoreCustomer/address | Adres güncelleme |
| Update Customer Limit | PUT | /v1/PaycoreCustomer/limit | Limit güncelleme |
| Debit Authorization | POST | /v1/PaycoreDebitAuthorization/debit-auth | İşlem otorisasyonu |
| Set Card PIN | PUT | /v1/PaycoreCardPin/set-card-pin | PIN belirleme |
| Get Products | GET | /v1/PaycoreParameters | Ürün parametreleri |
| Get Wallet Cards | GET | /v1/CustomerWalletCard/customer-cards | Cüzdan kartları |
| Update Wallet Card Name | PUT | /v1/CustomerWalletCard/customer-cards/name | Kart adı güncelleme |

---

## 4. Veri Modeli ve Tablo Envanteri

### 4.1 Veri Soy Ağacı (Data Lineage)

```
DOSYA GİRİŞ KAYNAĞI
├── BKM: /turkonay/PROD/PTS2TURKONAY/PAYCORE_REPORTS (Card)
│         /turkonay/PROD/PTS2TURKONAY/BKM_REPORTS/OUTGOING (Clearing)
├── VISA: /turkonay/PROD/PTS2TURKONAY/PAYCORE_REPORTS (Card)
│         /turkonay/PROD/PTS2TURKONAY/VISA_REPORTS/OUTGOING (Clearing)
└── MSC:  /turkonay/PROD/PTS2TURKONAY/PAYCORE_REPORTS (Card)
          /turkonay/PROD/PTS2TURKONAY/MASTERCARD_REPORTS/OUTGOING (Clearing)
   │
   │ Dosya pattern: CARD_TRANSACTIONS_YYYYMMDD_N (Card)
   │                 BKMACC/MSCACC/VISAACC + tarih (Clearing)
   │ Format: Sabit genişlikli (706 byte Card, 321-347 byte Clearing)
   ▼
PARSING
├── Header (H): FileDate, FileNo, FileVersionNumber
├── Detail (D): Tüm işlem alanları (60+ alan/ağ)
└── Footer (F): FileDate, TxnCount (beklenen satır sayısı)
   │
   │ Korelasyon: OceanTxnGuid (öncelik 1) veya Rrn:CardNo:ProvisionCode:Arn:Mcc:Amount:Currency (fallback)
   │ FileKey: Header+Footer hash
   ▼
VALIDATION & STAGING
├── ingestion.file (FileKey, Status, ExpectedCount, TotalCount, SuccessCount, ErrorCount)
├── ingestion.file_line (LineNumber, RecordType, RawData, ParsedData, Status, ReconciliationStatus, CorrelationKey, DuplicateStatus, MatchedClearingLineId)
├── ingestion.card_bkm_detail / card_visa_detail / card_msc_detail
└── ingestion.clearing_bkm_detail / clearing_visa_detail / clearing_msc_detail
   │
   │ Duplikat tespiti: Card → OceanTxnGuid, Clearing → ClrNo:ControlStat
   │ Clearing eşleştirme: OceanTxnGuid üzerinden matched_clearing_line_id
   │ Statüler: FileStatus (Processing/Failed/Success), FileRowStatus (Processing/Failed/Success)
   │ ReconciliationStatus: Ready → Processing → Success/Failed
   ▼
BUSINESS PROCESSING (Evaluate)
├── reconciliation.evaluation (FileLineId, GroupId, Status, CreatedOperationCount)
│   Statüler: Pending → Evaluating → Planned → Completed/Failed
├── reconciliation.operation (FileLineId, EvaluationId, Code, SequenceIndex, Status, RetryCount, IdempotencyKey)
│   Statüler: Planned → Blocked → Executing → Completed/Failed/Cancelled
├── reconciliation.review (OperationId, ReviewerId, Decision, ExpiresAt, ExpirationAction)
│   Statüler: Pending → Approved/Rejected/Cancelled
└── reconciliation.operation_execution (OperationId, AttemptNumber, Status, RequestPayload, ResponsePayload, ErrorCode)
    Statüler: Started → Completed/Failed/Skipped
   │
   │ Alert oluşturma: reconciliation.alert (Severity, AlertType, AlertStatus)
   │ AlertStatus: Pending → Processing → Consumed/Failed/Ignored
   │ E-posta bildirim: ReconciliationAlertTemplate
   ▼
RECONCILIATION (Execute)
├── Otomatik operasyonlar: Dış sistem entegrasyonu, retry (exponential backoff)
├── Manuel operasyonlar: Review → Approve/Reject → Branch operasyonları
└── İdempotency: {Code}:{FileLineId}:{SequenceIndex}:{CorrelationValue}:...
   │
   │ AutoArchiveAfterExecute = true
   ▼
ARCHIVE
├── archive.ingestion_file (dosya snapshot)
├── archive.ingestion_file_line (satır snapshot — belirsiz, Archive entity'lerden çıkarım)
├── archive.card_*_detail, archive.clearing_*_detail (detay snapshotları)
├── archive.reconciliation_* (evaluation, operation, execution, review, alert snapshotları)
└── archive.archive_log (işlem logu: IngestionFileId, Status, CreateDate)
    │
    │ Retention: 90 gün, MinLastUpdateAgeHours: 72
    │ Terminal statüler kontrol edilir
    │ Silme: CASCADE — dosya ve tüm ilişkili kayıtlar
    ▼
REPORTING (23 SQL View)
├── Base Layer: vw_base_card_transaction, vw_base_clearing_transaction (BKM+VISA+MSC UNION ALL)
├── Matching Layer: vw_reconciliation_matched_pair (LEFT JOIN + mismatch flags)
├── Business Layer: vw_unmatched_card, vw_amount_mismatch, vw_status_mismatch, vw_clean_matched, vw_problem_records
├── Summary Layer: vw_summary_daily, vw_summary_by_network, vw_summary_by_file, vw_summary_overall
└── Operational Layer: vw_file_ingestion_summary, vw_reconciliation_file_summary, vw_line_detail,
    vw_unmatched_cards, vw_operation_tracker, vw_pending_actions, vw_alert_dashboard,
    vw_daily_reconciliation_summary, vw_reconciliation_aging, vw_archive_audit_trail, vw_clearing_dispute_monitor
```

### 4.2 Hata Kodları ve Statüler

| Kategori | Statü | Değer | Anlamı |
|----------|-------|-------|--------|
| **FileStatus** | Processing | 1 | Dosya işleniyor |
| | Failed | 2 | İşlem başarısız |
| | Success | 3 | Başarılı |
| **FileRowStatus** | Processing | 1 | Satır işleniyor |
| | Failed | 2 | Parse hatası |
| | Success | 3 | Başarılı parse |
| **ReconciliationStatus** | Ready | 1 | Evaluate için hazır |
| | Failed | 2 | Korelasyon/evaluate hatası |
| | Success | 3 | Evaluate tamamlandı |
| | Processing | 4 | Evaluate claim edildi |
| **EvaluationStatus** | Pending | 0 | Bekliyor |
| | Evaluating | 1 | İşleniyor |
| | Planned | 2 | Operasyonlar planlandı |
| | Failed | 3 | Başarısız |
| | Completed | 4 | Tamamlandı |
| **OperationStatus** | Planned | 0 | Planlandı, çalıştırılmaya hazır |
| | Blocked | 1 | Önceki adım bekleniyor |
| | Executing | 2 | Yürütülüyor |
| | Completed | 3 | Başarıyla tamamlandı |
| | Failed | 4 | Retry tükendi, kalıcı hata |
| | Cancelled | 5 | İptal edildi |
| **ExecutionStatus** | Started | 0 | Başladı |
| | Completed | 1 | Tamamlandı |
| | Failed | 2 | Başarısız |
| | Skipped | 3 | İdempotent atlandı |
| **ReviewDecision** | Pending | 0 | Karar bekleniyor |
| | Approved | 1 | Onaylandı |
| | Rejected | 2 | Reddedildi |
| | Cancelled | 3 | Süre doldu/iptal |
| **AlertStatus** | Pending | 0 | Henüz işlenmedi |
| | Processing | 1 | İşleniyor |
| | Consumed | 2 | Bildirim gönderildi |
| | Failed | 3 | Gönderim hatası |
| | Ignored | 4 | Yok sayıldı |
| **DuplicateStatus** | Unique | - | Tekil kayıt |
| | Primary | - | Duplikat grubunun ana kaydı |
| | Secondary | - | Duplikat kopya |
| | Conflict | - | Aynı key, farklı içerik |
| **FileSourceType** | Remote | 1 | SFTP/FTP |
| | Local | 2 | Yerel dosya sistemi |
| **FileType** | Card | 1 | Kart işlem dosyası |
| | Clearing | 2 | Takas dosyası |
| **FileContentType** | Bkm | 1 | BKM formatı |
| | Msc | 2 | Mastercard formatı |
| | Visa | 3 | Visa formatı |

---

## 5. Süreç Bazlı Rapor Önerileri

### A. File Ingestion Raporları

#### A1. Günlük Dosya Alım Özeti
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Günlük Dosya Alım Özeti |
| 2 | **Amaç** | Her gün alınan dosyaların durumunu tek bakışta görmek |
| 3 | **İş Kullanıcıları** | Operasyon uzmanı, takım lideri |
| 4 | **Karar Destek Değeri** | Dosya akışının sağlıklı olup olmadığını anında gösterir |
| 5 | **Veri Kaynakları** | `ingestion.file` (veya `vw_file_ingestion_summary`) |
| 6 | **Hesaplama Mantığı** | Dosya sayısı, satır sayısı toplamları; success_rate = success/processed*100 |
| 7 | **Filtreler** | Tarih aralığı, SourceType, FileType, ContentType |
| 8 | **Boyutlar** | Tarih, Ağ (BKM/Visa/MSC), Dosya tipi (Card/Clearing) |
| 9 | **Metrikler** | Toplam dosya, toplam satır, başarılı satır, hatalı satır, success_rate, has_count_mismatch sayısı |
| 10 | **Uyarı/Alarm** | success_rate < %95, has_count_mismatch = true, ErrorCount > 100 |
| 11 | **Örnek Çıktı Kolonları** | Tarih, Ağ, Dosya Tipi, Dosya Adedi, Toplam Satır, Başarılı, Hatalı, Başarı Oranı, Sayaç Uyumsuzluk |
| 12 | **Güncellenme Sıklığı** | Her dosya yüklemesi sonrası (gerçek zamanlı) veya saatlik |
| 13 | **Veri Kalitesi Riskleri** | expected_count = 0 ise sayaç kontrolü devre dışı |
| 14 | **Operasyon Aksiyonu** | Hatalı dosyayı incele, kaynak sistemi bilgilendir |
| 15 | **Yönetici KPI** | Günlük dosya alım başarı oranı (%), sorunlu dosya sayısı |

#### A2. Beklenen vs Gelen Dosyalar
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Beklenen vs Gelen Dosya Raporu |
| 2 | **Amaç** | Hangi dosyaların gelmediğini tespit etmek |
| 3 | **İş Kullanıcıları** | Operasyon uzmanı |
| 4 | **Karar Destek Değeri** | SLA ihlali ve eksik dosya erken uyarısı |
| 5 | **Veri Kaynakları** | `ingestion.file` + beklenen dosya takvimi (konfigürasyon). **Not:** Beklenen dosya takvimi şu an veritabanında bulunmuyor — gap olarak işaretlendi. |
| 6 | **Hesaplama Mantığı** | Beklenen dosya listesi vs ingestion.file tablosundaki dosya adları karşılaştırması |
| 7 | **Filtreler** | Tarih, Ağ |
| 8 | **Boyutlar** | Tarih, Ağ, Dosya tipi |
| 9 | **Metrikler** | Beklenen dosya sayısı, gelen dosya sayısı, eksik dosya sayısı, gecikme süresi |
| 10 | **Uyarı/Alarm** | Eksik dosya > 0 (saat bazlı kontrol) |
| 11 | **Örnek Çıktı Kolonları** | Tarih, Ağ, Beklenen Dosya Adı, Geldi mi?, Geliş Saati, Gecikme (dk) |
| 12 | **Güncellenme Sıklığı** | Saatlik |
| 13 | **Veri Kalitesi Riskleri** | Beklenen dosya takvimi yoksa rapor çalışmaz |
| 14 | **Operasyon Aksiyonu** | Kaynak sistemi ara, SFTP bağlantısını kontrol et |
| 15 | **Yönetici KPI** | Dosya SLA uyumluluk oranı (%) |

#### A3. Validasyon Hata Kırılımı
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Parse/Validasyon Hata Kırılım Raporu |
| 2 | **Amaç** | Hatalı satırların kök nedenini analiz etmek |
| 5 | **Veri Kaynakları** | `ingestion.file_line` WHERE status = 'Failed', `ingestion.file` |
| 9 | **Metrikler** | Hata sayısı (dosya/ağ/tip bazlı), en yaygın hata mesajı, hatalı satır oranı |
| 10 | **Uyarı/Alarm** | Hata oranı > %5, aynı hata 3+ dosyada tekrar |
| 12 | **Güncellenme Sıklığı** | Her dosya yüklemesi sonrası |

#### A4. Tekrar Yüklenen Dosyalar
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Tekrar Yüklenen Dosya Raporu |
| 2 | **Amaç** | Aynı FileKey ile gelen duplikat dosya yüklemelerini izlemek |
| 5 | **Veri Kaynakları** | `ingestion.file` WHERE FileKey GROUP BY HAVING COUNT > 1 |
| 9 | **Metrikler** | Tekrar yükleme sayısı, recovery tetikleme sayısı |
| 12 | **Güncellenme Sıklığı** | Günlük |

#### A5. Kurum/Network/Dosya Tipi Bazında Yükleme Performansı
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Ağ Bazlı Dosya Performans Raporu |
| 5 | **Veri Kaynakları** | `vw_file_ingestion_summary` veya `vw_daily_reconciliation_summary` |
| 8 | **Boyutlar** | Ağ (BKM/Visa/MSC), Dosya tipi (Card/Clearing), Tarih |
| 9 | **Metrikler** | Dosya adedi, satır adedi, success_rate, ortalama dosya boyutu |
| 12 | **Güncellenme Sıklığı** | Günlük |

---

### B. Reconciliation / Mutabakat Raporları

#### B1. Gün Sonu Finansal Mutabakat Özeti
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Gün Sonu Finansal Mutabakat Özeti |
| 2 | **Amaç** | Günlük mutabakat durumunu tek bakışta görmek |
| 3 | **İş Kullanıcıları** | Operasyon yöneticisi, finans ekibi |
| 4 | **Karar Destek Değeri** | Gün kapanışı onayı, eskalasyon kararı |
| 5 | **Veri Kaynakları** | `vw_reconciliation_summary_daily` veya `vw_reconciliation_matched_pair` |
| 6 | **Hesaplama Mantığı** | clean_count/total_count, unmatched/total, mismatch toplamları |
| 7 | **Filtreler** | İşlem tarihi |
| 8 | **Boyutlar** | İşlem tarihi, Ağ |
| 9 | **Metrikler** | Toplam işlem, eşleşen, eşleşmeyen, tutar uyumsuzluk, para birimi uyumsuzluk, tarih uyumsuzluk, durum çelişkisi, problem sayısı, temiz kayıt, mutabakat oranı (%) |
| 10 | **Uyarı/Alarm** | Mutabakat oranı < %95, tutar uyumsuzluk > 10 adet, eşleşmeme oranı > %5 |
| 11 | **Örnek Çıktı Kolonları** | Tarih, Ağ, Toplam, Eşleşen, Eşleşmeyen, Tutar Farkı(adet), Durum Çelişkisi(adet), Temiz, Problem, Oran(%) |
| 12 | **Güncellenme Sıklığı** | Gün sonu (T+0), sonraki günlerde güncellenen eşleşmelerle |
| 13 | **Veri Kalitesi Riskleri** | clean+problem ≠ total (duplikat Primary kaynaklı boşluk) |
| 14 | **Operasyon Aksiyonu** | Oran < %95 → kök neden araştırması; tutar uyumsuzluk → tek tek inceleme |
| 15 | **Yönetici KPI** | Günlük mutabakat oranı (%), açık fark sayısı |

#### B2. Açık Farklar ve Aging Raporu
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Açık Farklar ve Yaşlanma Raporu |
| 2 | **Amaç** | Çözülmemiş mutabakat farklarının yaşlanmasını izlemek |
| 5 | **Veri Kaynakları** | `vw_reconciliation_aging`, `vw_reconciliation_problem_records` |
| 8 | **Boyutlar** | Yaş bucket (0-1, 2-3, 4-7, 8-14, 15-30, 30+), Ağ, Dosya tipi |
| 9 | **Metrikler** | Bucket başına açık kayıt sayısı, en eski kayıt tarihi, toplam açık tutar |
| 10 | **Uyarı/Alarm** | 8-14 gün bucket > 0 → uyarı; 15+ gün → kritik; 30+ gün → acil eskalasyon |
| 12 | **Güncellenme Sıklığı** | Günlük |

#### B3. İşlem Adedi Mutabakatı
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | İşlem Adedi Mutabakat Raporu |
| 2 | **Amaç** | Kart dosyası ile clearing dosyası arasında adet uyumunu doğrulamak |
| 5 | **Veri Kaynakları** | `ingestion.file` (Card vs Clearing aynı gün/ağ), `vw_reconciliation_matched_pair` |
| 9 | **Metrikler** | Kart dosya satır sayısı, clearing dosya satır sayısı, eşleşen, eşleşmeyen (iki yönde) |
| 12 | **Güncellenme Sıklığı** | Her clearing dosyası yüklenmesi sonrası |

#### B4. Tutar Mutabakatı
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Tutar Mutabakat Raporu |
| 2 | **Amaç** | Kart ve clearing tutarlarının toplamda uyumunu kontrol etmek |
| 5 | **Veri Kaynakları** | `vw_reconciliation_matched_pair`, `vw_reconciliation_amount_mismatch` |
| 9 | **Metrikler** | Toplam kart tutarı, toplam clearing tutarı, fark (net), uyumsuz işlem sayısı, ortalama fark, max fark |
| 10 | **Uyarı/Alarm** | Net fark > belirli eşik, tekil fark > 100 TL |
| 12 | **Güncellenme Sıklığı** | Günlük |

#### B5. Acquirer / Network Bazlı Mutabakat
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Ağ Bazlı Mutabakat Karşılaştırma Raporu |
| 5 | **Veri Kaynakları** | `vw_reconciliation_summary_by_network` |
| 8 | **Boyutlar** | Network (BKM/VISA/MSC) |
| 9 | **Metrikler** | Ağ başına: toplam, eşleşen, problem, clean, mismatch detayları |
| 14 | **Operasyon Aksiyonu** | Bir ağda anomali varsa parse kuralları/dosya formatı gözden geçir |

#### B6. Chargeback / Reversal / Dispute Etkileri
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Dispute İzleme Raporu |
| 5 | **Veri Kaynakları** | `vw_clearing_dispute_monitor` |
| 8 | **Boyutlar** | Ağ, Dispute kodu, Control stat, Tarih |
| 9 | **Metrikler** | Dispute sayısı (ağ/tip bazlı), toplam dispute tutarı, anormal control_stat sayısı |
| 10 | **Uyarı/Alarm** | Dispute sayısı > günlük ortalama * 2 |
| 12 | **Güncellenme Sıklığı** | Clearing dosyası yüklendikçe |

#### B7. Status Bazlı Askıda İşlem Raporu
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Bekleyen Operasyon ve Review Raporu |
| 5 | **Veri Kaynakları** | `vw_reconciliation_pending_actions`, `vw_reconciliation_operation_tracker` |
| 9 | **Metrikler** | Pending review sayısı, süresi dolmak üzere olan review sayısı, bloke operasyon sayısı, ortalama bekleme süresi |
| 10 | **Uyarı/Alarm** | Review ExpiresAt < 4 saat, bloke operasyon > 24 saat |
| 12 | **Güncellenme Sıklığı** | Gerçek zamanlı (her sorgulamada) |

#### B8. Duplikat İşlem Raporu
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Duplikat İşlem Analiz Raporu |
| 5 | **Veri Kaynakları** | `ingestion.file_line` WHERE duplicate_status NOT IN ('Unique', NULL) |
| 9 | **Metrikler** | Duplikat grup sayısı, Primary/Secondary/Conflict dağılımı, etkilenen dosya sayısı |
| 10 | **Uyarı/Alarm** | Conflict > 0 → veri kalitesi sorunu |
| 12 | **Güncellenme Sıklığı** | Her dosya yüklenmesi sonrası |

#### B9. Dosya-Toplam / Sistem-Toplam Karşılaştırması
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Dosya Sayaç Doğrulama Raporu |
| 5 | **Veri Kaynakları** | `vw_file_ingestion_summary` (has_count_mismatch), `check_consistency.sql` kontrolleri |
| 9 | **Metrikler** | expected_count vs processed_count, success+failed = processed kontrolü |
| 10 | **Uyarı/Alarm** | has_count_mismatch = true |

#### B10. İptal / İade / Ters Kayıt Mutabakatı
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | İptal ve İade İşlem Mutabakat Raporu |
| 5 | **Veri Kaynakları** | Clearing tabloları: `clearing_msc_detail.ReversalIndicator`, BKM/Visa `TxnType` alanları, Kart tabloları: `TxnEffect`, `FinancialType` |
| 9 | **Metrikler** | İptal/iade sayısı, tutarı; ters kayıt eşleşme oranı |
| 13 | **Veri Kalitesi Riskleri** | İptal/iade işlem tiplerinin enum değerleri dokümanasyonu eksik olabilir |

---

### C. Archive Raporları

#### C1. Archive Kayıt Özeti
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Arşiv İşlem Özet Raporu |
| 5 | **Veri Kaynakları** | `vw_archive_audit_trail`, `archive.archive_log` |
| 9 | **Metrikler** | Arşivlenen dosya sayısı, başarısız arşiv sayısı, ortalama days_to_archive |
| 10 | **Uyarı/Alarm** | FailedCount > 0, days_to_archive > retention_days |
| 12 | **Güncellenme Sıklığı** | Günlük |

#### C2. Arşiv Bekleme Süresi Analizi
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Arşiv Bekleme Süresi Trend Raporu |
| 5 | **Veri Kaynakları** | `vw_archive_audit_trail` → days_to_archive |
| 9 | **Metrikler** | Ortalama/medyan/max days_to_archive, trend (artış/azalış) |
| 10 | **Uyarı/Alarm** | Ortalama > 90 gün (retention ihlali) |

#### C3. Regülasyon Saklama Raporu
| # | Alan | Detay |
|---|------|-------|
| 1 | **Rapor Adı** | Regülasyon Saklama Durumu Raporu |
| 5 | **Veri Kaynakları** | `archive.ingestion_file` + `ingestion.file` (arşivlenmemiş) |
| 9 | **Metrikler** | Saklanan toplam kayıt, saklama süresini aşan kayıt, disk kullanım trendi |
| 14 | **Operasyon Aksiyonu** | Retention aşımı varsa arşiv tetikle |

---

## 6. Mutabakat Best Practice Raporları (Top 5 Derinleştirme)

### 6.1 Gün Sonu Finansal Mutabakat Özeti

**Business Tanım:** Her iş günü sonunda kart işlem dosyaları ve clearing dosyaları arasındaki uyumun tek sayfada özetlenmesi. Türk bankacılık regülasyonları gereği gün sonu mutabakat zorunludur.

**SQL Tasarım Yaklaşımı:**
```sql
-- Mevcut vw_reconciliation_summary_daily üzerine inşa
SELECT 
    card_transaction_date AS islem_tarihi,
    network AS ag,
    total_count AS toplam_islem,
    matched_count AS eslesen,
    unmatched_count AS eslesmeyen,
    amount_mismatch_count AS tutar_uyumsuz,
    currency_mismatch_count AS doviz_uyumsuz,
    status_mismatch_count AS durum_celiskisi,
    problem_count AS problem,
    clean_count AS temiz,
    ROUND(clean_count::numeric / NULLIF(total_count, 0) * 100, 2) AS mutabakat_orani,
    -- Ek: toplam tutar kırılımı (yeni view gerekir)
    SUM(card_original_amount) AS kart_toplam_tutar,
    SUM(clearing_source_amount) AS clearing_toplam_tutar,
    SUM(card_original_amount) - SUM(clearing_source_amount) AS net_fark
FROM reporting.vw_reconciliation_matched_pair
GROUP BY card_transaction_date, network
ORDER BY card_transaction_date DESC, network;
```

**Gerekli Join Mantığı:** `vw_reconciliation_matched_pair` zaten kart-clearing join'u yapıyor. Tutar toplamları için bu view üzerine GROUP BY yeterli.

**Gerekli Anahtar Alanlar:** `card_transaction_date`, `network`, `card_original_amount`, `clearing_source_amount`, `match_status`, `has_*_mismatch`

**Örnek Reconciliation Kuralları:**
- Mutabakat oranı < %98 → UYARI
- Mutabakat oranı < %95 → KRİTİK
- Net tutar farkı > 10.000 TL → ACİL
- Eşleşmeme oranı > %5 → Clearing gecikmesi kontrol et
- Herhangi bir currency_mismatch > 0 → ACİL araştırma

**Mismatch Senaryoları:**
1. Clearing dosyası henüz gelmemiş → yüksek unmatched (geçici)
2. Parse hatası nedeniyle korelasyon başarısız → kayıp eşleşme (kalıcı)
3. Kur farkı → küçük tutar farkları (< 0.01 tolerans dahilinde)
4. Chargeback/dispute → status mismatch (beklenen davranış)
5. Duplikat işlem → adet farkı

**Alarm Eşikleri:**

| Metrik | Normal | Uyarı | Kritik |
|--------|--------|-------|--------|
| Mutabakat Oranı | > %98 | %95-%98 | < %95 |
| Eşleşmeme Oranı | < %2 | %2-%5 | > %5 |
| Tutar Uyumsuzluk Adedi | 0 | 1-10 | > 10 |
| Net Tutar Farkı | 0 | < 1.000 TL | > 10.000 TL |

**Drill-down Akışı:**
1. Gün sonu özet → Ağ bazlı kırılım → Dosya bazlı kırılım → İşlem detayı
2. Problem sayısına tıkla → Problem kayıtlar listesi → Tek işlem detayı (kart + clearing yan yana)

**Dashboard Widget Önerileri:**
- **Kart:** Günlük mutabakat oranı (büyük sayı + renk: yeşil/sarı/kırmızı)
- **Trend Grafiği:** Son 30 gün mutabakat oranı çizgi grafik
- **Bar Chart:** Ağ bazlı problem dağılımı
- **Pie Chart:** Problem türü dağılımı (unmatched, amount, currency, status, duplicate)

---

### 6.2 Açık Farklar ve Aging Raporu

**Business Tanım:** Ready, Processing veya Failed durumundaki kayıtların ne kadar süredir açık olduğunu yaş aralıklarına göre gösteren rapor. SLA ihlallerini ve birikim trendlerini izler.

**SQL Tasarım Yaklaşımı:**
```sql
-- Mevcut vw_reconciliation_aging üzerine
SELECT 
    age_bucket,
    age_bucket_order,
    content_type,
    file_type,
    open_count,
    oldest_record_date,
    newest_record_date,
    -- Ek: toplam açık tutar (yeni view gerekir)
    SUM(CASE WHEN f.file_type = 'Card' THEN cd.original_amount ELSE 0 END) AS acik_tutar
FROM reporting.vw_reconciliation_aging a
-- Tutar için card detail join gerekir
GROUP BY age_bucket, age_bucket_order, content_type, file_type
ORDER BY age_bucket_order;
```

**Mismatch Senaryoları:**
- 0-1 gün: Normal akış, clearing bekleniyor
- 2-3 gün: Clearing gecikmesi olabilir
- 4-7 gün: Evaluate/Execute çalışmıyor olabilir
- 8+ gün: Kalıcı sorun, manuel müdahale gerekir

**Alarm Eşikleri:**

| Yaş Bucket | Kayıt Sayısı Eşik | Aksiyon |
|------------|-------------------|---------|
| 0-1 Gün | Bilgi | İzle |
| 2-3 Gün | > 1.000 | Clearing kontrolü |
| 4-7 Gün | > 100 | Pipeline kontrolü |
| 8-14 Gün | > 0 | Acil müdahale |
| 15-30 Gün | > 0 | Eskalasyon |
| 30+ Gün | > 0 | Yönetici bilgilendirme |

**Dashboard Widget:** Stacked bar chart (yaş bucket'ları renkli), yanında trend çizgisi (haftalık toplam açık kayıt).

---

### 6.3 Dosya-Sistem-Core Banking Üçlü Mutabakat Raporu

**Business Tanım:** Dosya (fiziksel kaynak) → Sistem (ingestion+reconciliation) → Core Banking (otorisasyon) üç katmanlı karşılaştırma. Her katmandaki adet ve tutar toplamlarının birbiriyle uyumunu doğrular.

**SQL Tasarım Yaklaşımı:**
```sql
-- Katman 1: Dosya (footer'dan beklenen)
SELECT 'DOSYA' AS katman, f.id, f.file_name, f.expected_line_count AS adet
FROM ingestion.file f

-- Katman 2: Sistem (gerçek parse edilen)
SELECT 'SİSTEM' AS katman, f.id, f.file_name, 
    COUNT(fl.id) FILTER (WHERE fl.line_type = 'D') AS adet
FROM ingestion.file f
LEFT JOIN ingestion.file_line fl ON fl.file_id = f.id
GROUP BY f.id

-- Katman 3: Core Banking
-- **BELİRSİZ:** DebitAuthorization tablosu mevcut ama dosya satırlarıyla 
-- doğrudan birleştiren bir foreign key veya correlation key yok.
-- OceanTxnGuid üzerinden JOIN mümkün olabilir ama bu ilişki 
-- şu an repo'da tanımlı değil.
```

**Gerekli Anahtar Alanlar:** `OceanTxnGuid` (ingestion ↔ core banking köprüsü), `FileKey`, `expected_count`, `processed_count`

**Gap:** Core banking (DebitAuthorization) ile ingestion arasında doğrudan bir foreign key ilişkisi yok. `OceanTxnGuid` her ikisinde de mevcut ancak JOIN tanımı yapılmamış. **Bu ilişkinin kurulması öncelikli geliştirme gerektiriyor.**

**Dashboard Widget:** 3 katmanlı waterfall chart: Dosya (beklenen) → Sistem (işlenen) → Core Banking (otorise edilen). Fark alanları kırmızı.

---

### 6.4 Validasyon/Hata Kaynaklı Finansal Etki Raporu

**Business Tanım:** Parse/validasyon hatası alan satırların potansiyel finansal etkisini ölçer. Hatalı satırlar mutabakata giremediği için "kör nokta" oluşturur.

**SQL Tasarım Yaklaşımı:**
```sql
SELECT 
    f.file_name,
    f.content_type AS ag,
    COUNT(fl.id) AS hatali_satir_sayisi,
    -- Hatalı satırların raw_data'sından tutar çıkarılamayabilir
    -- ParsedData JSON'dan tutar alanı okunabilir
    SUM(CASE 
        WHEN fl.parsed_data IS NOT NULL 
        THEN (fl.parsed_data::json->>'OriginalAmount')::numeric 
        ELSE 0 
    END) AS tahmini_etki_tutar
FROM ingestion.file_line fl
JOIN ingestion.file f ON f.id = fl.file_id
WHERE fl.status = 'Failed' AND fl.line_type = 'D'
GROUP BY f.file_name, f.content_type
ORDER BY hatali_satir_sayisi DESC;
```

**Alarm Eşikleri:** Hatalı satır tutarı > 50.000 TL → acil müdahale

**Dashboard Widget:** Hatalı satır sayısı ve tahmini tutar etkisi kart + trend.

---

### 6.5 Kurum ve Kart Şeması Bazlı İşlem/Tutar Uzlaşma Raporu

**Business Tanım:** BKM/Visa/Mastercard bazında işlem adedi ve tutar toplamlarının karşılaştırılması. Hangi ağda ne kadar fark var, fark trendi nasıl?

**SQL Tasarım Yaklaşımı:**
```sql
SELECT 
    mp.network AS ag,
    COUNT(*) AS toplam_islem,
    COUNT(*) FILTER (WHERE mp.match_status = 'MATCHED') AS eslesen,
    SUM(mp.card_original_amount) AS kart_toplam,
    SUM(mp.clearing_source_amount) AS clearing_toplam,
    SUM(mp.card_original_amount) - SUM(COALESCE(mp.clearing_source_amount, 0)) AS net_fark,
    SUM(ABS(COALESCE(mp.amount_difference, 0))) AS toplam_mutlak_fark,
    COUNT(*) FILTER (WHERE mp.has_amount_mismatch = TRUE) AS tutar_uyumsuz_adet,
    ROUND(
        COUNT(*) FILTER (WHERE mp.match_status = 'MATCHED' 
            AND mp.has_amount_mismatch = FALSE)::numeric 
        / NULLIF(COUNT(*), 0) * 100, 2
    ) AS uzlasma_orani
FROM reporting.vw_reconciliation_matched_pair mp
GROUP BY mp.network;
```

**Reconciliation Kuralları:**
- Net fark BKM için < %0.01 beklenir (yerel işlem, kur farkı düşük)
- Net fark Visa/MSC için < %0.5 kabul edilebilir (uluslararası işlem, kur etkisi)
- Uzlaşma oranı < %97 → ağ bazlı detay inceleme

**Dashboard Widget:** Ağ bazlı 3 kart (BKM/Visa/MSC) — her birinde uzlaşma oranı, net fark, trend.

---

## 7. Gap Analizi

### 7.1 Eksik Tablo/Alan

| # | Eksiklik | Etki | Öncelik | Öneri |
|---|----------|------|---------|-------|
| 1 | **Beklenen dosya takvimi tablosu yok** | "Beklenen vs gelen dosya" raporu üretilemez, SLA takibi yapılamaz | YÜKSEK | `ingestion.expected_file_schedule` tablosu oluştur (network, file_type, expected_time, frequency) |
| 2 | **Core banking (DebitAuthorization) → Ingestion ilişkisi tanımsız** | Üçlü mutabakat (dosya-sistem-core banking) yapılamaz | YÜKSEK | `OceanTxnGuid` üzerinden ilişki kurulması veya ayrı bir mapping tablosu |
| 3 | **GL/muhasebe hesap eşleştirmesi yok** | Finansal etki raporu, muhasebe mutabakatı yapılamaz | ORTA | Eğer core banking'de GL hareket varsa, GL account mapping tablosu gerekir |
| 4 | **Dosya işleme süresi (başlangıç-bitiş) ayrı takip edilmiyor** | Performans SLA ölçümü zor | ORTA | `ingestion.file` tablosuna `processing_started_at`, `processing_completed_at` ekle |
| 5 | **Komisyon/ücret reconciliation tablosu yok** | Komisyon farkı raporu üretilemez | ORTA | `DebitAuthorizationFee` ile clearing reimbursement karşılaştırması için view |
| 6 | **Kur bilgisi (exchange rate) saklanmıyor** | Kur farkı analizi yapılamaz | DÜŞÜK | Mismatch olan kayıtlarda günlük kur bilgisi ile cross-check |

### 7.2 Eksik Audit Trail

| # | Eksiklik | Etki | Öneri |
|---|----------|------|-------|
| 1 | **Dosya indirme audit log yok** | SFTP bağlantı hataları, dosya indirme süreleri izlenemiyor | SFTP transfer log tablosu |
| 2 | **Evaluate/Execute çağrı logu yok** | Kim, ne zaman, hangi parametrelerle çalıştırdı bilgisi yok | API call audit log |
| 3 | **Manuel review karar geçmişi sınırlı** | Reviewer sadece son karar, geçmiş kararlar sorgulanamıyor | Review karar tek bir kayıt (yeterli), ancak Comment alanı opsiyonel — zorunlu yapılmalı |

### 7.3 Eksik Status Takibi

| # | Eksiklik | Etki | Öneri |
|---|----------|------|-------|
| 1 | **Clearing dosyası bekleme durumu izlenmiyor** | Clearing gelmeden önce eşleşmeme "beklenen" mi "sorunlu" mu ayrılamıyor | Kart dosyası yüklendiğinde beklenen clearing dosyası bilgisi kaydedilmeli |
| 2 | **ReconciliationStatus = Failed sonrası recovery akışı yok** | Failed satırlar tekrar evaluate edilemiyor | Re-evaluate mekanizması veya status reset imkanı |

### 7.4 Eksik Correlation Key

| # | Eksiklik | Etki | Öneri |
|---|----------|------|-------|
| 1 | **DebitAuthorization ↔ IngestionFileLine doğrudan bağı yok** | Otorisasyon → clearing → dosya zinciri kırık | `OceanTxnGuid` üzerinden correlation view oluştur |
| 2 | **Clearing dosyasındaki FileId alanı kullanılmıyor** | Clearing dosyaları arası cross-reference eksik | FileId alanını raporlamada kullan |

### 7.5 Mutabakatı Zorlaştıran Veri Boşlukları

| # | Boşluk | Etki | Öneri |
|---|--------|------|-------|
| 1 | **Tarih karşılaştırması string tabanlı** | Farklı format (20260415 vs 2026-04-15) yanlış pozitif üretir | Integer/date normalize karşılaştırma |
| 2 | **NULL tutar = 0 varsayımı** | Gerçekten 0 tutarlı işlem ile eksik veri ayrılamıyor | NULL'lar ayrı kategori olarak izlenmeli |
| 3 | **clean_count + problem_count ≠ total_count** | Operasyon ekibi için kafa karıştırıcı | Tanımları hizala veya "gri alan" kategorisi ekle |
| 4 | **vw_reconciliation_clean_matched'de has_status_mismatch kontrol edilmiyor** | Status mismatch olan kayıtlar "temiz" sayılabilir | Clean tanımına status_mismatch = FALSE ekle |
| 5 | **Alert view'ında INNER JOIN** | EvaluationFailed alert'leri (OperationId = Empty) görünmüyor | LEFT JOIN kullan veya Guid.Empty kontrolü ekle |

### 7.6 Operasyon Ekibinin Göremediği Kör Noktalar

| # | Kör Nokta | Açıklama |
|---|-----------|----------|
| 1 | **Clearing olmadan eşleşme beklentisi** | Clearing dosyası ne zaman gelmeli bilinmiyor; eşleşmeme "normal gecikme" mi "kayıp işlem" mi ayrımı yapılamıyor |
| 2 | **Dosya formatı değişikliği tespiti** | Parse hatası artışı → format değişikliği olabilir; bu ilişki otomatik izlenmiyor |
| 3 | **Retry tükenmiş operasyonlar** | Failed operasyonlar alert oluşturuyor ama bunların toplu çözümü için araç yok |
| 4 | **Arşiv öncesi veri bütünlüğü** | Arşiv sonrası veri geri getirilemez; arşiv öncesi snapshot doğrulaması yok |
| 5 | **Cross-file duplikat** | Duplikat tespiti dosya içi yapılıyor; dosyalar arası duplikat kontrolü belirsiz |

### 7.7 Önceliklendirilmiş Öneri Listesi

| # | Öneri | Etki | Efor | Öncelik |
|---|-------|------|------|---------|
| 1 | Beklenen dosya takvimi tablosu | SLA raporu, eksik dosya alarmı | Düşük | 🔴 Acil |
| 2 | Core banking ilişki kurulumu (OceanTxnGuid) | Üçlü mutabakat | Orta | 🔴 Acil |
| 3 | Clean matched tanımına status_mismatch ekle | Doğru KPI | Düşük | 🔴 Acil |
| 4 | Alert view'ını LEFT JOIN yap | Evaluation alert görünürlüğü | Düşük | 🟡 Yüksek |
| 5 | Dosya işleme süresi alanları | Performans SLA | Düşük | 🟡 Yüksek |
| 6 | Tarih karşılaştırmasını integer yap | Yanlış pozitif azaltma | Düşük | 🟡 Yüksek |
| 7 | Komisyon reconciliation view | Komisyon farkı raporu | Orta | 🟡 Yüksek |
| 8 | SFTP transfer audit log | Bağlantı izleme | Orta | 🟢 Normal |
| 9 | ReconciliationStatus re-evaluate mekanizması | Failed kurtarma | Orta | 🟢 Normal |
| 10 | GL hesap eşleştirmesi | Muhasebe mutabakatı | Yüksek | 🟢 Normal |

---

## 8. Hızlı Kazanımlar

Bu bölüm, minimum eforla maksimum operasyonel değer sağlayacak adımları listeler.

| # | Kazanım | Süre | Değer |
|---|---------|------|-------|
| 1 | `vw_reconciliation_clean_matched` view'ına `has_status_mismatch = FALSE` filtresi ekle | 30 dk | KPI doğruluğu |
| 2 | `vw_reconciliation_alert_dashboard` view'ında INNER JOIN → LEFT JOIN yap | 30 dk | Evaluation alert görünürlüğü |
| 3 | Mevcut `vw_reconciliation_summary_daily` üzerine tutar toplamları ekle | 1 saat | Finansal mutabakat |
| 4 | `vw_file_ingestion_summary` üzerine ortalama işleme süresi (update_date - create_date) ekle | 1 saat | Performans izleme |
| 5 | Günlük otomatik rapor e-postası (summary endpoint verisi) | 2-4 saat | Proaktif izleme |
| 6 | Operasyon dashboard'u için REST endpoint (tüm KPI'ları tek çağrıda döner) | 4-8 saat | UX iyileştirme |
| 7 | Clearing dosyası gelmezse T+1'de otomatik alert | 2-4 saat | SLA izleme |

---

## 9. Fazlandırılmış Uygulanabilir Yol Haritası

### Faz 1: Temel İyileştirmeler (2 hafta)
- [ ] View düzeltmeleri (clean_matched, alert_dashboard)
- [ ] Tutar toplamları eklenmesi (summary view'ları)
- [ ] Beklenen dosya takvimi tablosu oluşturma
- [ ] Operasyon dashboard REST endpoint'i
- [ ] Temel alarm mekanizması (günlük e-posta özet)

### Faz 2: Rapor Katmanı (4 hafta)
- [ ] Yeni reporting view'ları:
  - Tutar mutabakat özeti (ağ/tarih bazlı)
  - Dosya-sistem sayaç doğrulama
  - Duplikat analiz view
  - Komisyon reconciliation view
- [ ] Dosya işleme süresi metrikleri
- [ ] Aging raporu iyileştirme (tutar dahil)
- [ ] Dashboard UI (frontend — kapsamda değilse API hazırlığı)

### Faz 3: Derinleştirme (4 hafta)
- [ ] Core banking ilişki kurulumu (OceanTxnGuid üzerinden)
- [ ] Üçlü mutabakat raporu
- [ ] Finansal etki raporu (hatalı satırlar)
- [ ] Dispute izleme dashboard
- [ ] Materialized view veya cache stratejisi (performans)
- [ ] ReconciliationStatus re-evaluate mekanizması

### Faz 4: İleri Düzey (6 hafta)
- [ ] GL/muhasebe entegrasyonu (varsa)
- [ ] Kur farkı analizi
- [ ] Otomatik anomali tespiti (trend analizi)
- [ ] Self-service rapor platformu (BI tool entegrasyonu)
- [ ] API gateway metrik entegrasyonu (Grafana/Prometheus)

---

## Öncelikli Geliştirilecek İlk 10 Rapor

| # | Rapor | Neden Öncelikli | Mevcut Veri Yeterliliği |
|---|-------|-----------------|------------------------|
| 1 | **Gün Sonu Finansal Mutabakat Özeti** | Her gün zorunlu kontrol | ✅ Mevcut view'larla üretilebilir (tutar eklenmeli) |
| 2 | **Açık Farklar ve Aging Raporu** | SLA takibi, birikim izleme | ✅ vw_reconciliation_aging mevcut |
| 3 | **Günlük Dosya Alım Özeti** | Tüm sürecin başlangıç kontrolü | ✅ vw_file_ingestion_summary mevcut |
| 4 | **Beklenen vs Gelen Dosyalar** | Eksik dosya erken uyarısı | ❌ Beklenen dosya takvimi tablosu gerekli |
| 5 | **Ağ Bazlı Mutabakat Karşılaştırma** | Hangi ağda sorun var tespiti | ✅ vw_reconciliation_summary_by_network mevcut |
| 6 | **Dispute İzleme Raporu** | Chargeback/dispute finansal etki | ✅ vw_clearing_dispute_monitor mevcut |
| 7 | **Bekleyen Operasyon ve Review Raporu** | Operatör iş kuyruğu | ✅ vw_reconciliation_pending_actions mevcut |
| 8 | **Tutar Mutabakat Raporu** | Finansal doğrulama | ⚠️ Tutar toplamları view'a eklenmeli |
| 9 | **Validasyon/Hata Kaynaklı Finansal Etki** | Kör nokta tespiti | ⚠️ ParsedData JSON'dan tutar çekmek gerekir |
| 10 | **Arşiv İşlem Özet Raporu** | Saklama/temizlik izleme | ✅ vw_archive_audit_trail mevcut |

---

## Ek Görev A: Operasyon Ekibi İçin KPI Sözlüğü

| KPI Adı | Tanım | Hesaplama | Normal Aralık | Alarm Eşiği | Kaynak |
|---------|-------|-----------|---------------|-------------|--------|
| **Mutabakat Oranı** | Temiz (sorunsuz eşleşmiş) kayıtların toplam kayıtlara oranı | clean_count / total_count * 100 | > %98 | < %95 | vw_reconciliation_summary_daily |
| **Dosya Alım Başarı Oranı** | Başarıyla parse edilen satırların oranı | success_count / processed_count * 100 | > %99 | < %95 | vw_file_ingestion_summary |
| **Eşleşme Oranı** | Clearing karşılığı bulunan kayıtların oranı | matched_count / total_count * 100 | > %98 | < %95 | vw_reconciliation_summary_daily |
| **Tutar Uyumsuzluk Sayısı** | Tutar farkı > 0.01 olan eşleşmiş kayıt sayısı | COUNT WHERE has_amount_mismatch = TRUE | 0 | > 10 | vw_reconciliation_matched_pair |
| **Ortalama Yaşlanma** | Açık kayıtların ortalama yaşı (gün) | AVG(age_days) WHERE status IN (Ready, Processing, Failed) | < 2 gün | > 5 gün | vw_reconciliation_aging |
| **Pending Review Sayısı** | Karar bekleyen manuel inceleme sayısı | COUNT WHERE decision = Pending | 0 | > 10 | reconciliation.review |
| **Açık Alert Sayısı** | İşlenmemiş uyarı sayısı | COUNT WHERE alert_status = Pending | 0 | > 5 | reconciliation.alert |
| **Arşiv SLA** | Dosya yüklenmesinden arşivlemeye kadar gün | days_to_archive ortalama | < 90 gün | > 90 gün | vw_archive_audit_trail |
| **Retry Oranı** | Retry gerektiren operasyonların oranı | retry_count > 0 / total_operations | < %5 | > %15 | reconciliation.operation |
| **Dosya SLA Uyumu** | Zamanında gelen dosyaların oranı | Gelen / Beklenen * 100 | %100 | < %100 | **Yeni tablo gerekli** |

---

## Ek Görev B: Veri Sözlüğü

### Status Değerleri
(Yukarıda Bölüm 4.2'de detaylı verildi)

### Dosya Tipleri

| Kod | Açıklama | Kaynak Dizin | Dosya Pattern | Satır Uzunluğu |
|-----|----------|-------------|---------------|----------------|
| Card:Bkm | BKM kart işlem dosyası | /PAYCORE_REPORTS | CARD_TRANSACTIONS_YYYYMMDD_N.txt | 706 byte |
| Card:Visa | Visa kart işlem dosyası | /PAYCORE_REPORTS | CARD_TRANSACTIONS_YYYYMMDD_N.txt | 706 byte |
| Card:Msc | MSC kart işlem dosyası | /PAYCORE_REPORTS | CARD_TRANSACTIONS_YYYYMMDD_N.txt | 706 byte |
| Clearing:Bkm | BKM clearing dosyası | /BKM_REPORTS/OUTGOING | BKMACC+tarih.txt | 331 byte |
| Clearing:Visa | Visa clearing dosyası | /VISA_REPORTS/OUTGOING | VISAACC+tarih.txt | 321 byte |
| Clearing:Msc | MSC clearing dosyası | /MASTERCARD_REPORTS/OUTGOING | MSCACC+tarih.txt | 347 byte |

### İşlem Referans Alanları

| Alan | Açıklama | Kullanıldığı Yer |
|------|----------|------------------|
| OceanTxnGuid | İşlem benzersiz ID (Paycore/Ocean sisteminden) | Korelasyon anahtarı #1, duplikat tespiti, clearing eşleştirme |
| OceanMainTxnGuid | Ana işlem ID (taksitli işlemlerde ana işlem) | Kart detayı |
| Rrn | Retrieval Reference Number — işlem takip numarası | Fallback korelasyon, işlem sorgulama |
| Arn | Acquirer Reference Number — otorisasyon referansı | Fallback korelasyon |
| ProvisionCode | Otorisasyon kodu | Fallback korelasyon |
| ClrNo | Clearing numarası | Clearing duplikat tespiti |
| ControlStat | Clearing kontrol durumu (Normal/anormal) | Status mismatch, dispute tespiti |
| DisputeCode | Dispute kodu (None/diğer) | Dispute izleme |
| Mcc | Merchant Category Code | İşlem sınıflandırma |

---

## Ek Görev C: Reconciliation İçin Canonical Data Model Önerisi

Tüm ağ verilerini normalize eden bir canonical model:

```
ReconciliationCanonicalRecord
├── record_id (GUID)
├── source: 'CARD' | 'CLEARING'
├── network: 'BKM' | 'VISA' | 'MSC'
├── file_id (FK → ingestion.file)
├── file_line_id (FK → ingestion.file_line)
├── correlation_key (OceanTxnGuid veya composite)
│
├── -- Referans Alanları --
├── ocean_txn_guid
├── rrn
├── arn
├── provision_code
├── card_no (masked)
│
├── -- Tutar Alanları --
├── original_amount
├── original_currency
├── settlement_amount
├── settlement_currency
├── billing_amount
├── billing_currency
│
├── -- Tarih Alanları --
├── transaction_date (int, YYYYMMDD)
├── transaction_time
├── value_date
│
├── -- Durum Alanları --
├── is_successful (boolean)
├── txn_status
├── control_stat (clearing only)
├── dispute_code (clearing only)
│
├── -- Üye İş Yeri --
├── merchant_name
├── merchant_city
├── merchant_country
├── mcc
├── terminal_id
├── merchant_id
│
├── -- Mutabakat Alanları --
├── reconciliation_status
├── match_status
├── matched_counterpart_id
├── duplicate_status
├── duplicate_group_id
│
├── -- Mismatch Flags (hesaplanmış) --
├── has_amount_mismatch
├── has_currency_mismatch
├── has_date_mismatch
├── has_status_mismatch
├── amount_difference
│
├── -- Audit --
├── create_date
├── update_date
├── record_status
```

**Not:** Bu model mevcut `vw_base_card_transaction` ve `vw_base_clearing_transaction` view'larının zaten uyguladığı normalizasyona karşılık gelir. Materialized view olarak oluşturulması performans açısından önerilir.

---

## Ek Görev D: Dashboard Menü Ağacı Önerisi

```
📊 Payify Operasyon Dashboard
│
├── 🏠 Genel Bakış (Summary Overall)
│   ├── Mutabakat oranı kartı
│   ├── Açık fark sayısı kartı
│   ├── Pending review sayısı kartı
│   ├── Açık alert sayısı kartı
│   └── Son 7 gün trend grafiği
│
├── 📁 Dosya Yönetimi
│   ├── Günlük Dosya Alım Özeti
│   ├── Beklenen vs Gelen Dosyalar
│   ├── Dosya Detay (tek dosya drill-down)
│   ├── Parse Hata Analizi
│   └── Tekrar Yüklenen Dosyalar
│
├── 🔄 Mutabakat
│   ├── Gün Sonu Özet
│   ├── Ağ Bazlı Karşılaştırma
│   ├── Dosya Bazlı Analiz
│   ├── İşlem Listesi (filtreli)
│   │   ├── Tüm İşlemler
│   │   ├── Problemli Kayıtlar
│   │   ├── Eşleşmemiş Kayıtlar
│   │   └── Tutar Uyumsuzlukları
│   ├── Duplikat Analizi
│   └── Dispute İzleme
│
├── ⏳ Yaşlanma ve Takip
│   ├── Aging Raporu (bucket chart)
│   ├── Bekleyen Operasyonlar
│   ├── Bekleyen Reviews (karar kuyruğu)
│   └── Alert Dashboard
│
├── 📦 Arşiv
│   ├── Arşiv Özeti
│   ├── Arşiv Audit Trail
│   └── Saklama Süresi Analizi
│
├── 📈 KPI ve Trend
│   ├── Günlük KPI Trend
│   ├── Haftalık Karşılaştırma
│   └── Aylık Yönetici Özeti
│
└── ⚙️ Yönetim
    ├── Parametre Yönetimi
    ├── Alarm Konfigürasyonu
    └── Sistem Sağlık Durumu
```

---

## Ek Görev E: Alarm ve Exception Management Tasarımı

### Alarm Kategorileri

| Kategori | Tetikleyici | Severity | Bildirim Kanalı | Aksiyon |
|----------|------------|----------|-----------------|---------|
| **Dosya Gelmedi** | Beklenen dosya T+X saat içinde gelmedi | HIGH | E-posta + SMS | Kaynak kontrol et |
| **Dosya Parse Hatası** | success_rate < %95 | HIGH | E-posta | Dosya formatı kontrol et |
| **Sayaç Uyumsuzluk** | has_count_mismatch = true | MEDIUM | E-posta | Dosya bütünlüğünü kontrol et |
| **Yüksek Eşleşmeme** | Unmatched oranı > %5 | HIGH | E-posta | Clearing dosyasını kontrol et |
| **Tutar Uyumsuzluk** | Tutar fark > 100 TL | CRITICAL | E-posta + SMS | Acil araştırma |
| **Currency Mismatch** | Herhangi bir currency mismatch | CRITICAL | E-posta + SMS | Veri hatası araştır |
| **Aging Kritik** | 15+ gün bucket > 0 | HIGH | E-posta | Eskalasyon |
| **Aging Acil** | 30+ gün bucket > 0 | CRITICAL | E-posta + SMS | Yönetici bilgilendir |
| **Review Süre Dolmak Üzere** | ExpiresAt < 4 saat | MEDIUM | E-posta | Karar ver |
| **Operasyon Retry Tükendi** | OperationStatus = Failed | HIGH | E-posta | Manuel müdahale |
| **SFTP Bağlantı Hatası** | Dosya çekme hatası | HIGH | E-posta + SMS | Altyapı kontrol |
| **Arşiv Hatası** | Archive FailedCount > 0 | MEDIUM | E-posta | Disk/DB kontrol |
| **Mutabakat Oranı Düşük** | Günlük oran < %95 | CRITICAL | E-posta + SMS | Kök neden araştır |
| **Duplikat Conflict** | Conflict duplikat tespit | MEDIUM | E-posta | Kaynak inceleme |

### Exception Akışı

```
Alarm Tetiklenir
    │
    ▼
┌──────────────┐
│ reconciliation│
│ .alert        │ AlertStatus = Pending
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ Alert Service │ → E-posta template (ReconciliationAlertTemplate)
│ (Consumer)    │   Hedef: operasyon@company.com
└──────┬───────┘
       │ Başarılı → AlertStatus = Consumed
       │ Başarısız → AlertStatus = Failed (retry yok)
       ▼
┌──────────────┐
│ Operatör     │
│ Dashboard    │ → Alert Dashboard view'ından izleme
│              │ → Severity + Age bazlı önceliklendirme
└──────┬───────┘
       │ Karar
       ▼
┌──────────────────────────────────┐
│ Aksiyon:                         │
│ • Review approve/reject          │
│ • Dosya yeniden yükleme tetikle  │
│ • Kaynak sistem bilgilendirme    │
│ • Manuel araştırma başlat        │
│ • Alert'i Ignored olarak işaretle│
└──────────────────────────────────┘
```

### Eskalasyon Matrisi

| Süre | Aksiyon |
|------|---------|
| 0-1 saat | Operasyon uzmanı dashboard'dan izler |
| 1-4 saat | Otomatik e-posta (mevcut) |
| 4-8 saat | Takım lideri bilgilendirme |
| 8-24 saat | Yönetici eskalasyon |
| 24+ saat | Üst yönetim + kaynak kurum bilgilendirme |

---

> **Doküman Sonu**
> 
> Bu doküman, backend.card repo'sunun kapsamlı analizine dayanmaktadır. Tüm tespitler kaynak kod, entity modelleri, SQL dosyaları, vault konfigürasyonları ve teknik dokümanlardan türetilmiştir. "Belirsiz" olarak işaretlenen noktalar ilgili veri veya kodun repo'da bulunmamasından kaynaklanmaktadır.

