# Payify Owned Process Design — File Ingestion · Reconciliation · Archive

> **Tarih:** 2026-04-16  
> **Versiyon:** 1.0  
> **Yazar:** Solution Architecture & Product Analysis  

---

## 1. Kapsam ve Süreç Sahipliği

**Bu çalışma yalnızca Payify'nin sahip olduğu file ingestion, reconciliation ve archive süreç tablolarını kapsar. Harici servisler ve dış domain tabloları bu modelin parçası değildir.**

### 1.1 File Ingestion

| Başlık | Açıklama |
|--------|----------|
| **İş amacı** | Harici processor'dan (Ocean) gelen Card Transaction ve Clearing/ACC dosyalarını alıp, parse edip, doğrulayıp, normalize ederek iç veri modeline yazmak. |
| **Başlangıç tetikleyicisi** | Zamanlayıcı (cron/scheduler) veya SFTP/local dizin taraması ile yeni dosya tespiti. |
| **Bitiş koşulu** | Dosyadaki tüm satırlar parse + validate + duplicate kontrol + business key üretim aşamalarından geçti; `IngestionFile.Status = Success ∨ Failed` yazıldı. |
| **Operasyon neden takip eder** | Dosya gelmezse veya parse/validation hata oranı yükselirse müşteri işlemleri eşlenemez, gölge bakiye riski oluşur. |

### 1.2 Reconciliation

| Başlık | Açıklama |
|--------|----------|
| **İş amacı** | Card Transaction dosyasındaki satırları Clearing dosyasındaki satırlarla OceanTxnGuid üzerinden eşleştirmek, farkları tespit etmek, düzeltme aksiyonları üretmek ve finansal etkiyi izlemek. |
| **Başlangıç tetikleyicisi** | İlgili Card + Clearing dosya çiftinin ingestion'ı tamamlandığında (her iki tarafın `Status = Success`). |
| **Bitiş koşulu** | Tüm satırlar evaluation'dan geçti; match / mismatch / action / review kayıtları yazıldı; `ReconciliationStatus ∈ {Success, Failed}` güncellendi. |
| **Operasyon neden takip eder** | Eşleşmeyen kayıtlar finansal fark demektir. Açık farklar yaşlandıkça gölge bakiye riski artar, regülatif risk doğar. |

### 1.3 Archive

| Başlık | Açıklama |
|--------|----------|
| **İş amacı** | Reconciliation'ı tamamlanmış ve retention süresini doldurmuş dosya/satır verilerini archive tablolarına taşımak, denetim izini korumak. |
| **Başlangıç tetikleyicisi** | Zamanlayıcı; `IngestionFile.IsArchived = false` ve reconciliation tamamlanmış + retention günü dolmuş dosyalar. |
| **Bitiş koşulu** | `ArchiveLog.Status ∈ {Success, Failed}` yazıldı; başarılıysa `IngestionFile.IsArchived = true`. |
| **Operasyon neden takip eder** | Arşiv başarısız olursa canlı tablolar şişer, performans düşer. Denetim izi kaybolursa regülatif risk oluşur. |

---

## 2. Owned Process Inventory

| # | Bileşen | Tip | Süreç | Amaç | Girdi | Çıktı | Operasyonel Önemi |
|---|---------|-----|-------|------|-------|-------|-------------------|
| 1 | `ingestion_file` | owned db table | Ingestion | Dosya meta verisi ve işleme durumu | SFTP/local dosya | Dosya kaydı (status, count) | Dosya geldi mi, başarılı mı? |
| 2 | `ingestion_file_line` | owned db table | Ingestion | Her satırın parse/validate/duplicate/recon durumu | Dosya satırı | Satır kaydı (parsed, status, dup, recon) | Satır bazlı izleme |
| 3 | `ingestion_card_*_detail` | owned db table | Ingestion | Card Transaction parse edilmiş detay (Visa/Msc/Bkm) | ParsedData JSON | Typed detail | Raporlama ve eşleşme |
| 4 | `ingestion_clearing_*_detail` | owned db table | Ingestion | Clearing/ACC parse edilmiş detay (Visa/Msc/Bkm) | ParsedData JSON | Typed detail | Raporlama ve eşleşme |
| 5 | `ingestion_file_control` *(yeni)* | owned db table | Ingestion | Header/footer kontrolü, beklenen vs gerçek sayı | H/F satırları | Kontrol sonucu | Bütünlük teyidi |
| 6 | `ingestion_parse_error` *(yeni)* | owned audit/log | Ingestion | Parse hatası detayı | Hatalı satır | Hata log | Hata spike tespiti |
| 7 | `ingestion_validation_error` *(yeni)* | owned audit/log | Ingestion | Zorunlu alan / format / tutarlılık hataları | Parse edilmiş satır | Hata log | Veri kalitesi izleme |
| 8 | `ingestion_duplicate_log` *(yeni)* | owned audit/log | Ingestion | Duplicate detection kararları | DuplicateDetectionKey | Karar logu | Duplicate oranı takibi |
| 9 | `ingestion_business_key` *(yeni)* | owned db table | Ingestion | Üretilmiş canonical business key | Parsed detail | Business key kaydı | Recon eşleşme temeli |
| 10 | `ingestion_status_history` *(yeni)* | owned audit/log | Ingestion | Dosya/satır durum geçiş logu | Status değişimi | Audit trail | Operasyonel iz |
| 11 | `reconciliation_evaluation` | owned db table | Reconciliation | Satır evaluation sonucu; kaç operation üretildi | FileLine | Evaluation kaydı | Eşleşme kalitesi |
| 12 | `reconciliation_operation` | owned db table | Reconciliation | Düzeltme / aksiyon kaydı (D1-D8 dahil) | Evaluation | Operation kaydı | Aksiyon takibi |
| 13 | `reconciliation_operation_execution` | owned db table | Reconciliation | Her execution denemesi | Operation | Execution kaydı | Retry / başarı takibi |
| 14 | `reconciliation_review` | owned db table | Reconciliation | Manuel onay/ret akışı | Operation | Review kaydı | SLA izleme |
| 15 | `reconciliation_alert` | owned db table | Reconciliation | Alarm kaydı | Evaluation/Operation | Alert kaydı | Alarm yönetimi |
| 16 | `reconciliation_case` *(yeni)* | owned db table | Reconciliation | Eşleşme case'i — bir Card satırı + sıfır/bir Clearing satırı birleşimi | FileLine pair | Case kaydı | Fark takibi |
| 17 | `reconciliation_match` *(yeni)* | owned db table | Reconciliation | Başarılı eşleşme detayı | Case | Match kaydı | Clean match izleme |
| 18 | `reconciliation_mismatch` *(yeni)* | owned db table | Reconciliation | Uyumsuzluk detayı (alan, beklenen vs gerçek) | Case | Mismatch kaydı | Fark analizi |
| 19 | `reconciliation_financial_impact` *(yeni)* | owned db table | Reconciliation | Finansal etki hesabı (tutar farkı, para birimi) | Case/Mismatch | FI kaydı | Mali risk izleme |
| 20 | `reconciliation_balance_adjustment` *(yeni)* | owned db table | Reconciliation | Gölge bakiye düzeltme kaydı | Action execution | Adjustment kaydı | Gölge bakiye takibi |
| 21 | `reconciliation_audit` *(yeni)* | owned audit/log | Reconciliation | Tüm reconciliation durum geçiş logu | Her durum değişimi | Audit trail | Denetim izi |
| 22 | `archive_log` | owned db table | Archive | Arşiv çalıştırma kaydı | Dosya filtresi | Status/Message | Arşiv başarı izleme |
| 23 | `archive_ingestion_file` | owned db table | Archive | Arşivlenmiş dosya kopyası | IngestionFile | Archive kopya | Denetim izi |
| 24 | `archive_ingestion_file_line` | owned db table | Archive | Arşivlenmiş satır kopyası | IngestionFileLine | Archive kopya | Denetim izi |
| 25 | `archive_candidate` *(yeni)* | owned db table | Archive | Archive uygunluk değerlendirmesi | Dosya/tarih filtresi | Aday kaydı | Backlog takibi |
| 26 | `archive_snapshot` *(yeni)* | owned db table | Archive | Arşiv anı snapshot (count, hash) | Archive run | Snapshot kaydı | Bütünlük doğrulama |
| 27 | `archive_error_log` *(yeni)* | owned audit/log | Archive | Arşiv hata detayı | Archive run | Error log | Hata takibi |
| 28 | File Ingestion Job | owned batch/job | Ingestion | Dosya tarama → parse → validate → persist | Cron tetik | IngestionFile/Line | SLA |
| 29 | Reconciliation Job | owned batch/job | Reconciliation | Case açma → match → action → review | Ingestion tamamlanma | Evaluation/Operation | SLA |
| 30 | Archive Job | owned batch/job | Archive | Aday belirleme → taşıma → snapshot | Cron tetik | ArchiveLog | SLA |

---

## 3. Canonical Owned Data Model

### A. Ingestion Tablo Ailesi

#### 3.A.1 `ingestion_file`

> **Mevcut entity:** `IngestionFile`

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | Tekil dosya kimliği | Dosya alındı | Tüm raporlar |
| file_key | varchar(256) | — | ✗ | Dosya benzersiz anahtarı (path+name+date hash) | Dosya alındı | R1, R2 |
| file_name | varchar(512) | — | ✗ | Orijinal dosya adı | Dosya alındı | R1, R2 |
| full_path | varchar(1024) | — | ✗ | Tam dosya yolu | Dosya alındı | R1 |
| source_type | smallint | — | ✗ | Remote=1, Local=2 | Dosya alındı | R1 |
| file_type | smallint | — | ✗ | Card=1, Clearing=2 | Dosya alındı | R1, R2, R6 |
| content_type | smallint | — | ✗ | Bkm=1, Msc=2, Visa=3 | Dosya alındı | R1, R2, R6 |
| status | smallint | — | ✗ | Processing=1, Failed=2, Success=3 | Her adımda güncellenir | R1, R2, R3 |
| message | text | — | ✓ | Hata/bilgi mesajı | Hata anında | R3 |
| expected_count | bigint | — | ✗ | Footer'dan okunan beklenen satır sayısı | Header/footer kontrolü | R2 |
| total_count | bigint | — | ✗ | Toplam işlenen satır | Parse tamamlandığında | R1, R2 |
| success_count | bigint | — | ✗ | Başarılı satır | Parse/validate tamamlandığında | R1 |
| error_count | bigint | — | ✗ | Hatalı satır | Parse/validate tamamlandığında | R1, R3 |
| last_processed_line_number | bigint | — | ✗ | Resumable processing için | İşlem sırasında | — |
| last_processed_byte_offset | bigint | — | ✗ | Resumable processing için | İşlem sırasında | — |
| is_archived | bool | — | ✗ | Arşivlendi mi | Archive sonrası | R12, R13 |
| created_at | timestamptz | — | ✗ | Kayıt oluşturma zamanı (AuditEntity) | Dosya alındı | R1 |
| updated_at | timestamptz | — | ✓ | Son güncelleme | Her adımda | — |

**Index önerileri:**
- `IX_ingestion_file_file_key` UNIQUE — duplicate file kontrolü
- `IX_ingestion_file_status_created` (status, created_at DESC) — operasyon dashboard
- `IX_ingestion_file_type_content_date` (file_type, content_type, created_at) — rapor filtreleri

**Partition:** `created_at` bazlı aylık range partition önerilir (yüksek hacim senaryosunda).

---

#### 3.A.2 `ingestion_file_line`

> **Mevcut entity:** `IngestionFileLine`

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | Tekil satır kimliği | Parse | Tüm satır bazlı raporlar |
| ingestion_file_id | uuid | FK→ingestion_file | ✗ | Ait olduğu dosya | Parse | R1, R4, R5 |
| line_number | bigint | — | ✗ | Dosyadaki satır numarası | Parse | R3 |
| byte_offset | bigint | — | ✗ | Dosyadaki byte konumu | Parse | — |
| byte_length | int | — | ✗ | Satır byte uzunluğu | Parse | — |
| record_type | varchar(16) | — | ✗ | H/D/F | Parse | R3 |
| raw_data | text | — | ✗ | Ham satır verisi | Parse | — |
| parsed_data | jsonb | — | ✗ | Parse edilmiş JSON | Parse | — |
| status | smallint | — | ✗ | Processing=1, Failed=2, Success=3 | Her adımda | R1, R3 |
| message | text | — | ✓ | Hata/bilgi | Hata anında | R3 |
| retry_count | int | — | ✗ | Yeniden deneme sayısı | Retry | — |
| correlation_key | varchar(64) | — | ✗ | Eşleşme alan adı (ör: "OceanTxnGuid") | Business key üretimi | R5 |
| correlation_value | varchar(256) | — | ✗ | Eşleşme alan değeri | Business key üretimi | R5, R6 |
| duplicate_detection_key | varchar(512) | — | ✗ | Duplicate kontrol composite key | Duplicate kontrolü | R4 |
| duplicate_status | varchar(16) | — | ✗ | Unique/Primary/Secondary/Conflict | Duplicate kontrolü | R4 |
| duplicate_group_id | uuid | — | ✓ | Duplicate grup kimliği | Duplicate kontrolü | R4 |
| reconciliation_status | smallint | — | ✓ | Ready=1, Failed=2, Success=3, Processing=4 | Reconciliation sonrası | R6, R7 |
| matched_clearing_line_id | uuid | FK→self | ✓ | Eşleşen clearing satır ID | Reconciliation match | R6 |
| created_at | timestamptz | — | ✗ | Kayıt zamanı | Parse | — |
| updated_at | timestamptz | — | ✓ | Son güncelleme | Her adımda | — |

**Index önerileri:**
- `IX_file_line_file_id` (ingestion_file_id)
- `IX_file_line_dup_key` (duplicate_detection_key) — duplicate kontrolü
- `IX_file_line_correlation` (correlation_key, correlation_value) — recon eşleşme
- `IX_file_line_recon_status` (reconciliation_status) WHERE reconciliation_status IS NOT NULL
- `IX_file_line_matched_clearing` (matched_clearing_line_id) WHERE matched_clearing_line_id IS NOT NULL

**Partition:** `created_at` bazlı aylık range partition; hacim yüksekse `ingestion_file_id` hash partition da düşünülebilir.

---

#### 3.A.3 `ingestion_file_control` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | Kontrol kaydı ID | Header/footer kontrolü | R2 |
| ingestion_file_id | uuid | FK→ingestion_file | ✗ | Dosya referansı | Header/footer kontrolü | R2 |
| header_file_date | varchar(16) | — | ✓ | Header'daki dosya tarihi | Parse | R2 |
| header_file_no | bigint | — | ✓ | Header'daki dosya numarası | Parse | R2 |
| footer_expected_count | bigint | — | ✗ | Footer'daki beklenen satır sayısı | Parse | R2 |
| actual_detail_count | bigint | — | ✗ | Gerçekte okunan detail satır sayısı | Parse sonrası | R2 |
| count_match | bool | — | ✗ | footer == actual? | Kontrol sonrası | R2 |
| file_sequence_no | bigint | — | ✓ | Dosya sıra numarası (kaynak meta) | Parse | R2 — sequence anomaly |
| previous_file_sequence_no | bigint | — | ✓ | Bir önceki dosya sıra no | Kontrol sonrası | R2 |
| sequence_gap_detected | bool | — | ✗ | Boşluk var mı | Kontrol sonrası | Alarm |
| control_result | varchar(32) | — | ✗ | PASS / FAIL / WARNING | Kontrol sonrası | R2 |
| message | text | — | ✓ | Detay | Kontrol sonrası | R2 |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_file_control_file_id` UNIQUE (ingestion_file_id)

---

#### 3.A.4 `ingestion_parse_error` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | Hata ID | Parse | R3 |
| ingestion_file_id | uuid | FK→ingestion_file | ✗ | Dosya | Parse | R3 |
| ingestion_file_line_id | uuid | FK→ingestion_file_line | ✓ | Satır (satır oluşturulabilmişse) | Parse | R3 |
| line_number | bigint | — | ✗ | Hatalı satır no | Parse | R3 |
| error_code | varchar(32) | — | ✗ | PARSE_FIXED_WIDTH / PARSE_JSON / ENCODING vb. | Parse | R3 |
| error_message | text | — | ✗ | Detay | Parse | R3 |
| raw_fragment | text | — | ✓ | Hatalı kısım (max 1000 char) | Parse | R3 |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_parse_error_file_id` (ingestion_file_id) — spike analizi için

---

#### 3.A.5 `ingestion_validation_error` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | Hata ID | Validation | R3 |
| ingestion_file_id | uuid | FK→ingestion_file | ✗ | Dosya | Validation | R3 |
| ingestion_file_line_id | uuid | FK→ingestion_file_line | ✗ | Satır | Validation | R3 |
| rule_code | varchar(32) | — | ✗ | REQUIRED_FIELD / FORMAT / CONSISTENCY vb. | Validation | R3 |
| field_name | varchar(64) | — | ✗ | Hatalı alan adı | Validation | R3 |
| expected_value | varchar(256) | — | ✓ | Beklenen | Validation | R3 |
| actual_value | varchar(256) | — | ✓ | Gerçek | Validation | R3 |
| severity | varchar(16) | — | ✗ | BLOCKING / WARNING | Validation | R3 |
| error_message | text | — | ✗ | Detay | Validation | R3 |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_validation_error_file_line` (ingestion_file_line_id)

---

#### 3.A.6 `ingestion_duplicate_log` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Duplicate kontrolü | R4 |
| ingestion_file_line_id | uuid | FK→ingestion_file_line | ✗ | Kontrol edilen satır | Duplicate kontrolü | R4 |
| duplicate_detection_key | varchar(512) | — | ✗ | Composite key | Duplicate kontrolü | R4 |
| duplicate_type | varchar(32) | — | ✗ | TECHNICAL / BUSINESS | Duplicate kontrolü | R4 |
| existing_line_id | uuid | FK→ingestion_file_line | ✓ | Önceden var olan satır | Duplicate kontrolü | R4 |
| decision | varchar(16) | — | ✗ | Unique/Primary/Secondary/Conflict | Duplicate kontrolü | R4 |
| duplicate_group_id | uuid | — | ✗ | Grup ID | Duplicate kontrolü | R4 |
| reason | text | — | ✓ | Karar nedeni | Duplicate kontrolü | R4 |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_dup_log_detection_key` (duplicate_detection_key)

---

#### 3.A.7 `ingestion_business_key` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Business key üretimi | R5 |
| ingestion_file_line_id | uuid | FK→ingestion_file_line | ✗ | Satır | BK üretimi | R5 |
| key_type | varchar(32) | — | ✗ | CARD_TXN / CLEARING_TXN | BK üretimi | R5 |
| business_key | varchar(512) | — | ✗ | Üretilmiş canonical key | BK üretimi | R5, R6 |
| key_components_json | jsonb | — | ✗ | Key oluşturan alanlar | BK üretimi | R5 |
| ocean_txn_guid | bigint | — | ✗ | External ref — eşleşme için | BK üretimi | R5, R6 |
| is_ocean_txn_guid_zero | bool | — | ✗ | Zero/missing flag | BK üretimi | R5, Alarm |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:**
- `IX_bk_business_key` UNIQUE (business_key) — uniqueness guarantee
- `IX_bk_ocean_txn_guid` (ocean_txn_guid) — recon join
- `IX_bk_line_id` (ingestion_file_line_id)

---

#### 3.A.8 `ingestion_status_history` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Her durum değişimi | — |
| entity_type | varchar(32) | — | ✗ | FILE / LINE | Her değişim | — |
| entity_id | uuid | — | ✗ | IngestionFile veya Line ID | Her değişim | — |
| from_status | varchar(32) | — | ✓ | Önceki durum | Her değişim | — |
| to_status | varchar(32) | — | ✗ | Yeni durum | Her değişim | — |
| changed_by | varchar(128) | — | ✗ | Sistem/kullanıcı | Her değişim | — |
| reason | text | — | ✓ | Neden | Her değişim | — |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_status_hist_entity` (entity_type, entity_id, created_at)

---

### B. Reconciliation Tablo Ailesi

#### 3.B.1 `reconciliation_case` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | Case ID | Case açılması | R6-R11 |
| card_file_line_id | uuid | FK→ingestion_file_line | ✗ | Card Transaction satırı | Case açılması | R6 |
| clearing_file_line_id | uuid | FK→ingestion_file_line | ✓ | Clearing satırı (NULL = unmatched) | Match sonrası | R6, R7 |
| card_business_key_id | uuid | FK→ingestion_business_key | ✗ | Card tarafı business key | Case açılması | R6 |
| clearing_business_key_id | uuid | FK→ingestion_business_key | ✓ | Clearing tarafı business key | Match sonrası | R6 |
| ocean_txn_guid | bigint | — | ✗ | Eşleşme referansı | Case açılması | R6, R7 |
| case_type | varchar(32) | — | ✗ | CARD_TO_CLEARING / CLEARING_ORPHAN | Case açılması | R6 |
| case_status | varchar(32) | — | ✗ | OPEN / MATCHED / MISMATCHED / RESOLVED / CLOSED | Her adımda | R6, R7 |
| match_result | varchar(32) | — | ✓ | FULL_MATCH / PARTIAL_MATCH / NO_MATCH / PENDING | Match sonrası | R6 |
| mismatch_count | int | — | ✗ | Uyumsuzluk sayısı | Match sonrası | R7 |
| action_count | int | — | ✗ | Üretilen aksiyon sayısı | Action üretimi sonrası | R8 |
| file_date | date | — | ✗ | İşlem dosya tarihi | Case açılması | R6, R7 |
| content_type | smallint | — | ✗ | Bkm=1, Msc=2, Visa=3 | Case açılması | R6 |
| opened_at | timestamptz | — | ✗ | Case açılış zamanı | Case açılması | R7 — aging |
| closed_at | timestamptz | — | ✓ | Case kapanış zamanı | Resolved/Closed | R7 — aging |
| created_at | timestamptz | — | ✗ | — | — | — |
| updated_at | timestamptz | — | ✓ | — | — | — |

**Index:**
- `IX_case_ocean_txn_guid` (ocean_txn_guid)
- `IX_case_status_date` (case_status, file_date)
- `IX_case_card_line` (card_file_line_id)

**Partition:** `file_date` bazlı aylık range partition.

---

#### 3.B.2 `reconciliation_match` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Match | R6 |
| case_id | uuid | FK→reconciliation_case | ✗ | Case | Match | R6 |
| match_type | varchar(32) | — | ✗ | EXACT / FUZZY / MANUAL | Match | R6 |
| match_score | decimal(5,2) | — | ✓ | Eşleşme skoru (0-100) | Match | R6 |
| matched_fields_json | jsonb | — | ✗ | Hangi alanlar eşleşti | Match | R6 |
| card_amount | decimal(18,4) | — | ✗ | Card tarafı tutar | Match | R6, R10 |
| clearing_amount | decimal(18,4) | — | ✗ | Clearing tarafı tutar | Match | R6, R10 |
| amount_difference | decimal(18,4) | — | ✗ | Fark | Match | R10 |
| currency | int | — | ✗ | Para birimi | Match | R10 |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_match_case_id` UNIQUE (case_id)

---

#### 3.B.3 `reconciliation_mismatch` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Match değerlendirmesi | R7 |
| case_id | uuid | FK→reconciliation_case | ✗ | Case | Match değerlendirmesi | R7 |
| field_name | varchar(64) | — | ✗ | Uyumsuz alan | Match değerlendirmesi | R7 |
| card_value | varchar(512) | — | ✓ | Card tarafı değer | Match değerlendirmesi | R7 |
| clearing_value | varchar(512) | — | ✓ | Clearing tarafı değer | Match değerlendirmesi | R7 |
| severity | varchar(16) | — | ✗ | CRITICAL / HIGH / MEDIUM / LOW | Match değerlendirmesi | R7 |
| mismatch_type | varchar(32) | — | ✗ | AMOUNT / STATUS / DATE / MISSING_COUNTERPART | Match değerlendirmesi | R7 |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_mismatch_case_id` (case_id)

---

#### 3.B.4 `reconciliation_action` *(yeni — mevcut `reconciliation_operation` ile birleştirilebilir)*

> **Varsayım:** Mevcut `reconciliation_operation` tablosu bu rolü zaten karşılıyor. Bu modelde `reconciliation_operation` tablosunu canonical `reconciliation_action` olarak kullanıyoruz ve yeni alanlar ekliyoruz.

Mevcut `reconciliation_operation` tablosuna eklenmesi önerilen alanlar:

| Kolon | Veri Tipi | PK/FK | Null | Açıklama |
|-------|-----------|-------|------|----------|
| case_id | uuid | FK→reconciliation_case | ✗ | Bağlı case |
| action_code | varchar(8) | — | ✗ | D1-D8 veya diğer kodlar |
| financial_impact_amount | decimal(18,4) | — | ✓ | Finansal etki tutarı |
| financial_impact_currency | int | — | ✓ | Para birimi |
| shadow_balance_effect | decimal(18,4) | — | ✓ | Gölge bakiye etkisi |
| requires_manual_review | bool | — | ✗ | Manuel review zorunlu mu |
| closure_condition | varchar(128) | — | ✓ | Kapanış koşulu açıklaması |

---

#### 3.B.5 `reconciliation_action_execution` → Mevcut `reconciliation_operation_execution`

Mevcut tablo yeterli. Ek alan önerisi:

| Kolon | Veri Tipi | PK/FK | Null | Açıklama |
|-------|-----------|-------|------|----------|
| case_id | uuid | FK→reconciliation_case | ✗ | Case referansı (denormalize — hız) |

---

#### 3.B.6 `reconciliation_manual_review` → Mevcut `reconciliation_review`

Mevcut tablo yeterli. Ek alan önerisi:

| Kolon | Veri Tipi | PK/FK | Null | Açıklama |
|-------|-----------|-------|------|----------|
| case_id | uuid | FK→reconciliation_case | ✗ | Case referansı |
| sla_deadline | timestamptz | — | ✓ | SLA son tarihi |
| is_sla_breached | bool | — | ✗ | SLA ihlali oldu mu |

---

#### 3.B.7 `reconciliation_alert` → Mevcut entity

Mevcut tablo yeterli. Ek alan önerisi:

| Kolon | Veri Tipi | PK/FK | Null | Açıklama |
|-------|-----------|-------|------|----------|
| case_id | uuid | FK→reconciliation_case | ✓ | Case referansı (bazı alarmlar case dışı olabilir) |
| alert_code | varchar(32) | — | ✗ | Alarm kataloğu kodu |
| closed_at | timestamptz | — | ✓ | Kapanış zamanı |
| closed_by | varchar(128) | — | ✓ | Kapatan |

---

#### 3.B.8 `reconciliation_financial_impact` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Finansal etki hesabı | R10 |
| case_id | uuid | FK→reconciliation_case | ✗ | Case | Finansal etki hesabı | R10 |
| impact_type | varchar(32) | — | ✗ | AMOUNT_DIFF / MISSING_TXN / CURRENCY_DIFF / FEE_DIFF | Hesaplama | R10 |
| card_amount | decimal(18,4) | — | ✗ | Card tarafı tutar | Hesaplama | R10 |
| clearing_amount | decimal(18,4) | — | ✗ | Clearing tarafı tutar | Hesaplama | R10 |
| difference_amount | decimal(18,4) | — | ✗ | Fark | Hesaplama | R10 |
| currency | int | — | ✗ | ISO para birimi | Hesaplama | R10 |
| direction | varchar(8) | — | ✗ | DEBIT / CREDIT | Hesaplama | R10 |
| is_resolved | bool | — | ✗ | Çözüldü mü | Action sonrası | R10 |
| resolved_at | timestamptz | — | ✓ | Çözüm zamanı | Action sonrası | R10 |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_fi_case_id` (case_id), `IX_fi_is_resolved` (is_resolved, created_at)

---

#### 3.B.9 `reconciliation_balance_adjustment` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Gölge bakiye düzeltme | R10 |
| case_id | uuid | FK→reconciliation_case | ✗ | Case | Execution sonrası | R10 |
| operation_id | uuid | FK→reconciliation_operation | ✗ | Hangi action tetikledi | Execution sonrası | R10 |
| execution_id | uuid | FK→reconciliation_operation_execution | ✗ | Hangi execution | Execution sonrası | R10 |
| adjustment_type | varchar(32) | — | ✗ | SHADOW_CREDIT / SHADOW_DEBIT / REVERSAL | Execution sonrası | R10 |
| amount | decimal(18,4) | — | ✗ | Tutar | Execution sonrası | R10 |
| currency | int | — | ✗ | Para birimi | Execution sonrası | R10 |
| running_shadow_balance | decimal(18,4) | — | ✗ | Düzeltme sonrası toplam gölge bakiye | Execution sonrası | R10 |
| reason | text | — | ✓ | Neden | Execution sonrası | R10 |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_ba_case_id` (case_id), `IX_ba_created` (created_at DESC)

---

#### 3.B.10 `reconciliation_audit` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Her durum değişimi | — |
| entity_type | varchar(32) | — | ✗ | CASE / EVALUATION / OPERATION / REVIEW / ALERT | — | — |
| entity_id | uuid | — | ✗ | Referans ID | — | — |
| case_id | uuid | — | ✓ | Case referansı | — | — |
| action | varchar(32) | — | ✗ | CREATED / STATUS_CHANGED / UPDATED / DELETED | — | — |
| from_value | varchar(128) | — | ✓ | Önceki değer | — | — |
| to_value | varchar(128) | — | ✓ | Yeni değer | — | — |
| changed_by | varchar(128) | — | ✗ | Sistem/kullanıcı | — | — |
| metadata_json | jsonb | — | ✓ | Ek bilgi | — | — |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_recon_audit_entity` (entity_type, entity_id, created_at)

---

### C. Archive Tablo Ailesi

#### 3.C.1 `archive_candidate` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Aday belirleme | R12 |
| ingestion_file_id | uuid | FK→ingestion_file | ✗ | Dosya | Aday belirleme | R12 |
| file_date | date | — | ✗ | Dosya tarihi | Aday belirleme | R12 |
| reconciliation_completed | bool | — | ✗ | Recon tamamlandı mı | Aday belirleme | R12 |
| retention_days | int | — | ✗ | Tutma süresi (gün) | Aday belirleme | R12 |
| eligible_after | date | — | ✗ | Bu tarihten sonra arşivlenebilir | Aday belirleme | R12 |
| is_eligible | bool | — | ✗ | Şu an uygun mu | Her kontrol | R12 |
| eligibility_reason | varchar(128) | — | ✓ | Neden uygun/değil | Her kontrol | R12 |
| archive_run_id | uuid | — | ✓ | Hangi run ile arşivlendi | Archive sonrası | R13 |
| status | varchar(32) | — | ✗ | PENDING / ARCHIVED / FAILED / SKIPPED | Her adımda | R12, R13 |
| created_at | timestamptz | — | ✗ | — | — | — |
| updated_at | timestamptz | — | ✓ | — | — | — |

**Index:** `IX_candidate_eligible` (is_eligible, eligible_after) WHERE status = 'PENDING'

---

#### 3.C.2 `archive_run` → Mevcut `archive_log` genişletilmiş

Mevcut `ArchiveLog` tablosuna eklenmesi önerilen alanlar:

| Kolon | Veri Tipi | PK/FK | Null | Açıklama |
|-------|-----------|-------|------|----------|
| run_type | varchar(32) | — | ✗ | SCHEDULED / MANUAL |
| started_at | timestamptz | — | ✗ | Başlangıç |
| finished_at | timestamptz | — | ✓ | Bitiş |
| total_candidates | int | — | ✗ | Toplam aday |
| success_count | int | — | ✗ | Başarılı |
| error_count | int | — | ✗ | Hatalı |
| skip_count | int | — | ✗ | Atlanan |

---

#### 3.C.3 `archive_run_item` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Archive çalışma | R13 |
| archive_run_id | uuid | FK→archive_log | ✗ | Run | Archive çalışma | R13 |
| archive_candidate_id | uuid | FK→archive_candidate | ✗ | Aday | Archive çalışma | R13 |
| ingestion_file_id | uuid | — | ✗ | Dosya | Archive çalışma | R13 |
| lines_archived | bigint | — | ✗ | Arşivlenen satır sayısı | Archive sonrası | R13 |
| status | varchar(32) | — | ✗ | SUCCESS / FAILED / SKIPPED | Archive sonrası | R13 |
| error_message | text | — | ✓ | Hata detayı | Hata anında | R13 |
| started_at | timestamptz | — | ✗ | Başlangıç | Archive çalışma | R13 |
| finished_at | timestamptz | — | ✓ | Bitiş | Archive sonrası | R13 |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_run_item_run_id` (archive_run_id)

---

#### 3.C.4 `archive_snapshot` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Archive sonrası | R13 |
| archive_run_item_id | uuid | FK→archive_run_item | ✗ | Run item | Archive sonrası | R13 |
| ingestion_file_id | uuid | — | ✗ | Dosya | Archive sonrası | R13 |
| source_line_count | bigint | — | ✗ | Kaynak satır sayısı | Archive anında | R13 |
| archived_line_count | bigint | — | ✗ | Arşivlenen satır sayısı | Archive anında | R13 |
| source_checksum | varchar(128) | — | ✗ | Kaynak veri hash | Archive anında | R13 |
| archive_checksum | varchar(128) | — | ✗ | Arşiv veri hash | Archive anında | R13 |
| checksum_match | bool | — | ✗ | Hash eşleşiyor mu | Doğrulama | R13 |
| snapshot_at | timestamptz | — | ✗ | Snapshot zamanı | Archive sonrası | R13 |
| created_at | timestamptz | — | ✗ | — | — | — |

**Index:** `IX_snapshot_run_item` UNIQUE (archive_run_item_id)

---

#### 3.C.5 `archive_error_log` *(yeni tablo)*

| Kolon | Veri Tipi | PK/FK | Null | Açıklama | Süreç Adımı | Raporlar |
|-------|-----------|-------|------|----------|-------------|----------|
| id | uuid | PK | ✗ | — | Hata anında | R13 |
| archive_run_id | uuid | FK→archive_log | ✗ | Run | Hata anında | R13 |
| archive_run_item_id | uuid | FK→archive_run_item | ✓ | Run item | Hata anında | R13 |
| error_code | varchar(32) | — | ✗ | COPY_FAILED / CHECKSUM_MISMATCH vb. | Hata anında | R13 |
| error_message | text | — | ✗ | Detay | Hata anında | R13 |
| stack_trace | text | — | ✓ | Stack trace | Hata anında | — |
| created_at | timestamptz | — | ✗ | — | — | — |

---

## 4. External Reference Fields Strategy

| Alan | Kaynak Anlamı | Owned Tablo | Raw | Normalized | İkisi de | History | Unique Key İçinde | Reconciliation Key Kullanımı | Operasyon Yorumu |
|------|--------------|-------------|-----|------------|----------|---------|-------------------|------------------------------|------------------|
| **OceanTxnGuid** | Ocean processor'ın işlem benzersiz ID'si | `ingestion_file_line` (correlation_value), `ingestion_business_key` (ocean_txn_guid), `reconciliation_case` (ocean_txn_guid) | ✓ (raw_data içinde) | ✓ (bigint olarak) | İkisi de | ✗ (tek kayıtta değişmez) | ✓ — business_key bileşeni | **Birincil eşleşme anahtarı.** Card → Clearing eşleşmesi bu alan üzerinden yapılır. | Sıfır gelirse = online'da bulunamayan/problemli kayıt. Alarm tetikler. |
| **OceanMainTxnGuid** | Ana işlem GUID'i (taksit, partial vb.) | `ingestion_card_*_detail` (kolon olarak) | ✓ | ✓ (bigint) | İkisi de | ✗ | ✗ | İkincil eşleşme / parent-child ilişki tespiti. | Taksitli işlemlerde ana işlemi bulmak için kullanılır. |
| **RRN** | Retrieval Reference Number — network referansı | `ingestion_file_line` (parsed_data içinde), `ingestion_card_*_detail`, `ingestion_clearing_*_detail` | ✓ | ✓ (varchar, trim) | İkisi de | ✗ | ✗ | OceanTxnGuid sıfır olduğunda yedek eşleşme anahtarı. | Dispute/chargeback araştırmalarında temel izleme alanı. |
| **ARN** | Acquirer Reference Number | `ingestion_card_*_detail`, `ingestion_clearing_*_detail` | ✓ | ✓ (varchar, trim) | İkisi de | ✗ | ✗ | OceanTxnGuid+RRN başarısız olduğunda üçüncü fallback. | Network bazlı araştırma ve dispute izleme. |
| **ProvisionCode** | Otorisasyon kodu | `ingestion_card_*_detail`, `ingestion_clearing_*_detail` | ✓ | ✓ | İkisi de | ✗ | ✗ | Doğrulama amaçlı — match confirmation. | İşlemin provizyon alıp almadığını doğrular. |
| **Stan** | System Trace Audit Number | `ingestion_card_*_detail` | ✓ | ✓ (int) | İkisi de | ✗ | ✗ | Destekleyici — duplicate detection bileşeni olabilir. | Aynı gün içinde tekil olması beklenir. |
| **ClrNo** | Clearing numarası | `ingestion_clearing_*_detail` | ✓ | ✓ (bigint) | İkisi de | ✗ | ✗ | Clearing tarafı unique referans. | Clearing dosyası içi tekil izleme. |
| **ControlStat** | Clearing kaydı kontrol durumu (N/P/D/X/Y) | `ingestion_clearing_*_detail` | ✓ | ✓ (enum → varchar) | İkisi de | **✓** — aynı kayıt farklı günlerde farklı statüyle gelebilir | ✗ | Eşleşme sonucu değerlendirmede kullanılır: P→N (ProblemToNormal=X) geçişi reconciliation case'i günceller. | Normal=sorun yok, Problem=incelenmeli, ChargebackClosing=dispute kapandı, ProblemToNormal=düzeldi, DisputeEnd=ihtilaf sona erdi. |
| **ResponseCode** | İşlem yanıt kodu | `ingestion_card_*_detail` | ✓ | ✓ (varchar) | İkisi de | ✗ | ✗ | Mismatch değerlendirmede destekleyici. | "00" = başarılı. Diğer kodlar başarısız sebebini gösterir. |
| **TxnStat** | İşlem durumu (N/R/V/E) | `ingestion_card_*_detail` | ✓ | ✓ (enum) | İkisi de | ✗ | ✗ | **IsSuccessfulTxn ile birlikte** operasyonel yorum için kullanılır. Reverse/Void/Expired işlemler reconciliation'da özel ele alınır. | N=Normal, R=Reverse (geri alınmış), V=Void (iptal), E=Expired (süresi dolmuş). |
| **IsSuccessfulTxn** | İşlem başarılı mı (Y/N) | `ingestion_card_*_detail` | ✓ | ✓ (enum) | İkisi de | ✗ | ✗ | **TxnStat ile birlikte** değerlendirilir. IsSuccessfulTxn=Y + TxnStat=N → gerçek başarılı işlem. | Y=başarılı finansal işlem, N=başarısız (decline vb.) — Card Transaction dosyası ikisini de içerir. |
| **TxnSettle** | Finansallaşma durumu (Y/N) | `ingestion_card_*_detail` | ✓ | ✓ (enum) | İkisi de | ✗ | ✗ | Sadece settle=Y olanlar clearing ile eşleşmesi beklenir. | Y=finansallaşmış, N=henüz finansallaşmamış. |
| **file sequence no** | Dosya sıra numarası (header'daki FileNo) | `ingestion_file_control` (file_sequence_no) | ✓ | ✓ (bigint) | İkisi de | ✗ | ✗ | Sequence gap tespiti. | Eksik dosya alarmı için sıra kontrolü. |
| **source network file metadata** | Dosya adı, tarih, versiyon (header bilgileri) | `ingestion_file_control` (header_file_date, header_file_no) | ✓ | ✓ | İkisi de | ✗ | ✗ | Dosya bütünlük kontrolü. | Dosyanın beklenen kaynaktan geldiğini doğrular. |

---

## 5. Süreç Akışı ve Durum Modeli

### 5.A Ingestion Status Flow

```
                    ┌─────────────┐
                    │  FILE_RECEIVED  │
                    └──────┬──────┘
                           ▼
                    ┌─────────────┐
              ┌─────│ CONTROL_CHECK │─────┐
              │     └──────┬──────┘      │
              │ fail       │ pass        │ sequence_gap
              ▼            ▼             ▼
     ┌──────────┐  ┌──────────┐  ┌─────────────┐
     │  FAILED  │  │  PARSING │  │CONTROL_WARNING│
     └──────────┘  └────┬─────┘  └──────┬───────┘
                        │               │
                        ▼               ▼
                 ┌──────────────┐
                 │  VALIDATING  │
                 └──────┬───────┘
                        │
              ┌─────────┴──────────┐
              ▼                    ▼
     ┌────────────────┐  ┌──────────────────┐
     │ DUPLICATE_CHECK │  │VALIDATION_FAILED │ → FAILED (terminal)
     └───────┬────────┘  └──────────────────┘
             │
             ▼
     ┌───────────────────┐
     │ BUSINESS_KEY_GEN  │
     └───────┬───────────┘
             │
             ▼
     ┌───────────────────┐
     │  READY_FOR_RECON  │ → (Recon süreci tetiklenir)
     └───────────────────┘

     Terminal durumlar: FAILED, READY_FOR_RECON
```

**File-level state machine:**

| Durum | Açıklama | Sonraki Durumlar | Hata Durumları | Terminal |
|-------|----------|-----------------|----------------|----------|
| Processing | Dosya işleniyor | Success, Failed | Failed | ✗ |
| Success | Dosya başarıyla işlendi | — | — | ✓ |
| Failed | Dosya işlenemedi | — | — | ✓ |

**Line-level state machine:**

| Durum | Açıklama | Sonraki Durumlar | Hata Durumları | Terminal |
|-------|----------|-----------------|----------------|----------|
| Processing | Satır işleniyor | Success, Failed | Failed | ✗ |
| Success | Satır başarılı | — | — | ✓ |
| Failed | Satır hatalı | — | — | ✓ |

**ReconciliationStatus (satır üzerinde):**

| Durum | Açıklama | Sonraki Durumlar | Hata Durumları | Terminal |
|-------|----------|-----------------|----------------|----------|
| Ready | Recon'a hazır | Processing | — | ✗ |
| Processing | Recon işleniyor | Success, Failed | Failed | ✗ |
| Success | Recon tamamlandı | — | — | ✓ |
| Failed | Recon başarısız | Ready (retry) | — | ✓* (retry ile çıkılabilir) |

### 5.B Reconciliation Status Flow

**Case state machine:**

| Durum | Açıklama | Sonraki Durumlar | Hata Durumları | Terminal |
|-------|----------|-----------------|----------------|----------|
| OPEN | Case açıldı, eşleşme bekliyor | MATCHED, MISMATCHED | — | ✗ |
| MATCHED | Tam eşleşme bulundu | CLOSED | — | ✗ |
| MISMATCHED | Fark tespit edildi | RESOLVED | — | ✗ |
| RESOLVED | Tüm aksiyonlar tamamlandı | CLOSED | — | ✗ |
| CLOSED | Case kapatıldı | — | — | ✓ |

**Evaluation state machine (mevcut):**

| Durum | Açıklama | Sonraki Durumlar | Hata Durumları | Terminal |
|-------|----------|-----------------|----------------|----------|
| Pending | Evaluation bekliyor | Evaluating | — | ✗ |
| Evaluating | Evaluation çalışıyor | Planned, Completed | Failed | ✗ |
| Planned | Operasyonlar planlandı | Completed | Failed | ✗ |
| Failed | Evaluation başarısız | Pending (retry) | — | ✓* |
| Completed | Tamamlandı | — | — | ✓ |

**Operation state machine (mevcut):**

| Durum | Açıklama | Sonraki Durumlar | Hata Durumları | Terminal |
|-------|----------|-----------------|----------------|----------|
| Planned | Planlandı | Blocked, Executing | — | ✗ |
| Blocked | Manuel review veya bağımlılık bekliyor | Executing, Cancelled | — | ✗ |
| Executing | Çalışıyor | Completed | Failed | ✗ |
| Completed | Başarılı | — | — | ✓ |
| Failed | Başarısız | Planned (retry) | — | ✓* |
| Cancelled | İptal edildi | — | — | ✓ |

**Review state machine (mevcut):**

| Durum | Açıklama | Sonraki Durumlar | Terminal |
|-------|----------|-----------------|----------|
| Pending | Onay bekliyor | Approved, Rejected, Cancelled | ✗ |
| Approved | Onaylandı | — | ✓ |
| Rejected | Reddedildi | — | ✓ |
| Cancelled | İptal/expire | — | ✓ |

### 5.C Archive Status Flow

| Durum | Açıklama | Sonraki Durumlar | Hata Durumları | Terminal |
|-------|----------|-----------------|----------------|----------|
| CANDIDATE_IDENTIFIED | Aday belirlendi | ELIGIBLE, NOT_ELIGIBLE | — | ✗ |
| NOT_ELIGIBLE | Henüz uygun değil (retention dolmadı veya recon tamamlanmadı) | ELIGIBLE (sonraki kontrol) | — | ✗ |
| ELIGIBLE | Arşive uygun | ARCHIVING | — | ✗ |
| ARCHIVING | Arşivleniyor | ARCHIVED, ARCHIVE_FAILED | — | ✗ |
| ARCHIVED | Başarıyla arşivlendi | — | — | ✓ |
| ARCHIVE_FAILED | Arşiv hatası | ELIGIBLE (retry) | — | ✓* |
| SKIPPED | Atlandı (manuel karar) | — | — | ✓ |

---

## 6. Reconciliation Key ve Matching Strategy

### 6.1 ingestion_file_line → reconciliation_case

| Başlık | Açıklama |
|--------|----------|
| **PK mantığı** | `reconciliation_case.id` (uuid) |
| **Business key** | `(ocean_txn_guid, content_type, file_date)` — bir OceanTxnGuid + network + gün kombinasyonu tek case oluşturur |
| **Duplicate kontrolü** | Aynı business key ile ikinci case açılmaz; varolan case güncellenir |
| **Geç gelen kayıt** | Clearing dosyası Card'dan sonra gelirse → mevcut OPEN case güncellenir, `clearing_file_line_id` set edilir |
| **Yeniden gelen kayıt** | ControlStat değişen clearing → mevcut case yeniden evaluate edilir; yeni evaluation kaydı açılır |
| **Reversal/refund/expire** | TxnStat=R/V/E → case_type REVERSAL/VOID/EXPIRED olarak etiketlenir; eşleşme orijinal case'e bağlanır (OceanMainTxnGuid ile) |
| **Eşleşmezse** | Card satırı clearing karşılığı bulamazsa → case_status=MISMATCHED, match_result=NO_MATCH kalır. Aging başlar. |
| **Result code** | `reconciliation_case.match_result` + `reconciliation_case.case_status` |

### 6.2 reconciliation_case → reconciliation_match

| Başlık | Açıklama |
|--------|----------|
| **PK mantığı** | `reconciliation_match.id`; case_id UNIQUE |
| **Business key** | case_id (1:1 ilişki) |
| **Duplicate kontrolü** | Bir case'e en fazla bir match kaydı yazılır |
| **Geç gelen kayıt** | Clearing geç gelirse ve mismatch→match'e dönerse, mevcut mismatch kaydı silinmez, match kaydı eklenir, case_status güncellenir |
| **Eşleşmezse** | Match kaydı oluşturulmaz; case MISMATCHED kalır |
| **Result code** | `reconciliation_match.match_type` (EXACT/FUZZY/MANUAL) |

### 6.3 reconciliation_case → reconciliation_mismatch

| Başlık | Açıklama |
|--------|----------|
| **PK mantığı** | `reconciliation_mismatch.id`; case_id + field_name bileşik unique |
| **Business key** | (case_id, field_name) |
| **Duplicate kontrolü** | Aynı case + field için tekrar mismatch yazılmaz |
| **Geç gelen kayıt** | Yeni clearing gelirse → mismatch'ler yeniden hesaplanır |
| **Reversal/refund/expire** | Reversal farkı ayrı field_name ile kaydedilir |
| **Eşleşmezse** | MISSING_COUNTERPART tipiyle tüm alan farkları tek kayıt olarak yazılır |
| **Result code** | `reconciliation_mismatch.severity` + `reconciliation_mismatch.mismatch_type` |

### 6.4 reconciliation_case → reconciliation_action (operation)

| Başlık | Açıklama |
|--------|----------|
| **PK mantığı** | `reconciliation_operation.id` |
| **Business key** | (case_id, action_code, sequence_index) |
| **Duplicate kontrolü** | `idempotency_key` ile — aynı key tekrar yazılmaz |
| **Geç gelen kayıt** | Yeni mismatch yeni action üretebilir; mevcut action'lar etkilenmez |
| **Reversal/refund/expire** | D5 (reversal reconciliation), D6 (expire handling) action'ları tetiklenir |
| **Eşleşmezse** | Mismatch varsa en az bir action üretilir (D1 minimum) |
| **Result code** | `reconciliation_operation.status` (Planned → Completed/Failed/Cancelled) |

### 6.5 reconciliation_action → reconciliation_action_execution

| Başlık | Açıklama |
|--------|----------|
| **PK mantığı** | `reconciliation_operation_execution.id` |
| **Business key** | (operation_id, attempt_number) |
| **Duplicate kontrolü** | Aynı operation + attempt tekrarlanmaz |
| **Yeniden deneme** | retry_count < max_retries → yeni attempt açılır |
| **Result code** | `reconciliation_operation_execution.status` + `result_code` |

### 6.6 reconciliation_action → reconciliation_manual_review

| Başlık | Açıklama |
|--------|----------|
| **PK mantığı** | `reconciliation_review.id` |
| **Business key** | (operation_id) — bir operation'a en fazla bir aktif review |
| **SLA** | `expires_at` alanı SLA deadline; aşılırsa `expiration_action` (Cancel/Approve/Reject) otomatik uygulanır |
| **Result code** | `reconciliation_review.decision` (Pending → Approved/Rejected/Cancelled) |

### 6.7 reconciliation_case → reconciliation_financial_impact

| Başlık | Açıklama |
|--------|----------|
| **PK mantığı** | `reconciliation_financial_impact.id` |
| **Business key** | (case_id, impact_type) |
| **Duplicate kontrolü** | Aynı case + impact_type güncellenir, tekrar yazılmaz |
| **Result code** | `is_resolved` flag |

### 6.8 reconciliation_case → reconciliation_balance_adjustment

| Başlık | Açıklama |
|--------|----------|
| **PK mantığı** | `reconciliation_balance_adjustment.id` |
| **Business key** | (case_id, operation_id, execution_id) |
| **Duplicate kontrolü** | execution_id unique olduğu için aynı execution'dan iki adjustment gelmez |
| **Result code** | `running_shadow_balance` alanı son durumu verir |

### 6.9 archive_candidate → archive_run_item

| Başlık | Açıklama |
|--------|----------|
| **PK mantığı** | `archive_run_item.id` |
| **Business key** | (archive_run_id, archive_candidate_id) |
| **Duplicate kontrolü** | Bir candidate bir run içinde tek item oluşturur |
| **Eşleşmezse** | Candidate FAILED/SKIPPED kalır; sonraki run'da tekrar denenebilir |
| **Result code** | `archive_run_item.status` (SUCCESS/FAILED/SKIPPED) |

---

## 7. D1-D8 Action Model

### 7.1 D Kodu Kataloğu

| Action Code | Tetik Koşulu | Input Alanları | Oluşturulan Kayıt | Finansal Etki | Gölge Bakiye Etkisi | Manual Review | Alarm | Kapanış Koşulu | Raporlama |
|-------------|-------------|----------------|-------------------|---------------|---------------------|---------------|-------|----------------|-----------|
| **D1** | Card-Clearing tutar farkı tespit edildi | case_id, card_amount, clearing_amount, currency | reconciliation_action + financial_impact | Fark tutarı kadar | Fark kadar shadow debit/credit | Fark > eşik ise ✓ | Fark > alarm eşiği ise ✓ | Fark giderildi veya manuel kabul edildi | R8, R10 |
| **D2** | Card kaydı var, Clearing karşılığı yok (unmatched) | case_id, card_line_id, ocean_txn_guid | reconciliation_action (bekle / eskalasyon) | Card tutarı kadar potansiyel | Card tutarı kadar shadow | T+3'ten sonra ✓ | T+5'ten sonra ✓ | Clearing geldi veya manuel kapatma | R7, R8 |
| **D3** | Clearing kaydı var, Card karşılığı yok (orphan) | clearing_line_id, ocean_txn_guid | reconciliation_action | Clearing tutarı kadar | Clearing tutarı kadar shadow | ✓ (her zaman) | ✓ | Card kaydı bulundu veya manuel red | R7, R8 |
| **D4** | ControlStat = Problem (P) gelen clearing kaydı | case_id, control_stat=P | reconciliation_action (izleme) | Belirsiz — tutar etkilenmemiş olabilir | Tutar kadar geçici shadow | ✗ (otomatik izleme) | ✓ | ControlStat X (ProblemToNormal) veya D (ChargebackClosing) gelene kadar | R8, R11 |
| **D5** | TxnStat=R (Reverse) Card kaydı — clearing karşılığı kontrolü | case_id, txn_stat=R | reconciliation_action (reversal reconciliation) | Orijinal tutar kadar ters etki | Reverse tutarı kadar shadow credit | Otomatik eşleşmezse ✓ | Eşleşmezse ✓ | Reverse clearing ile eşleşti | R8, R10 |
| **D6** | TxnStat=E (Expired) Card kaydı — settle olmamış provizyon | case_id, txn_stat=E | reconciliation_action (expire handling) | Expire tutarı kadar | Shadow release | ✗ | Toplu expire artışında ✓ | Otomatik — expire kesinleşir | R8, R10 |
| **D7** | OceanTxnGuid = 0 gelen kayıt — eşleşme yapılamıyor | line_id, ocean_txn_guid=0 | reconciliation_action (manuel araştırma) | Bilinmiyor | Tutar kadar shadow | ✓ (her zaman) | ✓ | Manuel olarak doğru eşleşme kuruldu veya red | R8, R9 |
| **D8** | ControlStat geçiş anomalisi (beklenmeyen stat sequence) | case_id, from_control_stat, to_control_stat | reconciliation_action (inceleme) | Belirsiz | Mevcut shadow korunur | ✓ | ✓ | Manuel inceleme tamamlandı | R8, R9, R11 |

### 7.2 Reconciliation Action State Machine

```
     ┌───────┐
     │  NEW  │
     └───┬───┘
         │ action planlandı
         ▼
    ┌──────────┐
    │ PLANNED  │
    └────┬─────┘
         │ requires_manual_review?
    ┌────┴────────────────┐
    │ ✗                   │ ✓
    ▼                     ▼
┌────────────────┐  ┌──────────────────┐
│READY_TO_EXECUTE│  │ PENDING_REVIEW   │
└───────┬────────┘  └────┬─────┬───────┘
        │                │     │
        │           approved rejected/expired
        │                │     │
        │                ▼     ▼
        │    ┌────────────────┐ ┌───────────┐
        │    │READY_TO_EXECUTE│ │ REJECTED  │ (terminal)
        │    └───────┬────────┘ └───────────┘
        │            │
        ▼            ▼
   ┌──────────────────┐
   │    EXECUTING      │ ← (Blocked ise buraya geçmeden Blocked'da bekler)
   └────┬─────┬────────┘
        │     │
    success  fail
        │     │
        ▼     ▼
┌──────────┐ ┌────────┐
│ EXECUTED │ │ FAILED │──→ retry? → PLANNED
└──────────┘ └────────┘      ✗ → FAILED (terminal)
                                   │
                              ┌────────────┐
                              │ CANCELLED  │ (terminal — manuel iptal)
                              └────────────┘
                              ┌────────────┐
                              │  EXPIRED   │ (terminal — SLA aşımı)
                              └────────────┘
```

**Geçiş kuralları:**

| Geçiş | Koşul |
|--------|-------|
| NEW → PLANNED | Evaluation tamamlandı, action oluşturuldu |
| PLANNED → PENDING_REVIEW | `requires_manual_review = true` |
| PLANNED → READY_TO_EXECUTE | `requires_manual_review = false` |
| PENDING_REVIEW → APPROVED → READY_TO_EXECUTE | Reviewer onayladı |
| PENDING_REVIEW → REJECTED | Reviewer reddetti |
| PENDING_REVIEW → CANCELLED | SLA aşıldı + expiration_action=Cancel |
| PENDING_REVIEW → EXPIRED | SLA aşıldı + expiration_action != Cancel |
| READY_TO_EXECUTE → EXECUTED | Execution başarılı |
| READY_TO_EXECUTE → FAILED | Execution başarısız |
| FAILED → PLANNED | retry_count < max_retries |
| Herhangi → CANCELLED | Manuel iptal |

> **Varsayım:** Mevcut `OperationStatus` enum'ı (Planned, Blocked, Executing, Completed, Failed, Cancelled) bu state machine'in altkümesidir. Yeni durumlar (NEW, PENDING_REVIEW, APPROVED, REJECTED, READY_TO_EXECUTE, EXECUTED, EXPIRED) eklenmesi veya mevcut Blocked durumunun PENDING_REVIEW ile eşlenmesi önerilir.

---

## 8. Rapor Kataloğu

### A. File Ingestion Raporları

#### R1 — Günlük Dosya Alım Özeti

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Günlük Dosya Alım Özeti |
| **Amacı** | Gün içinde gelen tüm dosyaların özet durumunu gösterir |
| **Business question** | Bugün kaç dosya geldi, kaçı başarılı, kaçı hatalı? |
| **Kullanılan tablolar** | `ingestion_file` |
| **Filtreler** | created_at (gün), file_type, content_type, source_type |
| **Boyutlar** | file_type, content_type, status |
| **Metrikler** | dosya_sayisi, toplam_satir, basarili_satir, hatali_satir, basari_orani |
| **Hesaplama** | basari_orani = success_count / total_count * 100 |
| **Alarm eşiği** | basari_orani < %95 → WARNING, < %90 → CRITICAL |
| **Operasyon aksiyonu** | Hatalı dosyaları incele, retry tetikle |
| **Yönetici KPI** | Dosya alım başarı oranı, günlük trend |
| **Önerilen grain** | Günlük / dosya bazlı |

#### R2 — Beklenen vs Gelen Dosya

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Beklenen vs Gelen Dosya Raporu |
| **Amacı** | Beklenen dosyaların gelip gelmediğini, sıra boşluklarını tespit eder |
| **Business question** | Bugün hangi dosyalar bekleniyor, hangileri geldi, boşluk var mı? |
| **Kullanılan tablolar** | `ingestion_file`, `ingestion_file_control` |
| **Filtreler** | file_date, content_type |
| **Boyutlar** | content_type, file_type |
| **Metrikler** | beklenen_dosya_sayisi, gelen_dosya_sayisi, eksik_dosya, sequence_gap, count_mismatch |
| **Hesaplama** | beklenen = konfigürasyondan; gelen = ingestion_file count; gap = sequence_no farki |
| **Alarm eşiği** | Eksik dosya > 0 veya sequence_gap = true → CRITICAL |
| **Operasyon aksiyonu** | Kaynağı kontrol et, SFTP bağlantısını incele |
| **Yönetici KPI** | Dosya tamamlanma oranı |
| **Önerilen grain** | Günlük |

#### R3 — Parse / Validation Hata Raporu

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Parse ve Validation Hata Raporu |
| **Amacı** | Parse ve validation hatalarının detayını ve dağılımını gösterir |
| **Business question** | Hangi dosyalarda, hangi satırlarda, ne tür hatalar oluştu? |
| **Kullanılan tablolar** | `ingestion_parse_error`, `ingestion_validation_error`, `ingestion_file`, `ingestion_file_line` |
| **Filtreler** | created_at, file_id, error_code, rule_code, severity |
| **Boyutlar** | error_code/rule_code, severity, file_type, content_type |
| **Metrikler** | hata_sayisi, dosya_bazli_hata_orani, alan_bazli_dagilim |
| **Hesaplama** | hata_orani = error_count / total_count * 100 (dosya bazlı) |
| **Alarm eşiği** | Tek dosyada hata oranı > %5 → WARNING, > %10 → CRITICAL |
| **Operasyon aksiyonu** | Kaynak format değişikliği mi? Parser güncellenmeli mi? |
| **Yönetici KPI** | Günlük parse/validation hata trendi |
| **Önerilen grain** | Dosya bazlı / kayıt bazlı |

#### R4 — Duplicate Kayıt Raporu

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Duplicate Kayıt Raporu |
| **Amacı** | Teknik ve business duplicate tespit edilen kayıtların detayı |
| **Business question** | Bugün kaç duplicate kayıt geldi, hangi tipte, çelişki var mı? |
| **Kullanılan tablolar** | `ingestion_duplicate_log`, `ingestion_file_line` |
| **Filtreler** | created_at, duplicate_type, decision |
| **Boyutlar** | duplicate_type (TECHNICAL/BUSINESS), decision, content_type |
| **Metrikler** | duplicate_sayisi, conflict_sayisi, duplicate_orani |
| **Hesaplama** | duplicate_orani = secondary+conflict count / toplam satir * 100 |
| **Alarm eşiği** | conflict > 0 → WARNING; duplicate_orani > %2 → CRITICAL |
| **Operasyon aksiyonu** | Conflict kayıtları manuel incele |
| **Yönetici KPI** | Günlük duplicate oranı |
| **Önerilen grain** | Günlük / dosya bazlı |

#### R5 — Dosya Bazlı Business Key Üretim Raporu

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Business Key Üretim Raporu |
| **Amacı** | Business key üretim kapsamını ve OceanTxnGuid=0 durumunu gösterir |
| **Business question** | Kaç satır için business key üretildi, kaçında OceanTxnGuid eksik? |
| **Kullanılan tablolar** | `ingestion_business_key`, `ingestion_file_line`, `ingestion_file` |
| **Filtreler** | created_at, key_type, is_ocean_txn_guid_zero |
| **Boyutlar** | key_type, content_type, is_ocean_txn_guid_zero |
| **Metrikler** | toplam_key, zero_guid_sayisi, coverage_orani |
| **Hesaplama** | coverage = key_count / success_line_count * 100 |
| **Alarm eşiği** | coverage < %99 → WARNING; zero_guid_orani > %1 → CRITICAL |
| **Operasyon aksiyonu** | Zero GUID kayıtları D7 action ile izlenir |
| **Yönetici KPI** | Business key coverage oranı |
| **Önerilen grain** | Günlük / dosya bazlı |

### B. Reconciliation Raporları

#### R6 — Günlük Mutabakat Özeti

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Günlük Mutabakat Özeti |
| **Amacı** | Gün bazlı eşleşme sonuçlarının özeti |
| **Business question** | Bugün kaç case açıldı, kaçı eşleşti, kaçı açık kaldı? |
| **Kullanılan tablolar** | `reconciliation_case`, `reconciliation_match`, `reconciliation_mismatch` |
| **Filtreler** | file_date, content_type, case_status, match_result |
| **Boyutlar** | content_type, case_status, match_result |
| **Metrikler** | toplam_case, matched_count, mismatched_count, unmatched_count, clean_match_rate |
| **Hesaplama** | clean_match_rate = FULL_MATCH / toplam_case * 100 |
| **Alarm eşiği** | clean_match_rate < %95 → WARNING, < %90 → CRITICAL |
| **Operasyon aksiyonu** | Düşük match rate → kaynak dosya ve parser incele |
| **Yönetici KPI** | Clean match rate, günlük trend |
| **Önerilen grain** | Günlük |

#### R7 — Açık Farklar ve Aging Raporu

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Açık Farklar ve Aging Raporu |
| **Amacı** | Kapatılmamış case'lerin yaşlandırma analizi |
| **Business question** | Kaç gündür açık kalan farklar var, toplam tutarı ne? |
| **Kullanılan tablolar** | `reconciliation_case`, `reconciliation_financial_impact` |
| **Filtreler** | case_status IN (OPEN, MISMATCHED), opened_at |
| **Boyutlar** | aging_bucket (0-1, 2-3, 4-7, 8-14, 15-30, 30+), content_type, case_type |
| **Metrikler** | acik_case_sayisi, toplam_fark_tutari, ortalama_yas_gun |
| **Hesaplama** | yas = CURRENT_DATE - opened_at; bucket = CASE WHEN... |
| **Alarm eşiği** | 7+ gün açık case > 10 → WARNING; 30+ gün > 0 → CRITICAL |
| **Operasyon aksiyonu** | Yaşlı farkları eskalasyon, D2/D3 tetikle |
| **Yönetici KPI** | Aged open items, açık fark trendi |
| **Önerilen grain** | Günlük (snapshot) |

#### R8 — D1-D8 Aksiyon Raporu

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | D1-D8 Aksiyon Raporu |
| **Amacı** | Tüm düzeltme aksiyonlarının durumunu ve dağılımını gösterir |
| **Business question** | Hangi D kodlarında kaç aksiyon var, başarı oranı ne? |
| **Kullanılan tablolar** | `reconciliation_operation`, `reconciliation_operation_execution` |
| **Filtreler** | action_code, status, created_at |
| **Boyutlar** | action_code (D1-D8), status, content_type |
| **Metrikler** | aksiyon_sayisi, completed_count, failed_count, pending_count, success_rate |
| **Hesaplama** | success_rate = completed / (completed + failed) * 100 |
| **Alarm eşiği** | Belirli D kodunda anormal artış (önceki 7 gün ortalamasının 2x'i) → WARNING |
| **Operasyon aksiyonu** | Failed aksiyonları incele, retry tetikle |
| **Yönetici KPI** | Aksiyon başarı oranı, D kodu dağılımı |
| **Önerilen grain** | Günlük / D kodu bazlı |

#### R9 — Manual Review Bekleyenler Raporu

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Manual Review Bekleyenler Raporu |
| **Amacı** | Onay bekleyen review'ların listesi ve SLA durumu |
| **Business question** | Kaç review bekliyor, kaçı SLA'ı aşmak üzere? |
| **Kullanılan tablolar** | `reconciliation_review`, `reconciliation_operation` |
| **Filtreler** | decision=Pending, expires_at |
| **Boyutlar** | action_code, content_type, sla_bucket |
| **Metrikler** | pending_count, sla_at_risk_count, sla_breached_count, ortalama_bekleme_suresi |
| **Hesaplama** | sla_at_risk = expires_at - NOW() < 4 saat; breached = expires_at < NOW() |
| **Alarm eşiği** | sla_breached > 0 → CRITICAL |
| **Operasyon aksiyonu** | Hemen review yap, eskalasyon |
| **Yönetici KPI** | Pending review count, SLA compliance |
| **Önerilen grain** | Anlık / günlük |

#### R10 — Finansal Etki / Gölge Bakiye Raporu

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Finansal Etki ve Gölge Bakiye Raporu |
| **Amacı** | Açık farkların finansal etkisini ve gölge bakiye toplam durumunu gösterir |
| **Business question** | Toplam açık finansal etki ne kadar, gölge bakiye nerede? |
| **Kullanılan tablolar** | `reconciliation_financial_impact`, `reconciliation_balance_adjustment` |
| **Filtreler** | is_resolved, created_at, currency, direction |
| **Boyutlar** | impact_type, currency, direction, is_resolved |
| **Metrikler** | toplam_acik_fark, toplam_shadow_balance, resolved_amount, unresolved_amount |
| **Hesaplama** | shadow_balance = SUM(amount) FROM balance_adjustment WHERE currency=X |
| **Alarm eşiği** | shadow_balance > konfigüre edilen eşik → CRITICAL |
| **Operasyon aksiyonu** | Yüksek shadow balance → eskalasyon, D1-D3 hızlandır |
| **Yönetici KPI** | Shadow balance total, unresolved financial impact |
| **Önerilen grain** | Günlük (snapshot) |

#### R11 — Reconciliation Alarm Raporu

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Reconciliation Alarm Raporu |
| **Amacı** | Tüm aktif ve geçmiş alarmların durumu |
| **Business question** | Kaç alarm açık, kaçı kapandı, ortalama kapanış süresi ne? |
| **Kullanılan tablolar** | `reconciliation_alert` |
| **Filtreler** | alert_status, severity, alert_code, created_at |
| **Boyutlar** | alert_code, severity, alert_status |
| **Metrikler** | acik_alarm_sayisi, kapanan_alarm_sayisi, ort_kapanma_suresi, sla_breach_count |
| **Hesaplama** | ort_kapanma = AVG(closed_at - created_at) |
| **Alarm eşiği** | 24 saatten fazla açık alarm > 5 → CRITICAL |
| **Operasyon aksiyonu** | Eski alarmları incele, eskalasyon |
| **Yönetici KPI** | Alert closure rate, MTTR |
| **Önerilen grain** | Günlük |

### C. Archive Raporları

#### R12 — Archive Uygunluk Raporu

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Archive Uygunluk Raporu |
| **Amacı** | Archive'a uygun adayların listesi ve backlog durumu |
| **Business question** | Kaç dosya arşive uygun, backlog ne kadar? |
| **Kullanılan tablolar** | `archive_candidate`, `ingestion_file` |
| **Filtreler** | status=PENDING, is_eligible, eligible_after |
| **Boyutlar** | content_type, file_type, eligibility_reason |
| **Metrikler** | eligible_count, pending_count, backlog_days |
| **Hesaplama** | backlog_days = MAX(CURRENT_DATE - eligible_after) WHERE status=PENDING AND is_eligible=true |
| **Alarm eşiği** | backlog_days > 7 → WARNING, > 14 → CRITICAL |
| **Operasyon aksiyonu** | Archive job'ını tetikle/incele |
| **Yönetici KPI** | Archive backlog days |
| **Önerilen grain** | Günlük |

#### R13 — Archive Başarı/Başarısızlık Raporu

| Başlık | Açıklama |
|--------|----------|
| **Rapor adı** | Archive Başarı/Başarısızlık Raporu |
| **Amacı** | Archive run sonuçlarını ve snapshot bütünlüğünü gösterir |
| **Business question** | Son archive run başarılı mı, checksum eşleşiyor mu? |
| **Kullanılan tablolar** | `archive_log`, `archive_run_item`, `archive_snapshot`, `archive_error_log` |
| **Filtreler** | created_at, status |
| **Boyutlar** | run_type, status |
| **Metrikler** | run_count, success_rate, checksum_mismatch_count, lines_archived, error_count |
| **Hesaplama** | success_rate = success_count / total_candidates * 100 |
| **Alarm eşiği** | checksum_mismatch > 0 → CRITICAL; success_rate < %100 → WARNING |
| **Operasyon aksiyonu** | Checksum hatası → veri bütünlüğü incelemesi; failure → retry |
| **Yönetici KPI** | Archive success rate |
| **Önerilen grain** | Run bazlı / günlük |

---

## 9. View / Semantic Layer Tasarımı

### v_ingestion_daily_summary

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Günlük dosya alım özet dashboard'u |
| **Kaynak tablolar** | `ingestion_file` |
| **Join** | Yok — tek tablo aggregation |
| **Grain** | Gün + file_type + content_type |
| **Refresh** | Materialized view; her 15 dk veya incremental |
| **Index** | (report_date, file_type, content_type) |
| **Riskler** | Gün içinde çalışan dosyalar Processing statüsünde olabilir |
| **Dashboard** | Dosya alım durumu kartları, trend çizgisi |

```sql
CREATE VIEW v_ingestion_daily_summary AS
SELECT 
    DATE(created_at) AS report_date,
    file_type, content_type, status,
    COUNT(*) AS file_count,
    SUM(total_count) AS total_lines,
    SUM(success_count) AS success_lines,
    SUM(error_count) AS error_lines,
    CASE WHEN SUM(total_count) > 0 
         THEN ROUND(SUM(success_count)::numeric / SUM(total_count) * 100, 2) 
         ELSE 0 END AS success_rate
FROM ingestion_file
GROUP BY DATE(created_at), file_type, content_type, status;
```

### v_ingestion_expected_vs_received

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Beklenen vs gelen dosya karşılaştırması |
| **Kaynak tablolar** | `ingestion_file`, `ingestion_file_control` |
| **Join** | ingestion_file LEFT JOIN ingestion_file_control ON file_id |
| **Grain** | Gün + content_type |
| **Refresh** | Her saat veya dosya işleme sonrası |
| **Index** | (report_date, content_type) |
| **Riskler** | "Beklenen" dosya sayısı konfigürasyondan gelir; bu view'a hardcode veya config tablosu gerekli |
| **Dashboard** | Beklenen-gelen gap kartı, sequence gap uyarısı |

### v_ingestion_errors

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Parse ve validation hatalarının birleşik görünümü |
| **Kaynak tablolar** | `ingestion_parse_error`, `ingestion_validation_error`, `ingestion_file` |
| **Join** | UNION ALL (parse_error + validation_error) JOIN ingestion_file |
| **Grain** | Kayıt bazlı |
| **Refresh** | Real-time view (materialized değil) |
| **Index** | Kaynak tablolardaki mevcut indexler yeterli |
| **Riskler** | Yüksek hacimde UNION ALL yavaşlayabilir |
| **Dashboard** | Hata tipi dağılım bar chart, spike alert |

### v_ingestion_duplicates

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Duplicate karar log özeti |
| **Kaynak tablolar** | `ingestion_duplicate_log`, `ingestion_file_line` |
| **Join** | duplicate_log JOIN file_line ON ingestion_file_line_id |
| **Grain** | Kayıt bazlı + günlük aggregation |
| **Refresh** | Real-time |
| **Index** | Kaynak indexler yeterli |
| **Riskler** | Conflict kayıtları manuel müdahale gerektirir |
| **Dashboard** | Duplicate oranı gauge, conflict listesi |

### v_reconciliation_daily_summary

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Günlük mutabakat özeti |
| **Kaynak tablolar** | `reconciliation_case` |
| **Join** | Tek tablo aggregation |
| **Grain** | Gün + content_type |
| **Refresh** | Materialized; her 15 dk |
| **Index** | (file_date, content_type) |
| **Riskler** | Gün içi case'ler henüz OPEN olabilir |
| **Dashboard** | Match rate trend, case status dağılımı |

### v_reconciliation_open_items

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Açık kalan case'lerin detay listesi |
| **Kaynak tablolar** | `reconciliation_case`, `reconciliation_mismatch`, `reconciliation_financial_impact` |
| **Join** | case LEFT JOIN mismatch ON case_id LEFT JOIN financial_impact ON case_id |
| **Grain** | Case bazlı |
| **Refresh** | Real-time |
| **Index** | case.case_status partial index WHERE case_status IN ('OPEN','MISMATCHED') |
| **Riskler** | Yüksek açık case sayısında performans |
| **Dashboard** | Açık farklar listesi, detay drill-down |

### v_reconciliation_aging

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Case yaşlandırma bucket analizi |
| **Kaynak tablolar** | `reconciliation_case`, `reconciliation_financial_impact` |
| **Join** | case JOIN financial_impact ON case_id |
| **Grain** | Aging bucket (0-1, 2-3, 4-7, 8-14, 15-30, 30+) |
| **Refresh** | Materialized; günde 1 kez |
| **Index** | — (materialized view) |
| **Riskler** | Bucket tanımları değişebilir |
| **Dashboard** | Aging bar chart, tutar bazlı heat map |

### v_reconciliation_actions

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Aksiyon durumu ve D kodu dağılımı |
| **Kaynak tablolar** | `reconciliation_operation`, `reconciliation_operation_execution` |
| **Join** | operation LEFT JOIN execution ON operation_id |
| **Grain** | Operation bazlı |
| **Refresh** | Real-time |
| **Index** | operation.status, operation.action_code |
| **Riskler** | Execution sayısı yüksek olabilir; son execution filtrelenmeli |
| **Dashboard** | D kodu bar chart, success rate gauge |

### v_reconciliation_manual_review

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Pending review listesi ve SLA durumu |
| **Kaynak tablolar** | `reconciliation_review`, `reconciliation_operation` |
| **Join** | review JOIN operation ON operation_id |
| **Grain** | Review bazlı |
| **Refresh** | Real-time |
| **Index** | review.decision partial index WHERE decision = 'Pending' |
| **Riskler** | SLA hesaplaması timezone'a duyarlı |
| **Dashboard** | Pending count kartı, SLA risk listesi |

### v_reconciliation_financial_impact

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Finansal etki ve gölge bakiye toplam durumu |
| **Kaynak tablolar** | `reconciliation_financial_impact`, `reconciliation_balance_adjustment` |
| **Join** | fi LEFT JOIN ba ON case_id |
| **Grain** | Currency bazlı aggregation |
| **Refresh** | Materialized; her 30 dk |
| **Index** | — (materialized view) |
| **Riskler** | Çoklu para birimi dönüşüm gerekebilir |
| **Dashboard** | Shadow balance gauge, unresolved amount trend |

### v_reconciliation_alerts

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Aktif alarmlar ve kapanış durumu |
| **Kaynak tablolar** | `reconciliation_alert` |
| **Join** | Tek tablo |
| **Grain** | Alert bazlı |
| **Refresh** | Real-time |
| **Index** | alert_status partial index WHERE alert_status IN ('Pending','Processing') |
| **Riskler** | Alarm flood durumunda UI yavaşlayabilir |
| **Dashboard** | Alarm listesi, severity dağılımı |

### v_archive_eligibility

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Archive adayları ve backlog durumu |
| **Kaynak tablolar** | `archive_candidate`, `ingestion_file` |
| **Join** | candidate JOIN ingestion_file ON ingestion_file_id |
| **Grain** | Candidate bazlı |
| **Refresh** | Günde 1 kez |
| **Index** | candidate.status, candidate.is_eligible |
| **Riskler** | Retention policy değişirse backlog aniden artabilir |
| **Dashboard** | Backlog days gauge, eligible count trend |

### v_archive_runs

| Başlık | Açıklama |
|--------|----------|
| **Amacı** | Archive run sonuçları ve snapshot bütünlüğü |
| **Kaynak tablolar** | `archive_log`, `archive_run_item`, `archive_snapshot` |
| **Join** | log JOIN run_item ON archive_run_id LEFT JOIN snapshot ON run_item_id |
| **Grain** | Run bazlı |
| **Refresh** | Real-time |
| **Index** | log.created_at |
| **Riskler** | Snapshot checksum hesaplama süresi uzun olabilir |
| **Dashboard** | Son run durumu, checksum match rate |

---

## 10. KPI Sözlüğü

| # | KPI | İş Tanımı | Formül | Tablolar | Sıklık | Hedef / Eşik | Alarm |
|---|-----|-----------|--------|----------|--------|---------------|-------|
| 1 | **File Arrival Timeliness** | Beklenen dosyaların zamanında gelip gelmediği | (zamanında_gelen_dosya / beklenen_dosya) * 100 | `ingestion_file`, `ingestion_file_control` | Saatlik | ≥ %100; < %100 → alarm | ✓ |
| 2 | **Ingestion Success Rate** | Dosya bazlı işleme başarı oranı | (status=Success dosya / toplam dosya) * 100 | `ingestion_file` | Günlük | ≥ %99; < %95 → kritik | ✓ |
| 3 | **Validation Fail Rate** | Satır bazlı validation hata oranı | (validation_error_count / toplam_satir) * 100 | `ingestion_validation_error`, `ingestion_file` | Günlük | ≤ %1; > %5 → kritik | ✓ |
| 4 | **Duplicate Rate** | Duplicate kayıt oranı | (secondary+conflict count / toplam_satir) * 100 | `ingestion_duplicate_log`, `ingestion_file_line` | Günlük | ≤ %0.5; > %2 → alarm | ✓ |
| 5 | **Business Key Coverage Rate** | Business key üretilen satır oranı | (business_key_count / success_line_count) * 100 | `ingestion_business_key`, `ingestion_file_line` | Günlük | ≥ %99.5; < %99 → alarm | ✓ |
| 6 | **Reconciliation Clean Match Rate** | Tam eşleşme oranı | (FULL_MATCH case / toplam case) * 100 | `reconciliation_case` | Günlük | ≥ %95; < %90 → kritik | ✓ |
| 7 | **Mismatch Rate** | Uyumsuzluk oranı | (MISMATCHED case / toplam case) * 100 | `reconciliation_case` | Günlük | ≤ %5; > %10 → kritik | ✓ |
| 8 | **Manual Review Pending Count** | Bekleyen review sayısı | COUNT(*) WHERE decision=Pending | `reconciliation_review` | Anlık | 0 ideal; > 10 → alarm | ✓ |
| 9 | **Action Success Rate** | Aksiyon başarı oranı | (Completed / (Completed+Failed)) * 100 | `reconciliation_operation` | Günlük | ≥ %95; < %90 → alarm | ✓ |
| 10 | **Open Difference Amount** | Açık farkların toplam tutarı | SUM(difference_amount) WHERE is_resolved=false | `reconciliation_financial_impact` | Günlük | ≤ konfigüre eşik; aşarsa kritik | ✓ |
| 11 | **Aged Open Items** | 7+ gün açık case sayısı | COUNT(*) WHERE case_status IN (OPEN,MISMATCHED) AND age > 7 | `reconciliation_case` | Günlük | 0 ideal; > 5 → alarm | ✓ |
| 12 | **Shadow Balance Total** | Toplam gölge bakiye | SUM(running_shadow_balance) son kayıt per currency | `reconciliation_balance_adjustment` | Günlük | ≤ konfigüre eşik | ✓ |
| 13 | **Alert Closure Rate** | Alarm kapanma oranı | (Consumed / (Consumed+Pending+Processing)) * 100 | `reconciliation_alert` | Günlük | ≥ %90; < %80 → alarm | ✓ |
| 14 | **Archive Success Rate** | Archive başarı oranı | (success_count / total_candidates) * 100 per run | `archive_log`, `archive_run_item` | Her run | %100; < %100 → alarm | ✓ |
| 15 | **Archive Backlog Days** | En eski eligible ama arşivlenmemiş adayın yaşı | MAX(CURRENT_DATE - eligible_after) WHERE status=PENDING AND is_eligible=true | `archive_candidate` | Günlük | ≤ 3 gün; > 7 → alarm | ✓ |

---

## 11. Data Quality ve Audit Kuralları

| # | Rule Code | Kural Tanımı | Tablo | Blocking/Warning | İhlal Yazılır | Operasyon Aksiyonu |
|---|-----------|-------------|-------|-----------------|----------------|-------------------|
| 1 | **DQ-ING-001** | File header code doğru mu (H bekleniyor) | `ingestion_file_control` | Blocking | `ingestion_parse_error` | Dosya reddedilir; kaynak kontrol edilir |
| 2 | **DQ-ING-002** | File footer code doğru mu (F bekleniyor) | `ingestion_file_control` | Blocking | `ingestion_parse_error` | Dosya reddedilir |
| 3 | **DQ-ING-003** | Footer line count = actual detail count | `ingestion_file_control` | Blocking | `ingestion_file_control.count_match=false` | Dosya reddedilir veya WARNING ile devam |
| 4 | **DQ-ING-004** | Parse başarısız satır oranı ≤ %5 | `ingestion_parse_error`, `ingestion_file` | Warning (> %5 → Blocking > %10) | `reconciliation_alert` (PARSE_SPIKE) | Hata inceleme; parser güncelleme |
| 5 | **DQ-ING-005** | Zorunlu alan kontrolü (OceanTxnGuid, CardNo, Amount vb.) | `ingestion_validation_error` | Blocking (satır bazlı) | `ingestion_validation_error` | Satır Failed; raporda gösterilir |
| 6 | **DQ-ING-006** | Alan format kontrolü (tarih YYYYMMDD, tutar numeric vb.) | `ingestion_validation_error` | Blocking (satır bazlı) | `ingestion_validation_error` | Satır Failed |
| 7 | **DQ-ING-007** | Tarih/tutar/para birimi tutarlılığı (SourceAmount > 0, Currency geçerli ISO) | `ingestion_validation_error` | Warning | `ingestion_validation_error` | İnceleme |
| 8 | **DQ-ING-008** | Duplicate file kontrolü (file_key unique) | `ingestion_file` | Blocking | `ingestion_file.status=Failed` | Dosya reddedilir |
| 9 | **DQ-ING-009** | Duplicate business transaction kontrolü | `ingestion_duplicate_log` | Warning (Conflict → Blocking) | `ingestion_duplicate_log` | Conflict ise manuel inceleme |
| 10 | **DQ-REC-001** | ControlStat history tutarlılığı (P→N geçişi X olmalı) | `ingestion_clearing_*_detail` | Warning | `reconciliation_alert` (CONTROLSTAT_ANOMALY) | D8 action tetiklenir |
| 11 | **DQ-REC-002** | OceanTxnGuid = 0 kontrolü | `ingestion_business_key` | Warning | `reconciliation_alert` (ZERO_GUID) | D7 action tetiklenir |
| 12 | **DQ-REC-003** | Mismatch severity sınıflaması (CRITICAL: tutar > eşik; HIGH: statü farkı; MEDIUM: tarih farkı; LOW: diğer) | `reconciliation_mismatch` | N/A (sınıflama kuralı) | `reconciliation_mismatch.severity` | Severity'ye göre SLA |
| 13 | **DQ-REC-004** | Action execution audit zorunluluğu — her execution mutlaka log yazmalı | `reconciliation_operation_execution` | Blocking | `reconciliation_audit` | Execution logsuz tamamlanamaz |
| 14 | **DQ-REC-005** | Manual review SLA kontrolü (expires_at < NOW() ise otomatik aksiyon) | `reconciliation_review` | Warning | `reconciliation_alert` (SLA_BREACH) | Otomatik expire action tetiklenir |
| 15 | **DQ-ARC-001** | Archive eligibility kontrolü (recon tamamlanmış + retention dolmuş) | `archive_candidate` | Blocking | `archive_candidate.is_eligible=false` | Aday arşivlenmez |
| 16 | **DQ-ARC-002** | Archive snapshot bütünlüğü (source_checksum = archive_checksum) | `archive_snapshot` | Blocking | `archive_error_log` (CHECKSUM_MISMATCH) | Archive geri alınır, yeniden dener |

---

## 12. Alarm Kataloğu

| # | Alarm Kodu | Alarm Adı | Süreç | Tetik Koşulu | Severity | İlk Bakacak Ekip | Otomatik Aksiyon | Manuel Aksiyon | Rapor | Kapanış Kuralı |
|---|-----------|-----------|-------|-------------|----------|------------------|-----------------|----------------|-------|----------------|
| 1 | **ALR-ING-001** | Beklenen Dosya Gelmedi | Ingestion | Beklenen dosya konfigüre edilen saatte gelmedi | CRITICAL | Operasyon | Retry SFTP bağlantısı | Kaynak kontrol | R2 | Dosya geldi |
| 2 | **ALR-ING-002** | Duplicate File | Ingestion | Aynı file_key ile ikinci dosya geldi | HIGH | Operasyon | Dosya reddedildi | İnceleme | R1 | Manuel kapatma |
| 3 | **ALR-ING-003** | Parse Error Spike | Ingestion | Tek dosyada parse hata oranı > %5 | HIGH | Geliştirme | — | Parser/format inceleme | R3 | Hata oranı düştü |
| 4 | **ALR-ING-004** | Validation Error Spike | Ingestion | Tek dosyada validation hata oranı > %5 | HIGH | Operasyon | — | Kaynak veri kalitesi inceleme | R3 | Hata oranı düştü |
| 5 | **ALR-ING-005** | Business Key Üretilemeyen Kayıt | Ingestion | Business key coverage < %99 | MEDIUM | Operasyon | — | İlgili satırları incele | R5 | Coverage normal |
| 6 | **ALR-ING-006** | OceanTxnGuid Missing/Zero | Ingestion | Zero GUID oranı > %1 veya count > konfigüre eşik | HIGH | Operasyon + Geliştirme | D7 action tetiklendi | Kaynak inceleme | R5 | Zero GUID case'ler kapatıldı |
| 7 | **ALR-ING-007** | Duplicate Business Record | Ingestion | Conflict tipi duplicate > 0 | HIGH | Operasyon | — | Manuel inceleme | R4 | Conflict çözüldü |
| 8 | **ALR-REC-001** | Clean Match Rate Düşüşü | Reconciliation | Clean match rate < %90 | CRITICAL | Operasyon | — | Geniş inceleme, eskalasyon | R6 | Rate normale döndü |
| 9 | **ALR-REC-002** | Mismatch Amount Threshold Breach | Reconciliation | Tek case'de tutar farkı > konfigüre eşik | CRITICAL | Operasyon + Finans | — | Acil inceleme | R7 | Case resolved |
| 10 | **ALR-REC-003** | Pending Manual Review SLA Breach | Reconciliation | Review expires_at < NOW() | CRITICAL | Operasyon | Expiration action (Cancel/Approve/Reject) | Eskalasyon | R9 | Review tamamlandı |
| 11 | **ALR-REC-004** | Action Execution Failure | Reconciliation | Operation status=Failed AND retry_count >= max_retries | HIGH | Geliştirme | — | Manuel müdahale | R8 | Manuel fix veya cancel |
| 12 | **ALR-REC-005** | D1-D8 Anormal Artış | Reconciliation | Belirli D kodunda son 7 gün ortalamasının 2x'i | MEDIUM | Operasyon | — | Trend analizi | R8 | Trend normalize |
| 13 | **ALR-REC-006** | Shadow Balance Threshold Breach | Reconciliation | Toplam shadow balance > konfigüre eşik | CRITICAL | Finans + Operasyon | — | Acil D1-D3 hızlandır | R10 | Balance eşik altına düştü |
| 14 | **ALR-ARC-001** | Archive Aday Backlog Artışı | Archive | backlog_days > 7 | MEDIUM | Operasyon | — | Archive job tetikle/incele | R12 | Backlog temizlendi |
| 15 | **ALR-ARC-002** | Archive Failure | Archive | archive_run_item.status = FAILED | HIGH | Geliştirme | Retry | İnceleme | R13 | Retry başarılı |
| 16 | **ALR-ARC-003** | Snapshot Failure (Checksum Mismatch) | Archive | archive_snapshot.checksum_match = false | CRITICAL | Geliştirme + Operasyon | Archive geri al | Veri bütünlüğü inceleme | R13 | Snapshot düzeltildi |
| 17 | **ALR-REC-007** | Alert Kapanmama Süresi İhlali | Reconciliation | Alert açık kalma süresi > 24 saat | MEDIUM | Operasyon | — | Eskalasyon | R11 | Alert kapatıldı |
| 18 | **ALR-REC-008** | ControlStat Problemli Kayıt Artışı | Reconciliation | ControlStat=P gelen kayıt sayısı > önceki günlerin 2x'i | HIGH | Operasyon | D4 action otomatik | İnceleme | R8 | Problem kayıtlar P→X/D geçti |
| 19 | **ALR-REC-009** | Expired/Reversal Anomali Artışı | Reconciliation | TxnStat=E veya R kayıt sayısında anormal artış (>2x) | MEDIUM | Operasyon | D5/D6 action otomatik | Trend analizi | R8 | Trend normalize |
| 20 | **ALR-ING-008** | File Sequence Anomaly | Ingestion | sequence_gap_detected = true | HIGH | Operasyon | — | Kaynak kontrol, eksik dosya iste | R2 | Eksik dosya alındı veya gap kabul edildi |

---

## 13. Fazlandırılmış Uygulama Önerisi

### MVP (Faz 1) — Temel Altyapı

**Tablolar:**
- `ingestion_file` (mevcut)
- `ingestion_file_line` (mevcut)
- `ingestion_file_control` (yeni)
- `ingestion_parse_error` (yeni)
- `ingestion_validation_error` (yeni)
- `ingestion_duplicate_log` (yeni)
- `ingestion_business_key` (yeni)
- `reconciliation_case` (yeni)
- `reconciliation_match` (yeni)
- `reconciliation_mismatch` (yeni)
- `reconciliation_evaluation` (mevcut)
- `reconciliation_operation` (mevcut — action_code, case_id eklenmeli)
- `reconciliation_operation_execution` (mevcut)
- `archive_log` (mevcut)

**Raporlar:**
- R1: Günlük dosya alım özeti
- R2: Beklenen vs gelen dosya
- R3: Parse/validation hata raporu
- R6: Günlük mutabakat özeti

**Alarmlar:**
- ALR-ING-001: Beklenen dosya gelmedi
- ALR-ING-003: Parse error spike
- ALR-ING-006: OceanTxnGuid zero
- ALR-REC-001: Clean match rate düşüşü

**Kritik veri kalitesi kuralları:**
- DQ-ING-001, 002, 003: Header/footer/count kontrolü
- DQ-ING-005: Zorunlu alan
- DQ-ING-008: Duplicate file
- DQ-REC-002: OceanTxnGuid zero kontrolü

---

### Phase 2 — Aksiyon ve Review

**Tablolar:**
- `reconciliation_financial_impact` (yeni)
- `reconciliation_balance_adjustment` (yeni)
- `reconciliation_review` (mevcut — sla_deadline, is_sla_breached eklenmeli)
- `reconciliation_alert` (mevcut — alert_code, closed_at eklenmeli)
- `reconciliation_audit` (yeni)
- `ingestion_status_history` (yeni)

**Raporlar:**
- R4: Duplicate kayıt raporu
- R5: Business key üretim raporu
- R7: Açık farklar ve aging
- R8: D1-D8 aksiyon raporu
- R9: Manual review bekleyenler
- R10: Finansal etki / gölge bakiye
- R11: Alarm raporu

**Alarmlar:**
- ALR-ING-002, 004, 005, 007
- ALR-REC-002, 003, 004, 005, 006
- ALR-REC-007, 008, 009

**Veri kalitesi kuralları:**
- DQ-ING-004, 006, 007, 009
- DQ-REC-001, 003, 004, 005

---

### Phase 3 — Archive ve Olgunlaşma

**Tablolar:**
- `archive_candidate` (yeni)
- `archive_run_item` (yeni)
- `archive_snapshot` (yeni)
- `archive_error_log` (yeni)
- `archive_log` genişletme (run_type, started_at, count alanları)

**Raporlar:**
- R12: Archive uygunluk raporu
- R13: Archive başarı/başarısızlık raporu

**Alarmlar:**
- ALR-ARC-001, 002, 003
- ALR-ING-008: File sequence anomaly

**Veri kalitesi kuralları:**
- DQ-ARC-001, 002
- DQ-REC-003 (mismatch severity refinement)

---

## 14. Kapsam Dışı Domainler

Aşağıdaki domainler bu modelin **kesinlikle dışındadır**. Bu alanlara ait tablolar, endpointler veya iş kuralları bu tasarımda yer almaz:

1. **Paycore customer/card/debit/prepaid servis domaini**
   - Customer entity, wallet, card lifecycle tabloları
   - Debit authorization akışı
   - Prepaid kart yönetimi

2. **Core banking'in kendi muhasebe/GL tabloları**
   - Genel muhasebe (general ledger) kayıtları
   - Hesap bakiye tabloları
   - Muhasebe fişleri

3. **Harici switch/network sistem tabloları**
   - Ocean processor'ın kendi iç tabloları
   - Visa/Mastercard/BKM network tabloları
   - Switch routing tabloları

4. **Mobil kanal ekran verileri**
   - Mobil uygulama UI state
   - Push notification tabloları
   - Kullanıcı tercihleri

5. **Onboarding domain tabloları**
   - Müşteri kayıt akışı
   - KYC/AML tabloları
   - Kart başvuru süreçleri

> Bu domainlerden gelen veriler (OceanTxnGuid, RRN, ARN, ControlStat vb.) yalnızca **external reference field** olarak owned tablolarda tutulur. Bölüm 4'teki strateji bu kullanımı detaylandırır.

---

*Bu doküman Payify owned process tasarımının temelini oluşturur. Her bölüm backlog item'larına dönüştürülebilir niteliktedir.*

