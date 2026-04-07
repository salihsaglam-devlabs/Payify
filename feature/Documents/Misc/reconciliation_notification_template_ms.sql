IF EXISTS (
    SELECT 1
    FROM core.notification_template
    WHERE name = N'ReconciliationAlertTemplate_TR'
      AND type = N'Email'
)
BEGIN
UPDATE core.notification_template
SET
    originator = N'Payify',
    header = N'Mutabakat Uyari Bildirimi',
    body = N'<html><body style="font-family:Arial,sans-serif;font-size:13px;color:#222;background:#f6f8fb;padding:16px;"><table style="max-width:720px;width:100%;margin:auto;border-collapse:collapse;background:#fff;border:1px solid #d9e2f1;"><tr><td colspan="2" style="background:#1f4e79;color:#fff;padding:12px 14px;font-size:18px;font-weight:bold;">Mutabakat Uyari Bildirimi</td></tr><tr><td colspan="2" style="padding:12px 14px;border-bottom:1px solid #e6ebf2;">Mutabakat surecinde dikkat gerektiren bir kayit tespit edilmistir.</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;width:220px;"><b>Uyari Tipi</b></td><td style="padding:8px 14px;">@@alerttype</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Oncelik</b></td><td style="padding:8px 14px;">@@alertseverity</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Olusma Zamani</b></td><td style="padding:8px 14px;">@@raisedat</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Ozet</b></td><td style="padding:8px 14px;">@@summary</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Evaluation</b></td><td style="padding:8px 14px;">@@evaluationstatus - @@evaluationmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Operasyon</b></td><td style="padding:8px 14px;">@@operationcode - @@operationstatus - @@operationnote</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Son Execution</b></td><td style="padding:8px 14px;">@@lastexecutionstatus - @@lastresultmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;color:#b42318;"><b>Hata</b></td><td style="padding:8px 14px;color:#b42318;">@@error</td></tr><tr><td colspan="2" style="padding:10px 14px;background:#fbfcfe;border-top:1px solid #e6ebf2;"><b>Teknik Detay</b><div style="margin-top:6px;white-space:pre-wrap;font-family:Consolas,monospace;font-size:12px;color:#444;">@@detailmessage</div></td></tr></table></body></html>',
    channel = N'',
    update_date = GETDATE(),
    last_modified_by = N'019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = N'Active',
    category = N'Reconciliation'
WHERE name = N'ReconciliationAlertTemplate_TR'
  AND type = N'Email';
END
ELSE
BEGIN
INSERT INTO core.notification_template
(
    id, type, name, originator, header, body, channel,
    create_date, update_date, created_by, last_modified_by, record_status, category
)
VALUES
    (
        NEWID(),
        N'Email',
        N'ReconciliationAlertTemplate_TR',
        N'Payify',
        N'Mutabakat Uyari Bildirimi',
        N'<html><body style="font-family:Arial,sans-serif;font-size:13px;color:#222;background:#f6f8fb;padding:16px;"><table style="max-width:720px;width:100%;margin:auto;border-collapse:collapse;background:#fff;border:1px solid #d9e2f1;"><tr><td colspan="2" style="background:#1f4e79;color:#fff;padding:12px 14px;font-size:18px;font-weight:bold;">Mutabakat Uyari Bildirimi</td></tr><tr><td colspan="2" style="padding:12px 14px;border-bottom:1px solid #e6ebf2;">Mutabakat surecinde dikkat gerektiren bir kayit tespit edilmistir.</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;width:220px;"><b>Uyari Tipi</b></td><td style="padding:8px 14px;">@@alerttype</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Oncelik</b></td><td style="padding:8px 14px;">@@alertseverity</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Olusma Zamani</b></td><td style="padding:8px 14px;">@@raisedat</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Ozet</b></td><td style="padding:8px 14px;">@@summary</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Evaluation</b></td><td style="padding:8px 14px;">@@evaluationstatus - @@evaluationmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Operasyon</b></td><td style="padding:8px 14px;">@@operationcode - @@operationstatus - @@operationnote</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Son Execution</b></td><td style="padding:8px 14px;">@@lastexecutionstatus - @@lastresultmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;color:#b42318;"><b>Hata</b></td><td style="padding:8px 14px;color:#b42318;">@@error</td></tr><tr><td colspan="2" style="padding:10px 14px;background:#fbfcfe;border-top:1px solid #e6ebf2;"><b>Teknik Detay</b><div style="margin-top:6px;white-space:pre-wrap;font-family:Consolas,monospace;font-size:12px;color:#444;">@@detailmessage</div></td></tr></table></body></html>',
        N'',
        GETDATE(),
        NULL,
        N'019c5665-c7d5-7ea8-a5fc-d386d416e92a',
        NULL,
        N'Active',
        N'Reconciliation'
    );
END;

IF EXISTS (
    SELECT 1
    FROM core.notification_template
    WHERE name = N'ReconciliationAlertTemplate_EN'
      AND type = N'Email'
)
BEGIN
UPDATE core.notification_template
SET
    originator = N'Payify',
    header = N'Reconciliation Alert Notification',
    body = N'<html><body style="font-family:Arial,sans-serif;font-size:13px;color:#222;background:#f6f8fb;padding:16px;"><table style="max-width:720px;width:100%;margin:auto;border-collapse:collapse;background:#fff;border:1px solid #d9e2f1;"><tr><td colspan="2" style="background:#1f4e79;color:#fff;padding:12px 14px;font-size:18px;font-weight:bold;">Reconciliation Alert Notification</td></tr><tr><td colspan="2" style="padding:12px 14px;border-bottom:1px solid #e6ebf2;">A record requiring attention has been detected during the reconciliation process.</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;width:220px;"><b>Alert Type</b></td><td style="padding:8px 14px;">@@alerttype</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Severity</b></td><td style="padding:8px 14px;">@@alertseverity</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Raised At</b></td><td style="padding:8px 14px;">@@raisedat</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Summary</b></td><td style="padding:8px 14px;">@@summary</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Evaluation</b></td><td style="padding:8px 14px;">@@evaluationstatus - @@evaluationmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Operation</b></td><td style="padding:8px 14px;">@@operationcode - @@operationstatus - @@operationnote</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Latest Execution</b></td><td style="padding:8px 14px;">@@lastexecutionstatus - @@lastresultmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;color:#b42318;"><b>Error</b></td><td style="padding:8px 14px;color:#b42318;">@@error</td></tr><tr><td colspan="2" style="padding:10px 14px;background:#fbfcfe;border-top:1px solid #e6ebf2;"><b>Technical Details</b><div style="margin-top:6px;white-space:pre-wrap;font-family:Consolas,monospace;font-size:12px;color:#444;">@@detailmessage</div></td></tr></table></body></html>',
    channel = N'',
    update_date = GETDATE(),
    last_modified_by = N'019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = N'Active',
    category = N'Reconciliation'
WHERE name = N'ReconciliationAlertTemplate_EN'
  AND type = N'Email';
END
ELSE
BEGIN
INSERT INTO core.notification_template
(
    id, type, name, originator, header, body, channel,
    create_date, update_date, created_by, last_modified_by, record_status, category
)
VALUES
    (
        NEWID(),
        N'Email',
        N'ReconciliationAlertTemplate_EN',
        N'Payify',
        N'Reconciliation Alert Notification',
        N'<html><body style="font-family:Arial,sans-serif;font-size:13px;color:#222;background:#f6f8fb;padding:16px;"><table style="max-width:720px;width:100%;margin:auto;border-collapse:collapse;background:#fff;border:1px solid #d9e2f1;"><tr><td colspan="2" style="background:#1f4e79;color:#fff;padding:12px 14px;font-size:18px;font-weight:bold;">Reconciliation Alert Notification</td></tr><tr><td colspan="2" style="padding:12px 14px;border-bottom:1px solid #e6ebf2;">A record requiring attention has been detected during the reconciliation process.</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;width:220px;"><b>Alert Type</b></td><td style="padding:8px 14px;">@@alerttype</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Severity</b></td><td style="padding:8px 14px;">@@alertseverity</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Raised At</b></td><td style="padding:8px 14px;">@@raisedat</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Summary</b></td><td style="padding:8px 14px;">@@summary</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Evaluation</b></td><td style="padding:8px 14px;">@@evaluationstatus - @@evaluationmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Operation</b></td><td style="padding:8px 14px;">@@operationcode - @@operationstatus - @@operationnote</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>Latest Execution</b></td><td style="padding:8px 14px;">@@lastexecutionstatus - @@lastresultmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;color:#b42318;"><b>Error</b></td><td style="padding:8px 14px;color:#b42318;">@@error</td></tr><tr><td colspan="2" style="padding:10px 14px;background:#fbfcfe;border-top:1px solid #e6ebf2;"><b>Technical Details</b><div style="margin-top:6px;white-space:pre-wrap;font-family:Consolas,monospace;font-size:12px;color:#444;">@@detailmessage</div></td></tr></table></body></html>',
        N'',
        GETDATE(),
        NULL,
        N'019c5665-c7d5-7ea8-a5fc-d386d416e92a',
        NULL,
        N'Active',
        N'Reconciliation'
    );
END;