# Reconciliation Operation Execution Guideline

## Scope
Bu doküman `Reconciliation` operation execution standardını netleştirir. Amaç, yeni operation eklerken tutarlı mimari ve davranış korunumudur.

Bu standardın kapsamı:
- `OperationExecutionService`
- `IReconciliationOperationHandler` ve handler implementasyonları
- `OperationExecutionContext`
- Operation input / idempotency / error code yaklaşımı

## Core Responsibilities

### OperationExecutionService
`OperationExecutionService` yalnız orchestration katmanıdır.

Sorumluluklar:
- pending auto step seçimi
- step executable kontrolü
- step status geçişlerinin orchestration sırası
- execution log insert
- case status recalc tetikleme
- response counter güncelleme

`OperationExecutionService` içinde business side-effect kuralı yazılmaz.

### Handler
Handler, tek operation code için business execution boundary’sidir.

Sorumluluklar:
- context doğrulama
- typed input okuma
- idempotency zorunluluğu uygunsa doğrulama
- internal service call veya external client call
- `OperationHandlerResult` üretimi

Handler, execution pipeline orchestration akışını değiştirmez.

### OperationExecutionContext
`OperationExecutionContext`, handler’ın ihtiyaç duyduğu read-only çalışma datasıdır.

Kural:
- yeni DB query yazmaktansa context verisini kullan
- `Require*` methodları ile zorunlu veri açıkça doğrulanmalı
- context içeriğine runtime mutable business state taşıma yapılmamalı

## Execution Standards

### 1) Internal Operation Standard
Pattern:
- `handler -> existing application/domain service -> OperationHandlerResult`

Kullanım kriteri:
- dış provider çağrısı yoksa
- mevcut bounded-context servisleri işi zaten yapabiliyorsa

Bu tip operation’da ek client/transport katmanı eklenmez.

### 2) External Provider Operation Standard
Pattern:
- `handler -> operation client -> provider transport -> operation result mapping`

Kullanım kriteri:
- dış sistem HTTP/queue/provider çağrısı varsa
- timeout/network/protocol hata ayrımı gerekiyorsa

Transport katmanı:
- request/response DTO mapping
- HTTP status + body parse + timeout + network hata ayrımı

Client katmanı:
- operation-level request validation
- provider response’u operation response modeline dönüştürme

Handler:
- final business result (`OperationHandlerResult`) üretimi

## Operation Input / Payload Standard
- Planner tarafı step oluştururken `OperationInputType`, `OperationInputJson`, `IdempotencyKey` üretir.
- `OperationInputType` sabitleri versioned olmalı (`*_INPUT_V1`).
- Handler içinde input erişimi yalnız serializer üzerinden olmalı.
- Ad-hoc JSON parse yasak.

## Idempotency Standard
- Primary source: planner/input factory.
- Handler fallback idempotency üretmez.
- operation için zorunluysa `RequireIdempotencyKey` kullanılmalı.
- Legacy pending step kayıtları için migration/backfill planı zorunludur.

## Error Code Standard
Aşağıdaki kodlar standarttır:
- `OPERATION_VALIDATION_FAILED`
- `OPERATION_BUSINESS_REJECTED`
- `EXTERNAL_SYSTEM_FAILURE`
- `EXTERNAL_CONTRACT_FAILURE`

Kural:
- Validation/contract hataları deterministic ve non-retry olarak işaretlenmeli.
- Network/timeout/5xx sınıfı hatalar system failure olarak ele alınmalı.
- Business reject, provider availability hatası gibi davranılmamalı.

## Status / Result Semantics
- `Executed + Success`: operation side-effect başarıyla çalıştı.
- `Skipped + Skipped`: operation bilinçli no-op veya accepted-no-state-change.
- `Failed + Failed`: business reject veya operation failure.

Bu semantik response counter davranışıyla birebir uyumlu kalmalıdır.

## Non-Goals
- `OperationExecutionService` save order/sıra mantığını değiştirmek
- planner/evaluation/manual decision akışına müdahale
- operation pipeline içine yeni business rule eklemek

## Checklist for New Operation
Yeni operation eklemeden önce:
1. Internal mı external mı?
2. InputType sabiti ve payload modeli versioned mi?
3. Idempotency key zorunluluğu net mi?
4. Error code mapping standarda uyuyor mu?
5. Handler yalnız operation boundary’de mi kalıyor?
6. OperationExecutionService’e business logic sızdı mı?
