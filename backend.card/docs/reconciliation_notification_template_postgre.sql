DO
$$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM core.notification_template
        WHERE name = 'ReconciliationAlertTemplate'
          AND type = 'Email'
    ) THEN
UPDATE core.notification_template
SET
    originator = 'Payify',
    header = '@@emailsubject',
    body = '<html><body style="font-family:Arial,sans-serif;font-size:13px;color:#222;background:#f6f8fb;padding:16px;"><table style="max-width:720px;width:100%;margin:auto;border-collapse:collapse;background:#fff;border:1px solid #d9e2f1;"><tr><td colspan="2" style="background:#1f4e79;color:#fff;padding:12px 14px;font-size:18px;font-weight:bold;">@@templatetitle</td></tr><tr><td colspan="2" style="padding:12px 14px;border-bottom:1px solid #e6ebf2;">@@templatedescription</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;width:220px;"><b>@@labelalerttype</b></td><td style="padding:8px 14px;">@@alerttype</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labelseverity</b></td><td style="padding:8px 14px;">@@alertseverity</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labelraisedat</b></td><td style="padding:8px 14px;">@@raisedat</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labelsummary</b></td><td style="padding:8px 14px;">@@summary</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labelevaluation</b></td><td style="padding:8px 14px;">@@evaluationstatus - @@evaluationmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labeloperation</b></td><td style="padding:8px 14px;">@@operationcode - @@operationstatus - @@operationnote</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labellatestexecution</b></td><td style="padding:8px 14px;">@@lastexecutionstatus - @@lastresultmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;color:#b42318;"><b>@@labelerror</b></td><td style="padding:8px 14px;color:#b42318;">@@error</td></tr><tr><td colspan="2" style="padding:10px 14px;background:#fbfcfe;border-top:1px solid #e6ebf2;"><b>@@labeltechnicaldetails</b><div style="margin-top:6px;white-space:pre-wrap;font-family:Consolas,monospace;font-size:12px;color:#444;">@@detailmessage</div></td></tr></table></body></html>',
    channel = '',
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    category = 'Reconciliation'
WHERE name = 'ReconciliationAlertTemplate'
  AND type = 'Email';
ELSE
        INSERT INTO core.notification_template
        (
            id, type, name, originator, header, body, channel,
            create_date, update_date, created_by, last_modified_by, record_status, category
        )
        VALUES
        (
            gen_random_uuid(),
            'Email',
            'ReconciliationAlertTemplate',
            'Payify',
            '@@emailsubject',
            '<html><body style="font-family:Arial,sans-serif;font-size:13px;color:#222;background:#f6f8fb;padding:16px;"><table style="max-width:720px;width:100%;margin:auto;border-collapse:collapse;background:#fff;border:1px solid #d9e2f1;"><tr><td colspan="2" style="background:#1f4e79;color:#fff;padding:12px 14px;font-size:18px;font-weight:bold;">@@templatetitle</td></tr><tr><td colspan="2" style="padding:12px 14px;border-bottom:1px solid #e6ebf2;">@@templatedescription</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;width:220px;"><b>@@labelalerttype</b></td><td style="padding:8px 14px;">@@alerttype</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labelseverity</b></td><td style="padding:8px 14px;">@@alertseverity</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labelraisedat</b></td><td style="padding:8px 14px;">@@raisedat</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labelsummary</b></td><td style="padding:8px 14px;">@@summary</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labelevaluation</b></td><td style="padding:8px 14px;">@@evaluationstatus - @@evaluationmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labeloperation</b></td><td style="padding:8px 14px;">@@operationcode - @@operationstatus - @@operationnote</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;"><b>@@labellatestexecution</b></td><td style="padding:8px 14px;">@@lastexecutionstatus - @@lastresultmessage</td></tr><tr><td style="padding:8px 14px;background:#f3f6fa;color:#b42318;"><b>@@labelerror</b></td><td style="padding:8px 14px;color:#b42318;">@@error</td></tr><tr><td colspan="2" style="padding:10px 14px;background:#fbfcfe;border-top:1px solid #e6ebf2;"><b>@@labeltechnicaldetails</b><div style="margin-top:6px;white-space:pre-wrap;font-family:Consolas,monospace;font-size:12px;color:#444;">@@detailmessage</div></td></tr></table></body></html>',
            '',
            CURRENT_TIMESTAMP,
            NULL,
            '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
            NULL,
            'Active',
            'Reconciliation'
        );
END IF;
END
$$;