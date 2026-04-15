# Payify Reporting Views — Kapsamlı Teknik Analiz Dokümanı

> **Kaynak Dosya:** `V1_0_4__ReportingViews.sql`  
> **Versiyon:** 1.0.4  
> **Tarih:** 15 Nisan 2026  
> **Analiz Kapsamı:** 23 adet PostgreSQL reporting view — dosya yükleme, mutabakat, eşleştirme, dispute izleme, arşiv denetim, yaşlanma analizi ve KPI özet view'ları  
> **Hedef Kitle:** Senior Backend Developer, Data Analyst, DevOps, İş Analisti

---

## İçindekiler

1. [Genel Amaç](#1-genel-amaç)
2. [View Listesi](#2-view-listesi)
3. [Her View İçin Ayrıntılı Teknik Analiz](#3-her-view-için-ayrıntılı-teknik-analiz)
   - [VIEW 1: vw_file_ingestion_summary](#view-1-reportingvw_file_ingestion_summary)
   - [VIEW 2: vw_reconciliation_file_summary](#view-2-reportingvw_reconciliation_file_summary)
   - [VIEW 3: vw_reconciliation_line_detail](#view-3-reportingvw_reconciliation_line_detail)
   - [VIEW 4: vw_reconciliation_unmatched_cards](#view-4-reportingvw_reconciliation_unmatched_cards)
   - [VIEW 5: vw_reconciliation_operation_tracker](#view-5-reportingvw_reconciliation_operation_tracker)
   - [VIEW 6: vw_reconciliation_pending_actions](#view-6-reportingvw_reconciliation_pending_actions)
   - [VIEW 7: vw_reconciliation_alert_dashboard](#view-7-reportingvw_reconciliation_alert_dashboard)
   - [VIEW 8: vw_daily_reconciliation_summary](#view-8-reportingvw_daily_reconciliation_summary)
   - [VIEW 9: vw_reconciliation_aging](#view-9-reportingvw_reconciliation_aging)
   - [VIEW 10: vw_archive_audit_trail](#view-10-reportingvw_archive_audit_trail)
   - [VIEW 11: vw_clearing_dispute_monitor](#view-11-reportingvw_clearing_dispute_monitor)
   - [VIEW 12: vw_base_card_transaction](#view-12-reportingvw_base_card_transaction)
   - [VIEW 13: vw_base_clearing_transaction](#view-13-reportingvw_base_clearing_transaction)
   - [VIEW 14: vw_reconciliation_matched_pair](#view-14-reportingvw_reconciliation_matched_pair)
   - [VIEW 15: vw_reconciliation_unmatched_card](#view-15-reportingvw_reconciliation_unmatched_card)
   - [VIEW 16: vw_reconciliation_amount_mismatch](#view-16-reportingvw_reconciliation_amount_mismatch)
   - [VIEW 17: vw_reconciliation_status_mismatch](#view-17-reportingvw_reconciliation_status_mismatch)
   - [VIEW 18: vw_reconciliation_clean_matched](#view-18-reportingvw_reconciliation_clean_matched)
   - [VIEW 19: vw_reconciliation_problem_records](#view-19-reportingvw_reconciliation_problem_records)
   - [VIEW 20: vw_reconciliation_summary_daily](#view-20-reportingvw_reconciliation_summary_daily)
   - [VIEW 21: vw_reconciliation_summary_by_network](#view-21-reportingvw_reconciliation_summary_by_network)
   - [VIEW 22: vw_reconciliation_summary_by_file](#view-22-reportingvw_reconciliation_summary_by_file)
   - [VIEW 23: vw_reconciliation_summary_overall](#view-23-reportingvw_reconciliation_summary_overall)
4. [Business Mantıkları](#4-business-mantıkları)
5. [Yorumlama Senaryoları](#5-yorumlama-senaryoları)
6. [Tablolar ve Bağımlılıklar](#6-tablolar-ve-bağımlılıklar)
7. [Veri Akışı ve Mimari Bakış](#7-veri-akışı-ve-mimari-bakış)
8. [Teknik Riskler ve Dikkat Edilmesi Gerekenler](#8-teknik-riskler-ve-dikkat-edilmesi-gerekenler)
9. [Özet Tablo](#9-özet-tablo)

---

## 1. Genel Amaç

### 1.1 Migration Dosyasının Amacı

Bu migration dosyası (`V1_0_4__ReportingViews.sql`), **Payify Kart Mutabakat Sistemi** için kapsamlı bir raporlama katmanı oluşturur. Dosya, `reporting` adlı ayrı bir PostgreSQL schema'sı altında **23 adet view** tanımlar. Bu view'lar, operasyonel tabloların üzerine inşa edilen salt-okunur (read-only) raporlama nesneleridir.

### 1.2 Çözdüğü İhtiyaçlar

| İhtiyaç | Çözüm |
|---------|-------|
| Dosya yükleme sürecinin izlenmesi | `vw_file_ingestion_summary` — dosya bazlı başarı oranı, satır sayaçları |
| Mutabakat sürecinin operasyonel takibi | `vw_reconciliation_file_summary`, `vw_reconciliation_line_detail` — dosya ve satır bazlı mutabakat durumu |
| Eşleşmemiş kayıtların tespiti | `vw_reconciliation_unmatched_cards`, `vw_reconciliation_unmatched_card` — clearing karşılığı bulunamayan kart işlemleri |
| Operasyon ve aksiyon takibi | `vw_reconciliation_operation_tracker`, `vw_reconciliation_pending_actions` — operasyon durumları, retry, lease, manuel onay |
| Uyarı ve alarm yönetimi | `vw_reconciliation_alert_dashboard` — severity, type, yaşlanma bilgileriyle alert panosu |
| Günlük performans ölçümü | `vw_daily_reconciliation_summary` — günlük dosya/content bazlı özet metrikleri |
| Yaşlanma analizi | `vw_reconciliation_aging` — açık kayıtların yaş bucket'larına dağılımı |
| Arşiv denetim izi | `vw_archive_audit_trail` — arşivleme sürecinin audit logu |
| Dispute izleme | `vw_clearing_dispute_monitor` — Visa/Mastercard/BKM dispute ve anormal clearing kayıtları |
| Kart-clearing veri normalizasyonu | `vw_base_card_transaction`, `vw_base_clearing_transaction` — üç ağın verilerini tekil bir yapıya dönüştürme |
| Eşleştirme ve uyuşmazlık tespiti | `vw_reconciliation_matched_pair` — kart vs clearing yan yana karşılaştırma, mismatch flag'leri |
| İş senaryolarına özel filtreleme | `vw_reconciliation_amount_mismatch`, `vw_reconciliation_status_mismatch`, `vw_reconciliation_clean_matched`, `vw_reconciliation_problem_records` |
| KPI ve dashboard özeti | `vw_reconciliation_summary_daily/by_network/by_file/overall` — çeşitli kırılımlarda mutabakat başarı göstergeleri |

### 1.3 View'ların Sistem İçindeki Rolü

View'lar **raporlama API'lerinin doğrudan veri kaynağıdır**. Uygulama katmanındaki `ReportingController` endpoint'leri bu view'lardan okuma yapar. View'lar aynı zamanda:

- Operasyonel tabloların karmaşık join yapılarını soyutlar
- Business rule'ları SQL seviyesinde merkezi olarak uygular
- Dashboard ve KPI hesaplamalarını veritabanı seviyesinde gerçekleştirir
- Üst katman view'lar alt katman view'lara bağımlıdır → katmanlı mimari

---

## 2. View Listesi

| # | Tam Ad | Katman | Bağımlı Tablolar / View'lar |
|---|--------|--------|----------------------------|
| 1 | `reporting.vw_file_ingestion_summary` | Operasyonel | `ingestion.file` |
| 2 | `reporting.vw_reconciliation_file_summary` | Operasyonel | `ingestion.file`, `ingestion.file_line` |
| 3 | `reporting.vw_reconciliation_line_detail` | Operasyonel | `ingestion.file_line`, `ingestion.file`, `reconciliation.evaluation`, `reconciliation.operation` |
| 4 | `reporting.vw_reconciliation_unmatched_cards` | Operasyonel | `ingestion.file_line`, `ingestion.file` |
| 5 | `reporting.vw_reconciliation_operation_tracker` | Operasyonel | `reconciliation.operation`, `reconciliation.evaluation`, `ingestion.file_line`, `ingestion.file`, `reconciliation.operation_execution` |
| 6 | `reporting.vw_reconciliation_pending_actions` | Operasyonel | `reconciliation.operation`, `ingestion.file_line`, `ingestion.file`, `reconciliation.review` |
| 7 | `reporting.vw_reconciliation_alert_dashboard` | Operasyonel | `reconciliation.alert`, `reconciliation.operation`, `ingestion.file_line`, `ingestion.file` |
| 8 | `reporting.vw_daily_reconciliation_summary` | Operasyonel | `ingestion.file`, `ingestion.file_line` |
| 9 | `reporting.vw_reconciliation_aging` | Operasyonel | `ingestion.file_line`, `ingestion.file` |
| 10 | `reporting.vw_archive_audit_trail` | Operasyonel | `archive.archive_log`, `archive.ingestion_file` |
| 11 | `reporting.vw_clearing_dispute_monitor` | Operasyonel | `ingestion.clearing_visa_detail`, `ingestion.clearing_msc_detail`, `ingestion.clearing_bkm_detail`, `ingestion.file_line`, `ingestion.file` |
| 12 | `reporting.vw_base_card_transaction` | Temel (Base) | `ingestion.card_bkm_detail`, `ingestion.card_visa_detail`, `ingestion.card_msc_detail`, `ingestion.file_line`, `ingestion.file` |
| 13 | `reporting.vw_base_clearing_transaction` | Temel (Base) | `ingestion.clearing_bkm_detail`, `ingestion.clearing_visa_detail`, `ingestion.clearing_msc_detail`, `ingestion.file_line`, `ingestion.file` |
| 14 | `reporting.vw_reconciliation_matched_pair` | Eşleştirme | **VIEW:** `reporting.vw_base_card_transaction`, `reporting.vw_base_clearing_transaction` |
| 15 | `reporting.vw_reconciliation_unmatched_card` | İş (Business) | **VIEW:** `reporting.vw_reconciliation_matched_pair` |
| 16 | `reporting.vw_reconciliation_amount_mismatch` | İş (Business) | **VIEW:** `reporting.vw_reconciliation_matched_pair` |
| 17 | `reporting.vw_reconciliation_status_mismatch` | İş (Business) | **VIEW:** `reporting.vw_reconciliation_matched_pair` |
| 18 | `reporting.vw_reconciliation_clean_matched` | İş (Business) | **VIEW:** `reporting.vw_reconciliation_matched_pair` |
| 19 | `reporting.vw_reconciliation_problem_records` | İş (Business) | **VIEW:** `reporting.vw_reconciliation_matched_pair` |
| 20 | `reporting.vw_reconciliation_summary_daily` | Özet (Summary) | **VIEW:** `reporting.vw_reconciliation_matched_pair` |
| 21 | `reporting.vw_reconciliation_summary_by_network` | Özet (Summary) | **VIEW:** `reporting.vw_reconciliation_matched_pair` |
| 22 | `reporting.vw_reconciliation_summary_by_file` | Özet (Summary) | **VIEW:** `reporting.vw_reconciliation_matched_pair` |
| 23 | `reporting.vw_reconciliation_summary_overall` | Özet (Summary) | **VIEW:** `reporting.vw_reconciliation_matched_pair` |

---

## 3. Her View İçin Ayrıntılı Teknik Analiz

---

### VIEW 1: `reporting.vw_file_ingestion_summary`

#### Amaç
Sisteme alınan her dosyanın işlenme özetini tek satırda gösterir. Dosya yükleme sürecinin sağlık kontrolü için kullanılır.

#### Business İhtiyacı
Operatörün "bu dosya başarıyla yüklendi mi?" sorusuna anında cevap vermesini sağlar. Dosya bazlı başarı oranı ve sayaç uyuşmazlığı tespiti yapar.

#### Kaynak Tablolar ve Kolonlar

| Tablo | Kolon | Alias | Anlam |
|-------|-------|-------|-------|
| `ingestion.file` | `id` | `file_id` | Dosya PK |
| | `file_name` | `file_name` | Fiziksel dosya adı |
| | `file_key` | `file_key` | Header+footer hash — dosya parmak izi |
| | `source_type` | `source_type` | Remote/Local kaynak türü |
| | `file_type` | `file_type` | Card/Clearing |
| | `content_type` | `content_type` | BKM/Visa/MSC |
| | `status` | `status` | Dosya durumu |
| | `expected_line_count` | `expected_count` | Footer'dan okunan beklenen satır sayısı |
| | `processed_line_count` | `processed_count` | İşlenen satır sayısı |
| | `successful_line_count` | `success_count` | Başarıyla parse edilen satır sayısı |
| | `failed_line_count` | `error_count` | Parse hatası alan satır sayısı |
| | `is_archived` | `is_archived` | Arşivlenme durumu |
| | `create_date` | `ingested_at` | Yükleme zamanı |
| | `update_date` | `last_updated_at` | Son güncelleme zamanı |
| | `created_by` | `created_by` | Yükleyen kullanıcı/sistem |
| | `record_status` | `record_status` | Aktif/pasif kayıt durumu |

#### Join Yapısı
Tek tablodan okuma, join yok.

#### Hesaplanan Alanlar

**`success_rate`:**
```sql
CASE
    WHEN f.processed_line_count > 0
    THEN ROUND((f.successful_line_count::numeric / f.processed_line_count) * 100, 2)
    ELSE 0
END
```
- **İş anlamı:** İşlenen satırların yüzde kaçı başarıyla parse edilmiş.
- **Sıfıra bölme koruması:** `processed_line_count = 0` ise 0 döner.
- **`::numeric` cast:** PostgreSQL'de integer bölme yapılmaması için decimal cast.

**`has_count_mismatch`:**
```sql
CASE
    WHEN f.expected_line_count > 0 AND f.processed_line_count <> f.expected_line_count
    THEN true ELSE false
END
```
- **İş anlamı:** Footer'daki beklenen satır sayısı ile işlenen satır sayısı tutmuyor mu?
- **Neden önemli:** `true` değeri dosyanın eksik veya bozuk alındığını gösterir — truncation, iletişim hatası, ya da format sorunu.
- **Edge case:** `expected_line_count = 0` ise (footer yoksa veya okunamadıysa) `false` döner — bu durumda kontrol devre dışı kalır.

#### Null Handling
- `expected_line_count` NULL olabilir. NULL durumunda `NULL > 0` → `false` → `has_count_mismatch = false`. Yani beklenen sayı yoksa mismatch kontrolü yapılmaz.
- Tüm sayaç alanları `0` default'una sahip olmalı (varsayım: uygulama katmanı 0 ile initialize ediyor).

#### Performans Notları
- Tek tablodan direkt okuma — performans sorunu beklenmez.
- `file` tablosu genellikle düşük kardinaliteli (binler mertebesinde).

---

### VIEW 2: `reporting.vw_reconciliation_file_summary`

#### Amaç
Her dosyanın mutabakat satırlarının durum dağılımını gösterir. Dosya bazlı reconciliation pipeline durumunu özetler.

#### Business İhtiyacı
Bir dosyanın mutabakat sürecinde nerede olduğunu anlamak: kaç satır hazır, kaç tanesi işleniyor, kaç tanesi başarılı/başarısız? Eşleşme ve duplikat durumu ne?

#### Kaynak Tablolar

| Tablo | Rolü |
|-------|------|
| `ingestion.file` | Ana dosya bilgileri |
| `ingestion.file_line` | Satır bazlı mutabakat durumu |

#### Join Yapısı
```sql
LEFT JOIN ingestion.file_line fl ON fl.file_id = f.id
```
- **LEFT JOIN:** Henüz satırı olmayan dosyalar da görünür (dosya kaydı oluşturulmuş ama parse henüz başlamamış).

#### Gruplama
```sql
GROUP BY f.id, f.file_name, f.file_type, f.content_type, f.status, f.is_archived, f.create_date
```
Dosya bazlı gruplama — her dosya bir satır.

#### Aggregation ve FILTER Yapısı

Bu view, PostgreSQL'in `COUNT() FILTER (WHERE ...)` sözdizimini yoğun olarak kullanır. Bu, tek bir `GROUP BY` pass'inde birden fazla conditional count hesaplar.

| Kolon | Filtre | İş Anlamı |
|-------|--------|-----------|
| `total_detail_lines` | `line_type = 'D'` | Header/Footer hariç detay satır sayısı |
| `recon_ready_count` | `line_type = 'D' AND reconciliation_status = 'Ready'` | Değerlendirilmeyi bekleyen satır sayısı |
| `recon_processing_count` | `line_type = 'D' AND reconciliation_status = 'Processing'` | Aktif olarak işlenen satır sayısı |
| `recon_success_count` | `line_type = 'D' AND reconciliation_status = 'Success'` | Başarıyla değerlendirilmiş satır sayısı |
| `recon_failed_count` | `line_type = 'D' AND reconciliation_status = 'Failed'` | Değerlendirmesi başarısız satır sayısı |
| `recon_pending_count` | `line_type = 'D' AND reconciliation_status IS NULL` | Henüz durumu atanmamış satır sayısı |
| `matched_count` | `line_type = 'D' AND matched_clearing_line_id IS NOT NULL` | Clearing ile eşleşmiş satır sayısı |
| `unmatched_card_count` | `line_type = 'D' AND matched_clearing_line_id IS NULL AND f.file_type = 'Card'` | Eşleşmemiş kart satır sayısı |
| `duplicate_count` | `line_type = 'D' AND duplicate_status IS NOT NULL AND duplicate_status NOT IN ('Unique')` | Duplikat tespit edilen satır sayısı |

**Kritik filtre:** `line_type = 'D'` — yalnızca Detail (detay) satırlarını sayar; Header ('H') ve Footer ('F') satırlarını dahil etmez. Tüm metriklerin temelinde bu filtre bulunur.

#### Hesaplanan Alan: `recon_completion_rate`
```sql
CASE
    WHEN COUNT(fl.id) FILTER (WHERE fl.line_type = 'D') > 0
    THEN ROUND(
        (COUNT(fl.id) FILTER (WHERE fl.line_type = 'D' AND fl.reconciliation_status = 'Success')::numeric
         / COUNT(fl.id) FILTER (WHERE fl.line_type = 'D')) * 100, 2)
    ELSE 0
END
```
- **İş anlamı:** Detay satırlarının yüzde kaçının mutabakatı başarıyla tamamlanmış.
- **Sıfıra bölme koruması:** Detay satır yoksa 0 döner.

#### Null Handling
- `reconciliation_status IS NULL` → `recon_pending_count`: Bu durum, satır parse edilmiş ama korelasyon anahtarı atanamadığı veya ingestion henüz tamamlanmadığı anlamına gelebilir.
- `duplicate_status NOT IN ('Unique')` → `NULL` değerler bu koşulu sağlamaz (SQL'de `NULL NOT IN (...)` → `NULL` → sayılmaz). Dolayısıyla `duplicate_status IS NOT NULL` ön koşulu gereklidir.

#### Performans Notu
- `file_line` yüksek kardinaliteli tablo (milyonlarca satır olabilir). `LEFT JOIN + GROUP BY` üzerinden çalışan bu view, büyük veri kümelerinde **ağır** olabilir.
- `file_line(file_id, line_type, reconciliation_status)` üzerinde composite index önerilir.

---

### VIEW 3: `reporting.vw_reconciliation_line_detail`

#### Amaç
Her dosya satırı için evaluation ve operation aggregation'larını tek satırda sunar. Satır bazlı detaylı analiz sağlar.

#### Business İhtiyacı
Tek bir dosya satırının tüm yaşam döngüsünü görmek: Kaç kez değerlendirildi? Son değerlendirme ne dedi? Kaç operasyonu var, kaçı bekliyor, kaçı başarısız?

#### Kaynak Tablolar

| Tablo | Rolü |
|-------|------|
| `ingestion.file_line` | Ana satır bilgileri |
| `ingestion.file` | Dosya metadata |
| `reconciliation.evaluation` | Değerlendirme bilgileri (LATERAL subquery) |
| `reconciliation.operation` | Operasyon durumları (LATERAL subquery) |

#### Join Yapısı
```sql
JOIN ingestion.file f ON f.id = fl.file_id
LEFT JOIN LATERAL (...) eval_agg ON true
LEFT JOIN LATERAL (...) op_agg ON true
```
- **INNER JOIN** ile dosya bağlanır (her satırın mutlaka bir dosyası olmalı).
- **LEFT JOIN LATERAL** ile evaluation ve operation aggregate'leri ayrı subquery'lerle hesaplanır. `LATERAL` sayesinde her satır için bağımsız olarak hesaplama yapılır.

#### LATERAL Subquery: Evaluation Aggregates
```sql
SELECT
    COUNT(*)                            AS evaluation_count,
    (SELECT e2.status FROM reconciliation.evaluation e2
     WHERE e2.file_line_id = fl.id
     ORDER BY e2.create_date DESC LIMIT 1)  AS latest_evaluation_status,
    (SELECT e2.message FROM reconciliation.evaluation e2
     WHERE e2.file_line_id = fl.id
     ORDER BY e2.create_date DESC LIMIT 1)  AS latest_evaluation_message
FROM reconciliation.evaluation e
WHERE e.file_line_id = fl.id
```
- **evaluation_count:** Bu satır için kaç değerlendirme yapılmış (retry dahil).
- **latest_evaluation_status/message:** En son değerlendirmenin durumu. `ORDER BY create_date DESC LIMIT 1` ile en güncel alınır.
- **İç içe subquery:** COUNT ile LIMIT 1 aynı LATERAL bloğunda, ama status/message iç subquery ile çekiliyor. **Bu yapı performans açısından riskli** — aynı tabloyu 3 kez tarıyor.

#### LATERAL Subquery: Operation Aggregates
```sql
SELECT
    COUNT(*)                                                    AS operation_count,
    COUNT(*) FILTER (WHERE o.status IN ('Planned','Blocked','Executing'))  AS pending_operation_count,
    COUNT(*) FILTER (WHERE o.status = 'Failed')                 AS failed_operation_count,
    COUNT(*) FILTER (WHERE o.status = 'Completed')              AS completed_operation_count,
    COUNT(*) FILTER (WHERE o.is_manual = true)                  AS manual_operation_count
FROM reconciliation.operation o
WHERE o.file_line_id = fl.id
```

#### Hesaplanan Alan: `age_days`
```sql
EXTRACT(DAY FROM (NOW() - fl.create_date))::int
```
- Satırın oluşturulmasından bu yana geçen gün sayısı.
- **Dikkat:** `EXTRACT(DAY FROM interval)` toplam gün sayısını verir, ama negatif interval'larda beklenmedik sonuç üretebilir (varsayım: `create_date` her zaman geçmişte).

#### Filtre Koşulu
```sql
WHERE fl.line_type = 'D'
```
Yalnızca detay satırlarını içerir — header/footer hariç tutulur.

#### `has_match` Türetilen Kolon
```sql
CASE WHEN fl.matched_clearing_line_id IS NOT NULL THEN true ELSE false END
```
Boolean olarak eşleşme durumu.

#### Null Handling
- `eval_agg` NULL dönebilir (hiç evaluation yoksa) → `evaluation_count = NULL`. Bu durumda tüm eval_agg alanları NULL olur.
- `op_agg` benzer şekilde NULL dönebilir → `operation_count = NULL`.
- **Not:** `LEFT JOIN LATERAL` + `COUNT(*)` kullanımında, eğer hiç satır eşleşmezse LATERAL subquery'den tek satır döner ve `COUNT(*) = 0` olur — NULL değil. Ama iç subquery'ler (latest status/message) NULL dönecektir.

#### Performans Notu
- **LATERAL subquery'ler satır başına çalışır.** `file_line` tablosunda 1 milyon satır varsa, evaluation ve operation tablolarına satır başına 2 ayrı subquery gider. Bu **N+1 benzeri** bir pattern olup, büyük veri kümelerinde ciddi performans sorunu yaratır.
- `reconciliation.evaluation(file_line_id, create_date)` ve `reconciliation.operation(file_line_id, status)` üzerinde index şarttır.
- **Sayfalama:** Bu view mutlaka `LIMIT/OFFSET` ile kullanılmalı, `SELECT *` yapılmamalı.

---

### VIEW 4: `reporting.vw_reconciliation_unmatched_cards`

#### Amaç
Clearing karşılığı bulunamayan kart satırlarını listeler. Bu, ingestion seviyesindeki eşleşmeme takibi içindir.

#### Business İhtiyacı
Hangi kart işlemlerinin henüz clearing dosyasıyla eşleşmediğini tespit etmek. Clearing gecikmeleri veya kayıp işlemleri izlemek.

#### Filtre Koşulları
```sql
WHERE f.file_type = 'Card'
  AND fl.line_type = 'D'
  AND fl.matched_clearing_line_id IS NULL
  AND fl.status = 'Success'
```
- Yalnızca **Card** tipi dosyalar (Clearing dosyasının eşleşmemesi burada izlenmez).
- Yalnızca **Detail** satırları.
- `matched_clearing_line_id IS NULL` → Clearing ile eşleşmemiş.
- `fl.status = 'Success'` → Parse hatası olan satırlar hariç; yalnızca başarıyla okunan ama eşleşemeyen satırlar.

#### Hesaplanan Alan: `age_days`
Satırın kaç gündür eşleşmemiş olduğunu gösterir. 3+ gün → uyarı, 7+ gün → kritik.

#### Ayrım: VIEW 4 vs VIEW 15
- **VIEW 4** (`vw_reconciliation_unmatched_cards`): `ingestion.file_line` tablosundan doğrudan çeker, `fl.status = 'Success'` filtresi var.
- **VIEW 15** (`vw_reconciliation_unmatched_card`): `vw_reconciliation_matched_pair` view'ından çeker, `match_status = 'UNMATCHED_CARD'` filtresi var.
- İkisi farklı katmanlardadır; VIEW 4 operasyonel, VIEW 15 ise eşleştirme view'ı üzerine kuruludur.

---

### VIEW 5: `reporting.vw_reconciliation_operation_tracker`

#### Amaç
Her operasyonun detaylı yürütme durumunu, en son execution bilgisini ve toplam deneme sayısını gösterir.

#### Business İhtiyacı
Operasyon takip ekranı: Hangi operasyon hangi durumda? Kaç kez denendi? Son denemenin sonucu ne? Lease kime ait?

#### Kaynak Tablolar

| Tablo | Rolü |
|-------|------|
| `reconciliation.operation` | Ana operasyon bilgileri |
| `reconciliation.evaluation` | Bağlı evaluation durumu |
| `ingestion.file_line` | Bağlı dosya satırı |
| `ingestion.file` | Dosya metadata |
| `reconciliation.operation_execution` | Yürütme denemeleri (LATERAL) |

#### Join Yapısı
```sql
JOIN reconciliation.evaluation e ON e.id = o.evaluation_id
JOIN ingestion.file_line fl ON fl.id = o.file_line_id
JOIN ingestion.file f ON f.id = fl.file_id
LEFT JOIN LATERAL (...) latest_exec ON true   -- En son yürütme
LEFT JOIN LATERAL (...) exec_count ON true    -- Toplam yürütme sayısı
```

#### LATERAL: Latest Execution
```sql
SELECT ex.attempt_number, ex.status, ex.result_code, ex.result_message,
       ex.error_code, ex.error_message, ex.started_at, ex.finished_at
FROM reconciliation.operation_execution ex
WHERE ex.operation_id = o.id
ORDER BY ex.attempt_number DESC LIMIT 1
```
En son yürütme denemesinin tüm detayları.

#### LATERAL: Execution Count
```sql
SELECT COUNT(*) AS total_executions
FROM reconciliation.operation_execution ex2
WHERE ex2.operation_id = o.id
```

#### Hesaplanan Alan: `age_hours`
```sql
EXTRACT(EPOCH FROM (NOW() - o.create_date)) / 3600
```
- Operasyonun oluşturulmasından bu yana geçen saat sayısı.
- `EXTRACT(EPOCH ...)` saniye cinsinden döner, 3600'e bölünerek saate çevrilir.
- **Kullanım:** Uzun süredir tamamlanmayan operasyonları tespit etmek.

#### Performans Notu
- 4 INNER JOIN + 2 LATERAL subquery. Operasyon tablosu büyüdükçe performans düşer.
- `operation_execution(operation_id, attempt_number)` index'i kritik.

---

### VIEW 6: `reporting.vw_reconciliation_pending_actions`

#### Amaç
Tamamlanmamış (bekleyen, bloklanmış, yürütülen, başarısız) operasyonları ve varsa bekleyen manuel onay bilgilerini gösterir.

#### Business İhtiyacı
Operatörün "şu an ne bekliyor?" sorusuna cevap verir. Manuel review'ların süre dolma durumunu takip eder.

#### Filtre Koşulu
```sql
WHERE o.status IN ('Planned', 'Blocked', 'Executing', 'Failed')
```
Yalnızca terminal olmayan operasyonlar (Completed ve Cancelled hariç).

#### Join: Review Bilgisi
```sql
LEFT JOIN reconciliation.review r ON r.operation_id = o.id AND r.decision = 'Pending'
```
- **LEFT JOIN:** Her operasyonun review'ı olmayabilir (otomatik operasyonlar).
- `r.decision = 'Pending'` filtresi: Yalnızca henüz karara bağlanmamış review'ları getirir. Karara bağlanmışlar (Approved/Rejected/Cancelled) join dışı kalır.

#### Hesaplanan Alan: `is_review_expired`
```sql
CASE
    WHEN r.expires_at IS NOT NULL AND r.expires_at < NOW() AND r.decision = 'Pending'
    THEN true ELSE false
END
```
- **İş anlamı:** Süresi dolmuş ve henüz karara bağlanmamış review.
- **Neden önemli:** Bu `true` ise, sonraki Execute çalıştırmasında otomatik karar (ExpirationAction) uygulanacak.
- **Edge case:** `expires_at IS NULL` → süre sınırı yok → `false`. Karar zaten verilmişse join'dan düşer.

#### Hesaplanan Alan: `waiting_hours`
```sql
EXTRACT(EPOCH FROM (NOW() - o.create_date)) / 3600
```
Operasyonun kaç saattir beklediği.

---

### VIEW 7: `reporting.vw_reconciliation_alert_dashboard`

#### Amaç
Tüm mutabakat uyarılarını dosya ve operasyon bilgileriyle zenginleştirilmiş olarak gösterir.

#### Business İhtiyacı
Uyarı panosu: Hangi uyarılar var? Hangi severity'de? Hangi dosya ve operasyonla ilişkili? Ne kadar süredir açık?

#### Join Yapısı
```sql
JOIN reconciliation.operation o ON o.id = a.operation_id
JOIN ingestion.file_line fl ON fl.id = a.file_line_id
JOIN ingestion.file f ON f.id = fl.file_id
```
Tümü **INNER JOIN** — yalnızca operasyonu, satırı ve dosyası olan alertler görünür. Bağımlılığı eksik olan alert kayıtları (orphan) bu view'da görünmez.

#### Hesaplanan Alan: `age_hours`
Uyarının kaç saattir açık olduğu.

#### Dikkat
- `INNER JOIN` kullanımı: `a.operation_id` veya `a.file_line_id` NULL ise o alert bu view'da görünmez.
- Evaluation-level alertler (`operation_id = Guid.Empty`) bu view'da **görünmeyebilir** — `INNER JOIN operation` filtresi nedeniyle. Bu bir tasarım kararı veya potansiyel eksikliktir.

---

### VIEW 8: `reporting.vw_daily_reconciliation_summary`

#### Amaç
Dosyanın yüklenme tarihine göre günlük mutabakat özeti üretir.

#### Business İhtiyacı
Günlük operasyonel dashboard: O gün yüklenen dosyaların toplam satır sayısı, başarı/başarısızlık dağılımı, eşleşme oranı.

#### Gruplama
```sql
GROUP BY f.create_date::date, f.content_type, f.file_type
```
- `f.create_date::date` → timestamp'ten date'e çevirim. Aynı günün tüm dosyaları birleştirilir.
- `content_type` (BKM/Visa/MSC) ve `file_type` (Card/Clearing) kırılımları korunur.

#### Aggregation
VIEW 2 ile benzer `COUNT() FILTER (WHERE ...)` pattern'i. Ek olarak:

| Kolon | Anlam |
|-------|-------|
| `total_files` | `COUNT(DISTINCT f.id)` — o gün yüklenen benzersiz dosya sayısı |
| `recon_not_evaluated_count` | `reconciliation_status IS NULL` — hiç değerlendirilmemiş |
| `success_rate` | Başarılı mutabakat yüzdesi |

#### Performans Notu
- `f.create_date::date` üzerinde cast uygulanır. Bu, `create_date` üzerindeki index'in kullanılmasını engeller (function-based index yoksa).
- `ingestion.file(create_date)` üzerinde `CREATE INDEX ON ingestion.file ((create_date::date))` eklenebilir.

---

### VIEW 9: `reporting.vw_reconciliation_aging`

#### Amaç
Açık (tamamlanmamış) mutabakat satırlarını yaş aralıklarına (bucket) dağıtarak yaşlanma analizi yapar.

#### Business İhtiyacı
SLA takibi: Kaç kayıt ne kadar süredir çözümsüz? Birikim artıyor mu? Hangi content_type'ta yaşlanma fazla?

#### Age Bucket Mantığı
```sql
CASE
    WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 1  THEN '0-1 Gün'
    WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 3  THEN '2-3 Gün'
    WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 7  THEN '4-7 Gün'
    WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 14 THEN '8-14 Gün'
    WHEN EXTRACT(DAY FROM (NOW() - fl.create_date)) <= 30 THEN '15-30 Gün'
    ELSE '30+ Gün'
END AS age_bucket
```
- 6 yaş aralığı tanımlı.
- `age_bucket_order` (1-6) sıralama için sayısal karşılık.

#### Filtre Koşulları
```sql
WHERE fl.line_type = 'D'
  AND fl.reconciliation_status IN ('Ready', 'Processing', 'Failed')
  AND f.is_archived = false
```
- **Yalnızca açık kayıtlar:** Success olan (tamamlanmış) ve arşivlenmiş kayıtlar hariç tutulur.
- `Failed` dahil: Başarısız olan ama henüz arşivlenmemiş kayıtlar da yaşlanma analizine girer.
- `is_archived = false`: Arşivlenmiş dosyaların satırları sayılmaz.

#### Gruplama
```sql
GROUP BY age_bucket, age_bucket_order, f.content_type, f.file_type
```

#### Aggregation
| Kolon | Anlam |
|-------|-------|
| `open_count` | Bu bucket'taki açık kayıt sayısı |
| `oldest_record_date` | En eski kayıt tarihi |
| `newest_record_date` | En yeni kayıt tarihi |

#### Edge Case
- `EXTRACT(DAY ...)` fonksiyonu — günlük tam gün hesaplar. 1 gün 23 saat geçmiş kayıt `<= 1` bucket'ına girer. Tam 1 gün geçmiş de aynı şekilde.

---

### VIEW 10: `reporting.vw_archive_audit_trail`

#### Amaç
Arşivleme işlemlerinin denetim izini gösterir.

#### Business İhtiyacı
Arşiv süreci takibi: Hangi dosya ne zaman arşivlendi? Kim arşivledi? Kaç gün sonra arşivlendi? Başarısız arşivlerin nedeni ne?

#### Kaynak Tablolar

| Tablo | Rolü |
|-------|------|
| `archive.archive_log` | Arşiv işlem logu |
| `archive.ingestion_file` | Arşivlenmiş dosya snapshot'ı |

#### Join Yapısı
```sql
LEFT JOIN archive.ingestion_file af ON af.id = al.ingestion_file_id
```
- **LEFT JOIN:** Arşiv logu var ama dosya snapshot'ı yoksa (orphan log) da görünür.

#### Hesaplanan Alan: `days_to_archive`
```sql
EXTRACT(DAY FROM (al.create_date - af.create_date))::int
```
- Dosyanın yüklenmesinden arşivlenmesine kadar geçen gün sayısı.
- **İş anlamı:** Arşiv SLA ölçümü. Örneğin 90 gün retention policy varsa, `days_to_archive` 90'ın altında veya üstünde olabilir.
- **NULL riski:** `af.create_date` NULL ise (LEFT JOIN — dosya bulunamadıysa) → `days_to_archive = NULL`.

---

### VIEW 11: `reporting.vw_clearing_dispute_monitor`

#### Amaç
Üç kart ağının (Visa, Mastercard, BKM) clearing dosyalarındaki dispute ve anormal kayıtları birleştirir.

#### Business İhtiyacı
Dispute/chargeback izleme: Normal olmayan clearing kayıtlarını merkezi bir noktadan izlemek. Üç ağın verilerini normalize ederek karşılaştırılabilir hale getirmek.

#### Yapı: UNION ALL (3 Parça)

**Visa:**
```sql
FROM ingestion.clearing_visa_detail cvd
JOIN ingestion.file_line fl ON fl.id = cvd.file_line_id
JOIN ingestion.file f ON f.id = fl.file_id
WHERE cvd.control_stat <> 'Normal' OR cvd.dispute_code <> 'None'
```

**Mastercard:**
```sql
FROM ingestion.clearing_msc_detail cmd
-- ... aynı join yapısı ...
WHERE cmd.control_stat <> 'Normal' OR cmd.dispute_code <> 'None'
```

**BKM:**
```sql
FROM ingestion.clearing_bkm_detail cbd
-- ... aynı join yapısı ...
WHERE cbd.control_stat <> 'Normal' OR cbd.dispute_code <> 'None'
```

#### Filtre Mantığı (Business Rule)
```sql
WHERE control_stat <> 'Normal' OR dispute_code <> 'None'
```
- **`control_stat <> 'Normal'`:** Normal dışı herhangi bir kontrol durumu (chargeback, representment, vb.)
- **`dispute_code <> 'None'`:** Dispute kodu atanmış olan kayıtlar.
- **OR koşulu:** Herhangi biri anormal ise kayıt dahil olur.
- **İş anlamı:** Bu view yalnızca "sorunlu" clearing kayıtlarını gösterir; normal işlemler filtre dışı kalır.

#### `network` Kolonu
Her UNION bloğunda sabit string: `'Visa'`, `'Mastercard'`, `'BKM'`.

#### Null Handling
- `control_stat` veya `dispute_code` NULL ise: `NULL <> 'Normal'` → `NULL` (false olarak değerlendirilir SQL'de). Yani NULL'lar **dahil edilmez**. Bu, potansiyel bir risk — NULL dispute_code aslında belirsiz durumu gösterebilir.

---

### VIEW 12: `reporting.vw_base_card_transaction`

#### Amaç
BKM, Visa ve Mastercard kart işlem detaylarını normalize ederek tek bir unified yapıya dönüştürür.

#### Business İhtiyacı
Üç farklı kart ağının farklı tablo yapılarındaki verileri, ortak bir kolon seti üzerinden sorgulanabilir hale getirmek. Bu view, üst katman view'ların temelini oluşturur.

#### Yapı: UNION ALL (3 Parça)

| Blok | Kaynak Tablo | Network Label |
|------|-------------|---------------|
| BKM | `ingestion.card_bkm_detail` + `file_line` + `file` | `'BKM'::text` |
| VISA | `ingestion.card_visa_detail` + `file_line` + `file` | `'VISA'::text` |
| MSC | `ingestion.card_msc_detail` + `file_line` + `file` | `'MSC'::text` |

#### Normalize Edilen Kolonlar (Her Blokta Ortak)

**Tanımlayıcılar:**
`file_id`, `file_name`, `file_line_id`, `network`

**İşlem referansları:**
`ocean_txn_guid`, `ocean_main_txn_guid`, `rrn`, `arn`, `provision_code`, `card_no`

**Üye iş yeri:**
`merchant_name`, `merchant_city`, `merchant_country`, `mcc`

**Tarihler:**
`transaction_date`, `transaction_time`, `value_date`, `end_of_day_date`

**Tutarlar:**
`original_amount`, `original_currency`, `settlement_amount`, `settlement_currency`, `billing_amount`, `billing_currency`, `tax1`, `tax2`, `cashback_amount`, `surcharge_amount`

**Durum:**
`txn_stat`, `response_code`, `is_successful_txn`, `is_txn_settle`

**Mutabakat:**
`reconciliation_status`, `duplicate_status`, `duplicate_group_id`, `matched_clearing_line_id`

**Audit:**
`create_date`, `record_status`

#### Join Yapısı
```sql
INNER JOIN ingestion.file_line fl ON fl.id = cbd.file_line_id
INNER JOIN ingestion.file f ON f.id = fl.file_id
```
**INNER JOIN** — yalnızca hem satırı hem dosyası olan detaylar. Orphan kayıtlar görünmez.

#### Performans Notu
- 3 büyük tablonun UNION ALL'ı — tablo boyutlarına göre ağır olabilir.
- Her tablodan tüm satırlar çekilir (WHERE yok). Bu view, üzerine inşa edilen view'lar tarafından filtreleneceği varsayılır.
- Materialized view veya indeks stratejisi değerlendirilmeli.

---

### VIEW 13: `reporting.vw_base_clearing_transaction`

#### Amaç
BKM, Visa ve Mastercard clearing (takas) işlem detaylarını normalize ederek tek bir yapıya dönüştürür.

#### Yapı
VIEW 12 ile aynı pattern — 3 UNION ALL bloğu.

| Blok | Kaynak Tablo | Network Label |
|------|-------------|---------------|
| BKM | `ingestion.clearing_bkm_detail` + `file_line` + `file` | `'BKM'::text` |
| VISA | `ingestion.clearing_visa_detail` + `file_line` + `file` | `'VISA'::text` |
| MSC | `ingestion.clearing_msc_detail` + `file_line` + `file` | `'MSC'::text` |

#### Normalize Edilen Kolonlar (Clearing'e Özgü)

**İşlem referansları:**
`ocean_txn_guid`, `rrn`, `arn`, `provision_code`, `card_no`

**İşlem tipi:**
`txn_type`, `io_flag`, `control_stat`, `dispute_code`

**Tutarlar:**
`source_amount`, `source_currency`, `destination_amount`, `destination_currency`, `cashback_amount`, `reimbursement_amount`

**Üye iş yeri:**
`merchant_name`, `merchant_city`

**Tarihler:**
`txn_date`, `txn_time`

**Mutabakat:**
`reconciliation_status`, `matched_clearing_line_id`

**Audit:**
`create_date`, `record_status`

#### Kart vs Clearing Kolon Farkları
- Clearing'de `ocean_main_txn_guid` yok (kart tarafına özgü).
- Clearing'de `settlement_amount/billing_amount` yerine `source_amount/destination_amount` var.
- Clearing'de `txn_type`, `io_flag`, `control_stat`, `dispute_code` var (kart tarafında yok).
- Clearing'de `reimbursement_amount` var.

---

### VIEW 14: `reporting.vw_reconciliation_matched_pair`

#### Amaç
Kart işlemlerini clearing işlemleriyle yan yana getirir; eşleşme durumunu ve uyuşmazlık flag'lerini hesaplar. **Tüm üst katman iş ve özet view'larının temelidir.**

#### Business İhtiyacı
"Bu kart işleminin clearing karşılığı var mı? Varsa tutarlar tutuyor mu? Para birimleri aynı mı? Tarihler uyuşuyor mu? Durumlar çelişiyor mu?" sorularına merkezi cevap.

#### Join Yapısı
```sql
FROM reporting.vw_base_card_transaction c
LEFT JOIN reporting.vw_base_clearing_transaction clr
    ON c.matched_clearing_line_id = clr.file_line_id
```
- **VIEW üzerine VIEW join:** İki base view birleştirilir.
- **LEFT JOIN:** Eşleşmeyen kart kayıtları da görünür (clearing tarafı NULL olur).
- **Join key:** `c.matched_clearing_line_id = clr.file_line_id` — ingestion sırasında atanan eşleşme referansı.

#### Hesaplanan Alanlar — Mismatch Flag'leri

**1. `match_status`:**
```sql
CASE
    WHEN clr.file_line_id IS NULL THEN 'UNMATCHED_CARD'
    ELSE 'MATCHED'
END
```
- Clearing bulunamazsa `UNMATCHED_CARD`, bulunursa `MATCHED`.

**2. `amount_difference`:**
```sql
c.original_amount - clr.source_amount
```
- Ham tutar farkı. Pozitif → kart fazla; negatif → clearing fazla.
- **NULL riski:** Clearing yoksa (`clr.source_amount IS NULL`) → sonuç NULL.

**3. `has_amount_mismatch`:**
```sql
CASE
    WHEN clr.file_line_id IS NULL THEN NULL
    WHEN ABS(COALESCE(c.original_amount, 0) - COALESCE(clr.source_amount, 0)) > 0.01 THEN TRUE
    ELSE FALSE
END
```
- **0.01 toleransı:** 1 kuruşa kadar olan farklar yok sayılır (kur yuvarlama). Bu eşik bir **business rule**.
- **COALESCE:** NULL tutarlar 0 olarak değerlendirilir.
- **Eşleşmemiş:** NULL döner.

**4. `has_currency_mismatch`:**
```sql
CASE
    WHEN clr.file_line_id IS NULL THEN NULL
    WHEN c.original_currency IS DISTINCT FROM clr.source_currency THEN TRUE
    ELSE FALSE
END
```
- **IS DISTINCT FROM:** NULL-safe karşılaştırma. Standart `<>` operatörü NULL'larda çalışmaz; `IS DISTINCT FROM` NULL'ları da farklı değer olarak değerlendirir.

**5. `has_date_mismatch`:**
```sql
CASE
    WHEN clr.file_line_id IS NULL THEN NULL
    WHEN c.transaction_date::text IS DISTINCT FROM clr.txn_date::text THEN TRUE
    ELSE FALSE
END
```
- **`::text` cast:** Tarih alanları integer veya date tipinde olabilir. String'e çevrilerek tip bağımsız karşılaştırma yapılır.
- **Risk:** Farklı format temsilleri yanlış pozitif üretebilir (örneğin `20260415` vs `2026-04-15`).

**6. `has_status_mismatch`:**
```sql
CASE
    WHEN clr.file_line_id IS NULL THEN NULL
    WHEN c.is_successful_txn = 'Successful'
         AND COALESCE(clr.control_stat, 'Normal') <> 'Normal' THEN TRUE
    WHEN COALESCE(c.is_successful_txn, 'Unsuccessful') = 'Unsuccessful'
         AND COALESCE(clr.control_stat, 'Normal') = 'Normal' THEN TRUE
    ELSE FALSE
END
```
**Çelişki kuralları:**
1. Kart "Successful" + Clearing "Normal değil" → **çelişki** (kart başarılı diyor ama clearing'de dispute/chargeback var).
2. Kart "Unsuccessful" (veya NULL) + Clearing "Normal" → **çelişki** (kart başarısız diyor ama clearing normal diyor).
3. Her iki koşul da sağlanmazsa → `FALSE` (tutarlı).

**COALESCE davranışları:**
- `clr.control_stat` NULL → `'Normal'` varsayılır.
- `c.is_successful_txn` NULL → `'Unsuccessful'` varsayılır.

#### Performans Notu
- **VIEW-on-VIEW join:** İki base view (her biri 3 UNION ALL) birleştirilir. Bu, 6 tablo + 6 join'un tek bir SELECT'te çözülmesi anlamına gelir.
- PostgreSQL optimizer genellikle view'ları inline eder, ancak bu karmaşıklık seviyesinde query plan kontrolü yapılmalıdır.
- **Bu view'a doğrudan sayfalama uygulamak kritik önem taşır.** `SELECT * FROM vw_reconciliation_matched_pair` full table scan yapacaktır.

---

### VIEW 15: `reporting.vw_reconciliation_unmatched_card`

#### Amaç
Clearing karşılığı bulunamayan kart işlemlerini `matched_pair` view'ından filtreler.

#### Filtre
```sql
WHERE match_status = 'UNMATCHED_CARD'
```

#### Tüm kolonları `vw_reconciliation_matched_pair`'den inherit eder.

#### İş Anlamı
Bu kayıtların clearing dosyası henüz gelmemiş veya korelasyon anahtarı eşleşmemiştir.

---

### VIEW 16: `reporting.vw_reconciliation_amount_mismatch`

#### Amaç
Eşleşmiş ama tutar farkı > 0.01 olan çiftleri gösterir.

#### Filtre
```sql
WHERE match_status = 'MATCHED'
  AND has_amount_mismatch = TRUE
```

#### İş Anlamı
**Finansal risk içeren kayıtlar.** Her biri tek tek araştırılmalıdır. Kart ve clearing arasındaki tutar farkı kur dönüşümü, komisyon, veya veri hatası kaynaklı olabilir.

---

### VIEW 17: `reporting.vw_reconciliation_status_mismatch`

#### Amaç
Eşleşmiş ama durum bilgileri çelişen çiftleri gösterir.

#### Filtre
```sql
WHERE match_status = 'MATCHED'
  AND has_status_mismatch = TRUE
```

#### İş Anlamı
Kart "başarılı" diyor ama clearing "anormal" diyor (dispute, chargeback), veya tersi. Dispute sürecinin takibi gerektirir.

---

### VIEW 18: `reporting.vw_reconciliation_clean_matched`

#### Amaç
Eşleşmiş ve **tüm karşılaştırma kontrollerini geçen** (tam mutabık) çiftleri gösterir.

#### Filtre
```sql
WHERE match_status = 'MATCHED'
  AND has_amount_mismatch   = FALSE
  AND has_currency_mismatch = FALSE
  AND has_date_mismatch     = FALSE
```

#### Dikkat: `has_status_mismatch` Kontrol Edilmiyor
Filtre `has_status_mismatch = FALSE` koşulunu **içermiyor**. Bu, durum uyuşmazlığı olan ama tutar/para birimi/tarih uyuşmayan kayıtların "clean" sayılabileceği anlamına gelir. **Varsayım:** Bu bilinçli bir tasarım kararı olabilir — status mismatch dispute sürecine ait bir durum olup mutabakat "clean"liğini etkilemeyebilir. Ancak bu belirsiz bir noktadır ve doğrulanmalıdır.

**Güncelleme:** SQL'e tekrar bakıldığında, `has_status_mismatch = FALSE` koşulunun eklenmediği teyit edildi. Bu, `clean_matched` tanımının "tutar/para birimi/tarih temiz ama status mismatch olabilir" anlamına geldiğini gösterir.

---

### VIEW 19: `reporting.vw_reconciliation_problem_records`

#### Amaç
En az bir mutabakat sorunu olan **tüm** kayıtları birleştirir.

#### Filtre (OR Mantığı)
```sql
WHERE match_status = 'UNMATCHED_CARD'
   OR has_amount_mismatch   = TRUE
   OR has_currency_mismatch = TRUE
   OR has_date_mismatch     = TRUE
   OR has_status_mismatch   = TRUE
   OR (duplicate_status IS NOT NULL AND duplicate_status <> 'Unique')
```

#### Kapsam
| Koşul | Ne Anlama Geliyor |
|-------|-------------------|
| `UNMATCHED_CARD` | Clearing bulunamadı |
| `has_amount_mismatch = TRUE` | Tutar farkı > 0.01 |
| `has_currency_mismatch = TRUE` | Para birimi farklı |
| `has_date_mismatch = TRUE` | Tarih tutarsız |
| `has_status_mismatch = TRUE` | Durum çelişkisi |
| `duplicate_status <> 'Unique'` | Duplikat (Primary, Secondary, Conflict) |

#### Önemli Not: `problem_count ≠ total - clean_count`
- `clean_matched` `duplicate_status` kontrolü yapmaz ama `problem_records` yapar.
- `clean_matched` `has_status_mismatch` kontrolü yapmaz ama `problem_records` yapar.
- Bu yüzden `total_count ≠ problem_count + clean_count`. Arada "gri alan" kayıtlar vardır: matched, tutar/tarih/para birimi temiz ama duplikat Primary veya status mismatch olan kayıtlar.

---

### VIEW 20: `reporting.vw_reconciliation_summary_daily`

#### Amaç
İşlem tarihine göre günlük mutabakat KPI'ları üretir. Tek satır = bir gün.

#### Kaynak
```sql
FROM reporting.vw_reconciliation_matched_pair
GROUP BY card_transaction_date
```

#### KPI Kolonları

| Kolon | Hesaplama | İş Anlamı |
|-------|-----------|-----------|
| `total_count` | `COUNT(*)` | O günün toplam kart işlemi |
| `matched_count` | `MATCHED` filtreli count | Clearing ile eşleşen |
| `unmatched_count` | `UNMATCHED_CARD` filtreli count | Eşleşmeyen |
| `amount_mismatch_count` | `has_amount_mismatch = TRUE` | Tutar uyuşmazlığı |
| `currency_mismatch_count` | `has_currency_mismatch = TRUE` | Para birimi uyuşmazlığı |
| `date_mismatch_count` | `has_date_mismatch = TRUE` | Tarih uyuşmazlığı |
| `status_mismatch_count` | `has_status_mismatch = TRUE` | Durum çelişkisi |
| `problem_count` | OR birleşim filtre (6 koşul) | Toplam sorunlu kayıt |
| `clean_count` | MATCHED + tüm mismatch FALSE | Tam temiz kayıt |

#### `problem_count` Hesaplaması
```sql
COUNT(*) FILTER (
    WHERE match_status = 'UNMATCHED_CARD'
       OR has_amount_mismatch   = TRUE
       OR has_currency_mismatch = TRUE
       OR has_date_mismatch     = TRUE
       OR has_status_mismatch   = TRUE
       OR COALESCE(duplicate_status, 'Unique') <> 'Unique'
)
```
- `COALESCE(duplicate_status, 'Unique')`: NULL duplicate_status "Unique" varsayılır → problem sayılmaz.

#### `clean_count` Hesaplaması
```sql
COUNT(*) FILTER (
    WHERE match_status = 'MATCHED'
      AND has_amount_mismatch   = FALSE
      AND has_currency_mismatch = FALSE
      AND has_date_mismatch     = FALSE
      AND has_status_mismatch   = FALSE
)
```
- **status_mismatch burada kontrol ediliyor** (VIEW 18'den farklı olarak).
- **duplicate_status burada kontrol edilmiyor** — duplikat Primary olan ama tüm mismatch'leri FALSE olan kayıt "clean" sayılır.

#### Performans Notu
`vw_reconciliation_matched_pair` üzerinde GROUP BY — alttaki 6 tablonun tamamı taranır. **Büyük veri kümeleri için ağır.**

---

### VIEW 21: `reporting.vw_reconciliation_summary_by_network`

#### Amaç
Kart ağı (BKM/VISA/MSC) bazında mutabakat KPI'ları.

#### Gruplama
```sql
GROUP BY network
```

#### KPI Kolonları
VIEW 20 ile birebir aynı metrikler. Tek fark: `card_transaction_date` yerine `network` kırılımı.

#### İş Anlamı
Hangi kart ağında daha fazla sorun var? Ağlar arası benchmark yapılır.

---

### VIEW 22: `reporting.vw_reconciliation_summary_by_file`

#### Amaç
Dosya bazında mutabakat KPI'ları.

#### Gruplama
```sql
GROUP BY file_id, file_name
```

#### İş Anlamı
Hangi dosya en sorunlu? Tek bir dosyada yüksek `problem_count` → dosya bozuk veya kaynak hatalı.

---

### VIEW 23: `reporting.vw_reconciliation_summary_overall`

#### Amaç
**Tek satırlık** genel mutabakat özeti. Gruplama yok.

#### İş Anlamı
Dashboard'un en üstündeki "genel durum" kartı: Toplam kaç kayıt var, kaçı temiz, kaçı sorunlu?

#### Performans Notu
- GROUP BY yok, tüm matched_pair view'ı taranır ve tek satıra indirgenir.
- En ağır sorgu — tüm veritabanının özeti.
- **Cache mekanizması veya materialized view önerilir.**

---

## 4. Business Mantıkları

### 4.1 Business Rule Listesi

| # | Kural | SQL Konumu | Etkilediği Alan | Devreye Girdiği Senaryo |
|---|-------|------------|-----------------|------------------------|
| **BR-01** | Satır tipi filtresi: Yalnızca `line_type = 'D'` (Detail) satırları raporlamaya dahil edilir | VIEW 2, 3, 4, 8, 9 | Tüm sayaçlar ve metrikler | Header ('H') ve Footer ('F') satırlarının istatistikleri bozmamması için |
| **BR-02** | Başarı oranı hesaplaması: `success_rate = (success / processed) * 100` | VIEW 1, 2, 8 | `success_rate`, `recon_completion_rate` | İşleme kalitesinin ölçülmesi |
| **BR-03** | Sıfıra bölme koruması: İşlenen satır sayısı 0 ise oran 0 döner | VIEW 1, 2, 8 | Oran kolonları | Henüz satır işlenmemiş dosyalar |
| **BR-04** | Sayaç uyuşmazlığı tespiti: `expected_count > 0 AND processed_count <> expected_count` | VIEW 1 | `has_count_mismatch` | Dosya truncation veya iletişim hatası |
| **BR-05** | Eşleşme durumu: `matched_clearing_line_id IS NULL` → UNMATCHED_CARD | VIEW 4, 14 | `match_status`, unmatched filtresi | Clearing dosyası yüklenmemiş veya eşleşme başarısız |
| **BR-06** | Tutar toleransı: `ABS(card_amount - clearing_amount) > 0.01` eşiği | VIEW 14 | `has_amount_mismatch` | Kur yuvarlama farkı (0.01'e kadar kabul) vs gerçek uyuşmazlık |
| **BR-07** | NULL-safe para birimi karşılaştırması: `IS DISTINCT FROM` kullanımı | VIEW 14 | `has_currency_mismatch` | Bir tarafın NULL, diğer tarafın dolu olması |
| **BR-08** | Tarih string karşılaştırması: `::text` cast sonrası `IS DISTINCT FROM` | VIEW 14 | `has_date_mismatch` | Farklı tip/format temsilleri |
| **BR-09** | Durum çelişkisi — Senaryo 1: Kart Successful + Clearing Normal değil | VIEW 14 | `has_status_mismatch` | Dispute/chargeback durumu |
| **BR-10** | Durum çelişkisi — Senaryo 2: Kart Unsuccessful/NULL + Clearing Normal | VIEW 14 | `has_status_mismatch` | Kart başarısız ama clearing temiz |
| **BR-11** | NULL is_successful_txn → 'Unsuccessful' varsayımı | VIEW 14 | `has_status_mismatch` | `is_successful_txn` NULL ise başarısız kabul edilir |
| **BR-12** | NULL control_stat → 'Normal' varsayımı | VIEW 14 | `has_status_mismatch` | `control_stat` NULL ise normal kabul edilir |
| **BR-13** | NULL tutar → 0 varsayımı (`COALESCE(amount, 0)`) | VIEW 14 | `has_amount_mismatch` | Tutarı eksik olan kayıt |
| **BR-14** | Dispute filtresi: `control_stat <> 'Normal' OR dispute_code <> 'None'` | VIEW 11 | Dispute monitor | Normal olmayan veya dispute kodu atanmış clearing kayıtları |
| **BR-15** | Duplikat dahil etme: `duplicate_status <> 'Unique'` problem sayılır | VIEW 19, 20-23 (problem_count) | Problem kayıtlar ve KPI | Primary, Secondary, Conflict kayıtlar sorunlu kabul edilir |
| **BR-16** | NULL duplicate_status → 'Unique' varsayımı | VIEW 20-23 | `problem_count` | `COALESCE(duplicate_status, 'Unique')` |
| **BR-17** | Yaşlanma bucket'ları: 0-1, 2-3, 4-7, 8-14, 15-30, 30+ gün | VIEW 9 | `age_bucket` | SLA takibi ve eskalasyon |
| **BR-18** | Açık kayıt tanımı: `Ready, Processing, Failed` + `is_archived = false` | VIEW 9 | Aging analizi scope | Success ve arşivlenmiş kayıtlar yaşlanmaya dahil değil |
| **BR-19** | Review süre dolma: `expires_at < NOW() AND decision = 'Pending'` | VIEW 6 | `is_review_expired` | Otomatik karar uygulanması gereken durumlar |
| **BR-20** | Arşiv süre hesabı: `archive_date - ingestion_date` → `days_to_archive` | VIEW 10 | Arşiv SLA ölçümü | Retention policy takibi |
| **BR-21** | Ağ normalizasyonu: 3 farklı tablo → tekil `network` kolonu | VIEW 12, 13 | Ağ bazlı filtreleme ve gruplama | Çoklu kart ağı desteği |
| **BR-22** | Clean matched tanımı: Tüm mismatch FALSE + MATCHED (ama status_mismatch kontrol edilmez — VIEW 18'de) | VIEW 18 vs VIEW 20-23 | `clean_count` | VIEW 18: status hariç temiz; VIEW 20-23: status dahil temiz |
| **BR-23** | `has_count_mismatch` yalnızca `expected > 0` ise aktif | VIEW 1 | Footer kontrolü | Footer bilgisi yoksa kontrol devre dışı |

---

## 5. Yorumlama Senaryoları

### 5.1 `vw_file_ingestion_summary` Yorumlama

| Veri Durumu | Anlam | Aksiyon |
|-------------|-------|---------|
| `success_rate = 100`, `has_count_mismatch = false` | Dosya mükemmel yüklendi | Yok |
| `success_rate = 95`, `error_count = 250` | 250 satır parse hatası aldı | Hatalı satırları incele, format sorunu olabilir |
| `has_count_mismatch = true` | Footer'daki beklenen satır sayısı tutmadı | Dosya truncation veya iletişim hatası olabilir |
| `is_archived = false`, `status = 'Failed'` | Arşivleme başarısız | Hedef depo erişimini kontrol et |
| `expected_count = 0` | Footer bilgisi yok veya okunamadı | Format kontrolü gerekli, count mismatch devre dışı |

### 5.2 `vw_reconciliation_matched_pair` Yorumlama

| Veri Durumu | Anlam | Aksiyon |
|-------------|-------|---------|
| `match_status = 'MATCHED'`, tüm mismatch `FALSE` | Tam mutabakat sağlanmış | İdeal durum, aksiyon yok |
| `match_status = 'UNMATCHED_CARD'`, `age_days < 2` | Clearing henüz gelmemiş, bekleniyor | İzle, henüz acil değil |
| `match_status = 'UNMATCHED_CARD'`, `age_days > 7` | Clearing gelmemiş veya eşleşme başarısız | **Acil:** Clearing dosyasını kontrol et, korelasyon anahtarını doğrula |
| `has_amount_mismatch = TRUE`, `amount_difference = 0.50` | Küçük tutar farkı | Muhtemelen kur yuvarlama, izle |
| `has_amount_mismatch = TRUE`, `amount_difference = 500.00` | Büyük tutar farkı | **Acil araştırma:** Veri hatası veya fraud riski |
| `has_currency_mismatch = TRUE` | Farklı para birimleri | **Kritik:** Veri hatası veya çok para birimli işlem |
| `has_status_mismatch = TRUE` | Kart/clearing durum çelişkisi | Dispute/chargeback süreci kontrol edilmeli |
| `duplicate_status = 'Conflict'` | Aynı anahtar, farklı içerik | Veri kalitesi sorunu, kaynak incelenmeli |

### 5.3 `vw_reconciliation_aging` Yorumlama

| Bucket | Görülen Kayıt Sayısı | Anlam | Aksiyon |
|--------|----------------------|-------|---------|
| `0-1 Gün` | Yüksek | Normal — yeni yüklemeler | Yok |
| `2-3 Gün` | Orta | Kabul edilebilir birikim | İzle |
| `4-7 Gün` | > 100 | Evaluate/Execute planlaması gözden geçirilmeli | Evaluate ve Execute tetikle |
| `8-14 Gün` | > 0 | **Sorunlu.** Pipeline stuck | Acil müdahale, alert ve operasyon tracker kontrol et |
| `15-30 Gün` | > 0 | **Kritik** | Eskalasyon, manuel müdahale |
| `30+ Gün` | > 0 | **Acil** | Bu kayıtlar muhtemelen kalıcı hata, manuel çözüm veya arşivleme kararı gerekir |

### 5.4 `vw_reconciliation_summary_daily` Yorumlama

| Metrik | Normal Aralık | Alarm Eşiği | Örnek |
|--------|---------------|-------------|-------|
| `clean_count / total_count` | > %98 | < %95 | 49.000/50.000 = %98 → Normal |
| `unmatched_count / total_count` | < %2 | > %5 | 1.000/50.000 = %2 → Sınırda |
| `amount_mismatch_count` | 0 | > 10 | 5 → İzle; 100 → Acil |
| `problem_count` trendi | Azalan | Artan | 3 gün üst üste artıyorsa → Sistemik sorun |

### 5.5 Yanlış Yorumlama Riskleri

| Risk | Açıklama |
|------|----------|
| ⚠️ `clean_count + problem_count ≠ total_count` | Normal durumdur. Aradaki fark: matched + status mismatch olmayan ama duplikat olan kayıtlar, veya matched + tüm mismatch FALSE ama duplicate Primary olan kayıtlar. |
| ⚠️ VIEW 4 vs VIEW 15 karıştırılması | VIEW 4 (`vw_reconciliation_unmatched_cards`) `fl.status = 'Success'` filtresi uygular (ingestion seviyesi), VIEW 15 (`vw_reconciliation_unmatched_card`) `match_status = 'UNMATCHED_CARD'` filtresi uygular (matching seviyesi). Sonuçlar farklı olabilir. |
| ⚠️ `has_amount_mismatch = NULL` → "mismatch yok" değil | Eşleşmemiş kayıtlarda NULL döner, `FALSE` değil. Filtre yaparken `= TRUE` veya `= FALSE` kullanılmalı; `IS NOT NULL` kontrolü yapılmalı. |
| ⚠️ `success_rate` dosya success rate'i ≠ mutabakat success rate'i | VIEW 1'deki `success_rate` parse başarısını ölçer, VIEW 2'deki `recon_completion_rate` mutabakat başarısını ölçer. İkisi farklı metriklerdir. |
| ⚠️ `recon_pending_count` (VIEW 2, `IS NULL`) ≠ Evaluate bekleyen | `reconciliation_status IS NULL` korelasyon atanamadığı veya ingestion tamamlanmadığı durumu gösterir. `Ready` olan satırlar evaluate bekleyenlerdir. |

---

## 6. Tablolar ve Bağımlılıklar

### 6.1 Kullanılan Tablolar

| Schema | Tablo | Açıklama | Kullanan View'lar |
|--------|-------|----------|-------------------|
| `ingestion` | `file` | Dosya metadata (ad, tür, durum, sayaçlar) | 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 13 |
| `ingestion` | `file_line` | Dosya satırları (satır durumu, mutabakat durumu, eşleşme) | 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 13 |
| `ingestion` | `card_bkm_detail` | BKM kart işlem detayları | 12 |
| `ingestion` | `card_visa_detail` | Visa kart işlem detayları | 12 |
| `ingestion` | `card_msc_detail` | Mastercard kart işlem detayları | 12 |
| `ingestion` | `clearing_bkm_detail` | BKM clearing detayları | 11, 13 |
| `ingestion` | `clearing_visa_detail` | Visa clearing detayları | 11, 13 |
| `ingestion` | `clearing_msc_detail` | Mastercard clearing detayları | 11, 13 |
| `reconciliation` | `evaluation` | Mutabakat değerlendirme kayıtları | 3, 5 |
| `reconciliation` | `operation` | Mutabakat operasyonları | 3, 5, 6, 7 |
| `reconciliation` | `operation_execution` | Operasyon yürütme denemeleri | 5 |
| `reconciliation` | `review` | Manuel inceleme kayıtları | 6 |
| `reconciliation` | `alert` | Uyarı kayıtları | 7 |
| `archive` | `archive_log` | Arşiv işlem logu | 10 |
| `archive` | `ingestion_file` | Arşivlenmiş dosya snapshot'ı | 10 |

### 6.2 View → View Bağımlılıkları

| Bağımlı View | Bağlı Olduğu View |
|--------------|-------------------|
| `vw_reconciliation_matched_pair` (14) | `vw_base_card_transaction` (12), `vw_base_clearing_transaction` (13) |
| `vw_reconciliation_unmatched_card` (15) | `vw_reconciliation_matched_pair` (14) |
| `vw_reconciliation_amount_mismatch` (16) | `vw_reconciliation_matched_pair` (14) |
| `vw_reconciliation_status_mismatch` (17) | `vw_reconciliation_matched_pair` (14) |
| `vw_reconciliation_clean_matched` (18) | `vw_reconciliation_matched_pair` (14) |
| `vw_reconciliation_problem_records` (19) | `vw_reconciliation_matched_pair` (14) |
| `vw_reconciliation_summary_daily` (20) | `vw_reconciliation_matched_pair` (14) |
| `vw_reconciliation_summary_by_network` (21) | `vw_reconciliation_matched_pair` (14) |
| `vw_reconciliation_summary_by_file` (22) | `vw_reconciliation_matched_pair` (14) |
| `vw_reconciliation_summary_overall` (23) | `vw_reconciliation_matched_pair` (14) |

---

## 7. Veri Akışı ve Mimari Bakış

### 7.1 Katmanlı View Mimarisi

```
┌─────────────────────────────────────────────────────────────────────┐
│ KATMAN 0: OPERASYONEL TABLOLAR                                     │
│                                                                     │
│  ingestion.file ─────┐                                             │
│  ingestion.file_line ─┼── VIEW 1, 2, 3, 4, 8, 9                  │
│                       │                                             │
│  ingestion.card_*_detail ─────── VIEW 12 (base_card)               │
│  ingestion.clearing_*_detail ─── VIEW 13 (base_clearing)           │
│                                  VIEW 11 (dispute_monitor)         │
│                                                                     │
│  reconciliation.evaluation ──── VIEW 3, 5                          │
│  reconciliation.operation ───── VIEW 3, 5, 6, 7                   │
│  reconciliation.operation_execution ── VIEW 5                      │
│  reconciliation.review ──────── VIEW 6                             │
│  reconciliation.alert ───────── VIEW 7                             │
│                                                                     │
│  archive.archive_log ────────── VIEW 10                            │
│  archive.ingestion_file ─────── VIEW 10                            │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│ KATMAN 1: BASE VIEWS (Normalizasyon)                               │
│                                                                     │
│  VIEW 12: vw_base_card_transaction                                 │
│           BKM + VISA + MSC kart UNION ALL                          │
│                                                                     │
│  VIEW 13: vw_base_clearing_transaction                             │
│           BKM + VISA + MSC clearing UNION ALL                      │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│ KATMAN 2: EŞLEŞTİRME VIEW (Mismatch Hesaplama)                    │
│                                                                     │
│  VIEW 14: vw_reconciliation_matched_pair                           │
│           base_card LEFT JOIN base_clearing                        │
│           match_status + 4 mismatch flag                           │
└─────────────────────────────────────────────────────────────────────┘
                                │
                ┌───────────────┼───────────────┐
                ▼               ▼               ▼
┌───────────────────┐ ┌─────────────────┐ ┌─────────────────────────┐
│ KATMAN 3A: İŞ     │ │ KATMAN 3B: ÖZET │ │                         │
│                   │ │                 │ │                         │
│ VIEW 15: unmatched│ │ VIEW 20: daily  │ │                         │
│ VIEW 16: amount   │ │ VIEW 21: network│ │                         │
│ VIEW 17: status   │ │ VIEW 22: file   │ │                         │
│ VIEW 18: clean    │ │ VIEW 23: overall│ │                         │
│ VIEW 19: problems │ │                 │ │                         │
└───────────────────┘ └─────────────────┘ └─────────────────────────┘
```

### 7.2 Bağımsız Operasyonel View'lar (Katman 0'dan Doğrudan)

```
ingestion.file ──────────────── VIEW 1: file_ingestion_summary
ingestion.file + file_line ──── VIEW 2: reconciliation_file_summary
                                VIEW 4: reconciliation_unmatched_cards
                                VIEW 8: daily_reconciliation_summary
                                VIEW 9: reconciliation_aging
ingestion.file_line + eval + op  VIEW 3: reconciliation_line_detail
reconciliation.operation + exec  VIEW 5: operation_tracker
reconciliation.operation + review VIEW 6: pending_actions
reconciliation.alert + operation  VIEW 7: alert_dashboard
archive.archive_log + file ───── VIEW 10: archive_audit_trail
ingestion.clearing_*_detail ──── VIEW 11: clearing_dispute_monitor
```

### 7.3 Kritik Veri Akış Yolu

```
1. Dosya SFTP'den alınır
   └─→ ingestion.file kaydı oluşur
   └─→ ingestion.file_line kaydı oluşur (her satır için)
   └─→ ingestion.card_*_detail / clearing_*_detail kaydı oluşur
   └─→ matched_clearing_line_id atanır (clearing eşleştirmesi)
   
2. VIEW 12 + VIEW 13: Üç ağın verileri normalize edilir
   
3. VIEW 14: Kart ve clearing yan yana getirilir
   - match_status hesaplanır
   - Mismatch flag'leri hesaplanır
   
4. VIEW 15-19: İş senaryolarına göre filtrelenir
   
5. VIEW 20-23: KPI'lar grup bazında hesaplanır
```

---

## 8. Teknik Riskler ve Dikkat Edilmesi Gerekenler

### 8.1 Performans Riskleri

| Risk | Etkilenen View'lar | Detay | Önerilen Çözüm |
|------|-------------------|-------|-----------------|
| **VIEW-on-VIEW-on-VIEW zinciri** | 14, 15-23 | VIEW 20-23, VIEW 14'e bağlı; VIEW 14, VIEW 12+13'e bağlı; her biri 3 UNION ALL. Toplam: 6 tablo scan + 6 join tek query'de. | Materialized view (VIEW 14 seviyesinde) veya incremental refresh |
| **LATERAL subquery N+1 pattern** | 3, 5 | Her satır için ayrı subquery çalışır. 1M satır = 2M ek sorgu. | Index'leme kritik; sayfalama zorunlu; materialized view düşünülebilir |
| **Büyük UNION ALL'lar** | 11, 12, 13 | Her sorguda 3 büyük tablo birleştirilir, WHERE yok. | Partition veya materialized view |
| **Full table scan** | 23 (overall) | GROUP BY yok, tüm veri tek satıra indirgenir. | Cache katmanı veya periyodik snapshot |
| **`::date` cast** | 8 | `f.create_date::date` index kullanımını engeller. | Functional index: `CREATE INDEX ON ingestion.file ((create_date::date))` |
| **Çoklu EXTRACT hesaplaması** | 9 | `EXTRACT(DAY FROM ...)` 6 kez tekrarlanır (CASE + ORDER kolonu). | Hesaplanan değeri CTE'ye almak |
| **LEFT JOIN + GROUP BY** | 2, 8 | file_line milyonlarca satır olabilir. | Composite index: `file_line(file_id, line_type, reconciliation_status)` |

### 8.2 Duplicate Veri Üretme Riskleri

| Risk | Açıklama |
|------|----------|
| **VIEW 14 LEFT JOIN** | `matched_clearing_line_id` ile tek clearing satırına bağlanır. Birden fazla kart satırı aynı clearing satırına eşleşmişse → clearing tarafı tekrar eder. Ama bu kart-centric bir view olduğu için sorun değil. |
| **VIEW 12/13 UNION ALL** | `UNION ALL` duplicate elimination yapmaz. Aynı kayıt hem BKM hem Visa tablosunda varsa (normalde olmamalı ama veri hatası durumunda) → çift sayılır. |
| **VIEW 2 LEFT JOIN** | Bir dosyanın hiç satırı yoksa tüm count'lar 0 gelir. Birden fazla `file_line` satırı → tek dosya satırı olarak gruplanır (GROUP BY ile korunur). |

### 8.3 Business Yorumlama Riskleri

| Risk | Açıklama |
|------|----------|
| **`has_status_mismatch` VIEW 18'de kontrol edilmiyor** | `clean_matched` view'ında status mismatch filtresi yok. Status mismatch olan kayıtlar "clean" sayılabilir. |
| **`clean_count + problem_count ≠ total_count`** | İki tanım farklı — aradaki boşluk düşük riskli kayıtlardır ama bu fark bilinmezse kafa karışıklığı yaratır. |
| **NULL dispute_code filtering** | VIEW 11'de `dispute_code <> 'None'` → NULL dispute_code'lar dahil edilmez. NULL belirsiz durumu gösterebilir. |
| **NULL mismatch flag'leri** | Eşleşmemiş kayıtlarda mismatch flag'leri NULL döner, FALSE değil. `WHERE has_amount_mismatch = FALSE` filtresi NULL kayıtları **dahil etmez**. |

### 8.4 Tarih/Timestamp Riskleri

| Risk | Açıklama |
|------|----------|
| **`NOW()` kullanımı** | VIEW 3, 4, 5, 6, 7, 9'da `NOW()` ile yaşlanma hesaplanır. Her sorguda farklı `NOW()` değeri döner → aynı view'ın art arda çağrılmasında milisaniyelik farklılıklar olabilir. |
| **`::text` date karşılaştırması** | VIEW 14'te tarih alanları text'e çevrilerek karşılaştırılır. Integer (`20260415`) ve date (`2026-04-15`) formatları farklı string üretir → yanlış pozitif risk. |
| **Timezone farkı** | `NOW()` sunucu timezone'unu kullanır. Kart ağları UTC kullanabilir → yaşlanma hesaplamaları birkaç saat sapabilir. |

### 8.5 Null ve Eksik Veri Riskleri

| Risk | Açıklama |
|------|----------|
| **COALESCE default değerleri** | `COALESCE(amount, 0)` → NULL tutar 0 sayılır. Clearing tutarı gerçekten 0 ise tutar mismatch yapay olarak oluşur. |
| **COALESCE(is_successful_txn, 'Unsuccessful')** | NULL başarı durumu "başarısız" varsayılır. Gerçekte belirsiz olabilir. |
| **COALESCE(control_stat, 'Normal')** | NULL kontrol durumu "normal" varsayılır. Gerçekte belirsiz olabilir. |
| **LEFT JOIN orphan kayıtlar** | VIEW 10'da `archive.ingestion_file` bulunamazsa dosya bilgileri NULL döner. VIEW 7'de INNER JOIN kullanıldığı için orphan alertler görünmez. |

---

## 9. Özet Tablo

| # | View Adı | Amaç | Kaynak Tablolar | Temel Business Mantığı | Kritik Kolonlar | Riskler |
|---|----------|------|-----------------|------------------------|-----------------|---------|
| 1 | `vw_file_ingestion_summary` | Dosya yükleme özeti | `ingestion.file` | Parse başarı oranı, sayaç uyuşmazlığı | `success_rate`, `has_count_mismatch` | `expected_count = 0` → kontrol devre dışı |
| 2 | `vw_reconciliation_file_summary` | Dosya bazlı mutabakat durumu | `ingestion.file`, `file_line` | Satır bazlı recon status dağılımı, duplikat sayısı | `recon_completion_rate`, `unmatched_card_count`, `duplicate_count` | LEFT JOIN + GROUP BY performansı |
| 3 | `vw_reconciliation_line_detail` | Satır detay analizi | `file_line`, `file`, `evaluation`, `operation` | Evaluation/operation aggregate, yaşlanma | `evaluation_count`, `latest_evaluation_status`, `age_days` | LATERAL N+1 pattern, performans |
| 4 | `vw_reconciliation_unmatched_cards` | Eşleşmemiş kart satırları (ingestion seviyesi) | `file_line`, `file` | Card + Success + unmatched filtresi | `age_days`, `correlation_value` | VIEW 15 ile karıştırılma riski |
| 5 | `vw_reconciliation_operation_tracker` | Operasyon takibi | `operation`, `evaluation`, `file_line`, `file`, `execution` | Son execution bilgisi, toplam deneme | `last_execution_status`, `age_hours`, `total_executions` | 4 JOIN + 2 LATERAL, performans |
| 6 | `vw_reconciliation_pending_actions` | Bekleyen aksiyonlar | `operation`, `file_line`, `file`, `review` | Review süre dolma tespiti | `is_review_expired`, `waiting_hours` | Review decision = 'Pending' filtresi |
| 7 | `vw_reconciliation_alert_dashboard` | Uyarı panosu | `alert`, `operation`, `file_line`, `file` | Alert severity, type, yaşlanma | `severity`, `alert_type`, `age_hours` | INNER JOIN → orphan alertler görünmez |
| 8 | `vw_daily_reconciliation_summary` | Günlük özet (dosya yükleme tarihine göre) | `ingestion.file`, `file_line` | Content_type/file_type kırılımlı sayaçlar | `total_files`, `success_rate`, `matched_count` | `::date` cast → index kullanılmaz |
| 9 | `vw_reconciliation_aging` | Yaşlanma analizi | `file_line`, `file` | Age bucket (6 aralık), açık kayıt tanımı | `age_bucket`, `open_count` | Failed dahil, Success hariç |
| 10 | `vw_archive_audit_trail` | Arşiv denetim izi | `archive.archive_log`, `archive.ingestion_file` | Arşive kadar geçen gün | `days_to_archive`, `archive_status` | LEFT JOIN → NULL dosya bilgileri |
| 11 | `vw_clearing_dispute_monitor` | Dispute izleme | `clearing_*_detail`, `file_line`, `file` | Normal olmayan control_stat veya dispute_code | `dispute_code`, `control_stat`, `network` | NULL dispute_code dahil edilmez |
| 12 | `vw_base_card_transaction` | Kart veri normalizasyonu | `card_bkm/visa/msc_detail`, `file_line`, `file` | 3 ağ → tekil yapı (UNION ALL) | `network`, `ocean_txn_guid`, `original_amount` | Filtre yok → tüm veriler taranır |
| 13 | `vw_base_clearing_transaction` | Clearing veri normalizasyonu | `clearing_bkm/visa/msc_detail`, `file_line`, `file` | 3 ağ → tekil yapı (UNION ALL) | `network`, `ocean_txn_guid`, `source_amount` | Filtre yok → tüm veriler taranır |
| 14 | `vw_reconciliation_matched_pair` | Kart-clearing eşleştirme | **VIEW** 12 + 13 | 4 mismatch flag + match_status | `match_status`, `has_amount_mismatch`, `has_status_mismatch` | VIEW-on-VIEW, 0.01 tolerans, `::text` cast |
| 15 | `vw_reconciliation_unmatched_card` | Eşleşmemiş kartlar (matching seviyesi) | **VIEW** 14 | `match_status = 'UNMATCHED_CARD'` | Tümü VIEW 14'ten | VIEW 4 ile karıştırılma |
| 16 | `vw_reconciliation_amount_mismatch` | Tutar uyuşmazlıkları | **VIEW** 14 | `MATCHED + has_amount_mismatch = TRUE` | `amount_difference` | Finansal risk |
| 17 | `vw_reconciliation_status_mismatch` | Durum çelişkileri | **VIEW** 14 | `MATCHED + has_status_mismatch = TRUE` | `card_is_successful_txn`, `clearing_control_stat` | Dispute süreci bağlantısı |
| 18 | `vw_reconciliation_clean_matched` | Tam temiz kayıtlar | **VIEW** 14 | MATCHED + amount/currency/date mismatch FALSE | — | **status_mismatch kontrol edilmiyor** |
| 19 | `vw_reconciliation_problem_records` | Tüm sorunlu kayıtlar | **VIEW** 14 | 6 koşuldan herhangi biri TRUE (OR) | Tüm mismatch flag'leri | `clean + problem ≠ total` |
| 20 | `vw_reconciliation_summary_daily` | Günlük KPI | **VIEW** 14 | İşlem tarihine göre grup | `clean_count`, `problem_count` | VIEW zinciri performansı |
| 21 | `vw_reconciliation_summary_by_network` | Ağ bazlı KPI | **VIEW** 14 | Network'e göre grup | Tüm KPI'lar | VIEW zinciri performansı |
| 22 | `vw_reconciliation_summary_by_file` | Dosya bazlı KPI | **VIEW** 14 | file_id/file_name'e göre grup | `problem_count` (sıralama için) | VIEW zinciri performansı |
| 23 | `vw_reconciliation_summary_overall` | Genel özet (tek satır) | **VIEW** 14 | GROUP BY yok, tüm veri | `total_count`, `clean_count`, `problem_count` | **En ağır sorgu**, cache önerilir |

---

> **Doküman Sonu**  
> Bu doküman, `V1_0_4__ReportingViews.sql` migration dosyasındaki 23 view'ın eksiksiz teknik analizini kapsamaktadır. Tüm business rule'lar, veri akışları, bağımlılıklar, yorumlama senaryoları ve teknik riskler detaylı olarak belgelenmiştir.

