IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'TriggeredRuleSetKey'
      AND COLUMN_NAME = 'ComplianceRuleSetKey'
)
BEGIN
    ALTER TABLE Core.TriggeredRuleSetKey ADD ComplianceRuleSetKey VARCHAR(50);
END
GO